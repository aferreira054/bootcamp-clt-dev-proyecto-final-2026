# CQRS Cuenta - Clean Architecture

API REST de gestión de cuentas bancarias (CRUD + movimientos de saldo). Es el trabajo final del bootcamp, aplicando Clean Architecture y CQRS con MediatR sobre .NET 10. Incluye persistencia con EF Core/PostgreSQL, despliegue en Kubernetes con Helm, configuración externalizada, logging estructurado con Seq y un pipeline de CI/CD en GitHub Actions.

## Stack

| Capa          | Tecnología                                      |
| ------------- | ------------------------------------------------ |
| API           | ASP.NET Core 10, Swagger/OpenAPI (Swashbuckle)   |
| CQRS          | MediatR (commands/queries + pipeline behaviors)  |
| Validación    | FluentValidation                                 |
| Mapeo         | AutoMapper                                       |
| Persistencia  | EF Core 10 + Npgsql (PostgreSQL 16)              |
| Tests         | xUnit                                            |
| Logging       | Serilog (estructurado) + Seq (centralización)    |
| Contenedores  | Docker (multistage, Alpine) + Docker Compose     |
| Orquestación  | Kubernetes + Helm (chart en `helm/cqrs-cuenta`)  |
| CI/CD         | GitHub Actions                                   |

## Arquitectura

```
src/
├── CleanArchitecture.Full.Domain          -> Entidades, enums, reglas de negocio (sin dependencias externas)
├── CleanArchitecture.Full.Application     -> Commands/Queries (CQRS), validators, behaviors, DTOs, interfaces
├── CleanArchitecture.Full.Infrastructure  -> EF Core, DbContext, migraciones, configuración de entidades
└── CleanArchitecture.Full.Api             -> Controllers, Program.cs, manejo global de errores, Swagger
tests/
└── CleanArchitecture.Full.Application.Tests -> tests unitarios (validators + reglas de dominio)
helm/
└── cqrs-cuenta -> chart de Helm (Deployment, StatefulSet, Services, ConfigMap, Secret)
```

La regla de dependencias es la clásica de Clean Architecture: `Api` depende de `Application` e `Infrastructure`, `Infrastructure` depende de `Application`, `Application` depende de `Domain`. El dominio no depende de nada.

Es CQRS "lite": los comandos y las queries están separados como objetos de MediatR con sus propios handlers, pero comparten el mismo DbContext y la misma base. No hay bases de lectura/escritura separadas a propósito - para un CRUD de este tamaño, separar los modelos físicamente hubiera sido sobre-ingeniería sin ningún beneficio real.

### Pipeline de MediatR

Cada Command/Query pasa por dos behaviors antes de llegar a su handler:

1. `LoggingBehavior`: loguea en Debug el payload completo del request, y en Information el resultado con su tiempo de ejecución.
2. `ValidationBehavior`: corre los validators de FluentValidation registrados. Si algo falla, corta la cadena con una `ValidationException` y el handler ni se llega a ejecutar.

### Manejo de errores

Un `GlobalExceptionHandler` (la interfaz `IExceptionHandler` de .NET 8+) centraliza todas las excepciones que no atrapan los controllers y las traduce a `ProblemDetails` / `ValidationProblemDetails` (RFC 7807), sin filtrar detalles internos como stack traces al cliente:

| Excepción | Status |
| --- | --- |
| `FluentValidation.ValidationException` | 400, con el detalle por campo |
| `ConflictException` (recurso duplicado) | 409 |
| `InvalidOperationException` (regla de negocio del dominio) | 409 |
| No encontrado (lo maneja el controller directamente) | 404 |
| Cualquier otra excepción no controlada | 500, mensaje genérico al cliente pero el detalle real queda logueado |

## Persistencia

La entidad de dominio es `Cuenta` (`src/CleanArchitecture.Full.Domain/Entities/Cuenta.cs`), con las reglas de negocio encapsuladas en métodos como `ActualizarSaldo` o `ActualizarEstado`. El constructor es privado y los setters también, así que la única forma de mutarla es a través de esos métodos.

Para la base uso EF Core 10 con el proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`, apuntando a PostgreSQL 16. Hay dos migraciones versionadas en `src/CleanArchitecture.Full.Infrastructure/Migrations/`: `InitialCreate` y `AddMotivoCancelacion`. Se aplican solas al arrancar la API con `Database.MigrateAsync()`.

El CRUD está completo (GET lista paginada, GET por id, POST, PUT, DELETE lógico, PATCH de saldo) y vive en `CuentasController`, que nunca recibe ni devuelve la entidad `Cuenta` directamente: solo DTOs (`CuentaResumenDto`, `CuentaDetalleDto`, commands de MediatR). Toda la entrada pasa por FluentValidation antes de llegar al handler.

## Cómo correr el proyecto

### Prerrequisitos

- Docker Desktop instalado y corriendo.
- .NET SDK 10, si vas a correr sin Docker o los tests. Se verifica con `dotnet --version`.
- Para la parte de Kubernetes: minikube, kubectl y Helm.

### Opción A: Docker Compose (la más rápida para probar)

Desde la raíz del proyecto, donde está `docker-compose.yml`:

```bash
docker compose up --build -d
```

Tarda unos segundos: Postgres arranca, la API espera a que esté healthy, aplica sus migraciones y siembra 4 cuentas de ejemplo.

```bash
docker compose ps
```

Los tres servicios (postgres, seq, api) tienen que figurar como running/healthy.

| Servicio | URL |
| --- | --- |
| API | http://localhost:8080/api/v1/cuentas |
| Swagger | http://localhost:8080/swagger |
| Seq (logs en vivo) | http://localhost:8081 |
| PostgreSQL | localhost:5433 |

Si querés cambiar usuario/password/db, copiá `.env.example` a `.env` y ajustá ahí.

```bash
docker compose logs -f api     # logs en vivo
docker compose down            # para los contenedores, conserva los volumes
docker compose down -v         # también borra los volumes
```

### Opción B: local sin Docker

Necesita un PostgreSQL corriendo en `localhost:5433` (o ajustar `ConnectionStrings:DefaultConnection` en `appsettings.Development.json`).

```bash
dotnet run --project src/CleanArchitecture.Full.Api
```

### Correr los tests

```bash
dotnet test CleanArchitecture.Full.sln
```

`tests/CleanArchitecture.Full.Application.Tests` cubre el validator de `CreateCuenta` y las reglas de dominio de `Cuenta` (depósitos, retiros con saldo insuficiente, cancelación con y sin saldo, etc). Son 16 tests en total.

## Observabilidad / Logging

Serilog se configura desde `Program.cs` con dos sinks: consola, que siempre está activo, y Seq, que solo se activa si `Seq:ServerUrl` está configurado. Esto último es a propósito, para que el proyecto ande sin problema con `dotnet run` local sin depender de que Seq esté levantado.

Los logs quedan enriquecidos con `MachineName` (el nombre del pod en K8s), `EnvironmentName`, `ProcessId`, `ThreadId` y `Application`.

### RequestId y correlación

Hay un middleware chico en `Program.cs` que toma el `TraceIdentifier` de ASP.NET Core y lo empuja al `LogContext` de Serilog como propiedad `RequestId`, antes de que el request entre al pipeline de MediatR:

```csharp
app.Use(async (context, next) =>
{
    using (Serilog.Context.LogContext.PushProperty("RequestId", context.TraceIdentifier))
    {
        await next();
    }
});
```

Con esto, todos los logs de un mismo request HTTP (el log de acceso, los del `LoggingBehavior` de MediatR, los de EF Core al abrir/cerrar conexión) comparten el mismo `RequestId`, sin importar en qué réplica del pod se ejecutaron.

### Los cuatro niveles de severidad

| Nivel | Dónde aparece | Ejemplo |
| --- | --- | --- |
| Debug | `LoggingBehavior` (payload completo del request), EF Core (ciclo de vida de la conexión) | `Procesando GetCuentasQuery {@Request}` |
| Information | Cada request HTTP y cada Command/Query completado | `GetCuentasQuery completado en 5 ms` |
| Warning | `GlobalExceptionHandler`: validaciones fallidas (400) y violaciones de reglas de negocio (409) | `ValidationException al procesar CreateCuentaCommand` |
| Error | `GlobalExceptionHandler`: excepciones no controladas (500) | Excepción no esperada, con stack trace completo |

El nivel mínimo (`Serilog:MinimumLevel:Default`) se puede parametrizar por entorno desde el ConfigMap de Helm: Debug en `values-dev.yaml` para poder ver los 4 niveles, Information en `values-qa.yaml` para tener menos ruido, más parecido a como estaría en producción.

### Demo real: correlación de logs entre réplicas de la API

Con el chart de Helm desplegado en dev (ver la sección de Kubernetes) y 4 réplicas de la API arriba, generé tráfico contra el Service desde un pod dentro del cluster. Uso esto en vez de `kubectl port-forward` porque port-forward fija una sola réplica para toda la sesión, no balancea de verdad entre pods como lo hace kube-proxy:

```bash
kubectl run curltest -n cqrs-dev --image=curlimages/curl:8.10.1 --restart=Never -- sleep 3600
kubectl exec -n cqrs-dev curltest -- sh -c \
  'for i in $(seq 1 30); do curl -s -o /dev/null http://cqrs-dev-api:8080/api/v1/cuentas; done'
```

Consultando Seq (vía su API HTTP, con port-forward a `svc/cqrs-dev-seq` puerto 80) filtrando por una sola propiedad (`RequestName = 'GetCuentasQuery'`), los resultados vinieron de 4 pods distintos. Eso confirma que Seq centraliza y correlaciona logs de todas las réplicas en un mismo lugar:

```bash
curl -s "http://localhost:15341/api/events?count=100&filter=RequestName%20%3D%20'GetCuentasQuery'" \
  | grep -o '"MachineName","Value":"[^"]*"' | sort | uniq -c
```

```
     18 "MachineName","Value":"cqrs-dev-api-845bf69fd9-b7jww"
      2 "MachineName","Value":"cqrs-dev-api-845bf69fd9-dl5td"
     16 "MachineName","Value":"cqrs-dev-api-845bf69fd9-pc8h6"
     64 "MachineName","Value":"cqrs-dev-api-845bf69fd9-zk2rw"
```

Filtrando por un `RequestId` puntual (`RequestId = '0HNO0C3RA0EG0:00000001'`) se ve la correlación dentro de un mismo request: todo con el mismo `MachineName` (un request HTTP siempre lo atiende una sola réplica), en orden cronológico: el log de acceso HTTP, el log del `LoggingBehavior` con el resultado, y los logs Debug de EF Core abriendo y cerrando la conexión. Son 6 líneas de distintos `SourceContext`, todas unidas por una sola propiedad.

En la UI de Seq (http://localhost:8081 en Docker Compose, o `svc/cqrs-dev-seq-ui` en K8s) el mismo filtro se escribe `RequestName = 'GetCuentasQuery'` directo en la barra de búsqueda.

## Kubernetes con Helm

Lo que antes eran manifiestos sueltos en `k8s/` ahora está empaquetado como un chart de Helm en `helm/cqrs-cuenta/`, parametrizado con `values.yaml` (los defaults) más un archivo de overrides por entorno.

```
helm/cqrs-cuenta/
├── Chart.yaml
├── values.yaml       # defaults
├── values-dev.yaml    # replicaCount=2, logLevel=Debug, ASPNETCORE_ENVIRONMENT=Development
├── values-qa.yaml     # replicaCount=3, logLevel=Information, ASPNETCORE_ENVIRONMENT=Staging
└── templates/
    ├── configmap.yaml  # no sensible: ASPNETCORE_ENVIRONMENT, nivel de log, Seq URL
    ├── secret.yaml      # sensible: credenciales de Postgres + connection string
    ├── postgres.yaml    # StatefulSet + Service headless + volumeClaimTemplates
    ├── seq.yaml         # Deployment + Service + PVC
    └── api.yaml         # Deployment (2+ réplicas, resources, readinessProbe) + Service
```

El ConfigMap y el Secret se inyectan en el pod de la API con `envFrom` (`configMapRef` + `secretRef`), no variable por variable. Así cualquier clave nueva que se agregue queda disponible automáticamente sin tocar el manifiesto del Deployment. No hay ningún valor hardcodeado en el código: `appsettings.json` solo trae defaults de desarrollo local sin contenedores.

### 1. Arrancar el cluster

```bash
minikube start
```

### 2. Construir la imagen y cargarla en minikube

El cluster de minikube tiene su propio Docker interno, separado del de Windows:

```bash
docker build -t cqrs-cuenta-api:latest .
minikube image load cqrs-cuenta-api:latest
```

### 3. Desplegar con Helm (dev)

```bash
helm install cqrs-dev helm/cqrs-cuenta -n cqrs-dev --create-namespace -f helm/cqrs-cuenta/values-dev.yaml
```

### 4. Desplegar con Helm (qa, opcional, en paralelo, en otro namespace)

```bash
helm install cqrs-qa helm/cqrs-cuenta -n cqrs-qa --create-namespace -f helm/cqrs-cuenta/values-qa.yaml
```

### 5. Verificar

```bash
kubectl get pods,svc,statefulset,pvc -n cqrs-dev
```

Esta es la salida real de la demo (namespace `cqrs-dev`, 2 réplicas de API por el default de `values-dev.yaml`):

```
NAME                                READY   STATUS    RESTARTS      AGE
pod/cqrs-dev-api-845bf69fd9-scpf8   1/1     Running   0             30s
pod/cqrs-dev-api-845bf69fd9-zk2rw   1/1     Running   1 (14s ago)   30s
pod/cqrs-dev-postgres-0             1/1     Running   0             30s
pod/cqrs-dev-seq-74fb5d6dd6-w4wds   1/1     Running   0             30s

NAME                        TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)           AGE
service/cqrs-dev-api        NodePort    10.107.171.103   <none>        8080:30082/TCP    30s
service/cqrs-dev-postgres   ClusterIP   None             <none>        5432/TCP          30s
service/cqrs-dev-seq        ClusterIP   10.110.113.194   <none>        5341/TCP,80/TCP   30s
service/cqrs-dev-seq-ui     NodePort    10.110.117.145   <none>        80:30081/TCP      30s

NAME                                 READY   AGE
statefulset.apps/cqrs-dev-postgres   1/1     30s

NAME                                                         STATUS   CAPACITY   ACCESS MODES
persistentvolumeclaim/cqrs-dev-seq-pvc                       Bound    1Gi        RWO
persistentvolumeclaim/postgres-storage-cqrs-dev-postgres-0   Bound    1Gi        RWO
```

Vale la pena mirar el nombre del PVC (`postgres-storage-cqrs-dev-postgres-0`): lo generó automáticamente el `volumeClaimTemplates` del StatefulSet, con el índice ordinal del pod (`-0`) incluido en el nombre. Esa es justamente la identidad estable que un Deployment común no da.

### 6. Acceder a la API y a Seq

```bash
kubectl port-forward -n cqrs-dev svc/cqrs-dev-api 8080:8080
kubectl port-forward -n cqrs-dev svc/cqrs-dev-seq-ui 8081:80
```

http://localhost:8080/swagger y http://localhost:8081.

### 7. Demo: autorecuperación (borrar un Pod)

```bash
kubectl get pods -n cqrs-dev -l app=cqrs-dev-api
kubectl delete pod cqrs-dev-api-845bf69fd9-scpf8 -n cqrs-dev
kubectl get pods -n cqrs-dev -l app=cqrs-dev-api
```

Salida real:

```
=== ANTES ===
NAME                            READY   STATUS    RESTARTS      AGE
cqrs-dev-api-845bf69fd9-scpf8   1/1     Running   0             37s
cqrs-dev-api-845bf69fd9-zk2rw   1/1     Running   1 (21s ago)   37s

pod "cqrs-dev-api-845bf69fd9-scpf8" deleted

=== INMEDIATAMENTE DESPUES ===
NAME                            READY   STATUS     RESTARTS      AGE
cqrs-dev-api-845bf69fd9-pc8h6   0/1     Init:0/1   0             1s     <- pod nuevo, lo creó el ReplicaSet
cqrs-dev-api-845bf69fd9-zk2rw   1/1     Running    1 (23s ago)   39s

=== DESPUES DE RECUPERARSE (14s más tarde) ===
NAME                            READY   STATUS    RESTARTS      AGE
cqrs-dev-api-845bf69fd9-pc8h6   1/1     Running   0             14s
cqrs-dev-api-845bf69fd9-zk2rw   1/1     Running   1 (36s ago)   52s
```

El Deployment define `replicas: 2`. En cuanto uno murió, el ReplicaSet que lo administra creó `cqrs-dev-api-845bf69fd9-pc8h6` para volver al número declarado, sin que nadie tuviera que intervenir.

### 8. Demo: escalado declarativo

```bash
helm upgrade cqrs-dev helm/cqrs-cuenta -n cqrs-dev -f helm/cqrs-cuenta/values-dev.yaml --set api.replicaCount=4
kubectl get pods -n cqrs-dev -l app=cqrs-dev-api
```

Salida real:

```
Release "cqrs-dev" has been upgraded. Happy Helming!
REVISION: 2

NAME                            READY   STATUS    RESTARTS      AGE
cqrs-dev-api-845bf69fd9-b7jww   1/1     Running   0             15s
cqrs-dev-api-845bf69fd9-dl5td   1/1     Running   0             15s
cqrs-dev-api-845bf69fd9-pc8h6   1/1     Running   0             38s
cqrs-dev-api-845bf69fd9-zk2rw   1/1     Running   1 (60s ago)   76s
```

Cambiar `replicaCount` (o cualquier otro valor) en el values.yaml y correr `helm upgrade` es escalado declarativo: se declara el estado deseado y Kubernetes converge hacia él. Es distinto de `kubectl scale`, que es imperativo y no queda registrado en el chart.

### 9. Apagar

```bash
helm uninstall cqrs-dev -n cqrs-dev
kubectl delete namespace cqrs-dev
minikube stop
```

### Nota sobre puertos

Los Service tipo NodePort usan por defecto 30082 (api) y 30081 (seq-ui) en dev, y 30092/30091 en qa, para que ambos entornos puedan convivir en el mismo cluster sin chocar. Si algún puerto ya está en uso, se edita `nodePort` en el `values-*.yaml` correspondiente (el rango válido es 30000-32767).

## Configuración

Nada de configuración sensible o dependiente del entorno está hardcodeada en el código. `appsettings.json` solo trae defaults para desarrollo local sin contenedores.

| Fuente | Contenido | Ejemplo |
| --- | --- | --- |
| ConfigMap (`templates/configmap.yaml`) | No sensible | `ASPNETCORE_ENVIRONMENT`, `Serilog__MinimumLevel__Default`, `Seq__ServerUrl` |
| Secret (`templates/secret.yaml`) | Sensible | `POSTGRES_USER`, `POSTGRES_PASSWORD`, `ConnectionStrings__DefaultConnection` |

Los valores concretos (usuario/password de Postgres, cantidad de réplicas, resources, nivel de log, `ASPNETCORE_ENVIRONMENT`) están parametrizados en `values.yaml` y se overridean por entorno:

| | dev | qa |
| --- | --- | --- |
| ASPNETCORE_ENVIRONMENT | Development | Staging |
| Nivel de log | Debug | Information |
| Réplicas de API | 2 | 3 |
| Storage de Postgres | 1Gi | 2Gi |

## CI/CD

El workflow está en `.github/workflows/ci-cd.yml`, con triggers `push`, `pull_request` y `workflow_dispatch` (así también se puede disparar a mano desde la pestaña Actions).

El job `validacion` corre siempre, en este orden:

```
checkout -> setup .NET -> restore -> build (--no-restore) -> test (--no-build)
```

El job `empaquetado` depende de `validacion` (`needs: validacion`) y solo corre en push a main/master, no en PRs. Hace build y push de la imagen a GitHub Container Registry (ghcr.io).

Sobre Variables y Secrets: `IMAGE_NAME` es una variable no sensible (Settings > Actions > Variables), con default `cqrs-cuenta-api` si no está seteada. `GITHUB_TOKEN` es el secret, provisto automáticamente por Actions, y se usa solo para el login en ghcr.io. No se imprime en los logs, el step de login de docker/login-action lo enmascara.

### Demo: un PR con un test roto a propósito

Abrí el PR #1 (rama `test/ci-fallo-intencional` contra `master`): https://github.com/aferreira054/bootcamp-clt-dev-proyecto-final-2026/pull/1

Primero el commit que rompe el test a propósito (`c5de1d1`): cambié el valor esperado en `CuentaTests.Deposito_incrementa_el_saldo` de 150m a 140m. El job `validacion` corrió y el step de test falló, check en rojo:

> Run 32584279573: "1 failing, 1 skipped checks"

```
[xUnit.net 00:00:00.29]     CleanArchitecture.Full.Application.Tests.Domain.CuentaTests.Deposito_incrementa_el_saldo [FAIL]
  Error Message:
   Assert.Equal() Failure: Values differ
Expected: 140
Actual:   150
     at CleanArchitecture.Full.Application.Tests.Domain.CuentaTests.Deposito_incrementa_el_saldo() in .../CuentaTests.cs:line 19

Failed!  - Failed:     1, Passed:    15, Skipped:     0, Total:    16, Duration: 100 ms
Error: Process completed with exit code 1.
```

El job `empaquetado` quedó skipped, porque depende de `needs: validacion` y ni intentó correr con el build roto.

Después el commit que corrige el test (`92a5204`): vuelvo el valor a 150m y lo pusheo a la misma rama. El workflow corre de nuevo solo, y esta vez el check queda verde:

> Run 32584366514: "All checks have passed, 1 skipped, 1 successful checks", CI/CD / Validacion (restore -> build -> test), successful en 22s

```
Passed!  - Failed:     0, Passed:    16, Skipped:     0, Total:    16, Duration: 69 ms - CleanArchitecture.Full.Application.Tests.dll (net10.0)
```

El job `empaquetado` sigue en skipped en ambos runs porque el trigger es un pull_request, no un push a main/master. El PR se mergeó a `master` el 22/08/2026, y ese push sí disparó el job de empaquetado.

## Notas de diseño

Baja lógica, no física: `DELETE /api/v1/cuentas/{id}` cambia el estado a Cancelada (pide motivo y saldo en cero) en vez de borrar la fila, así se puede auditar después.

PUT no permite cancelar cuentas: el validator de UpdateCuenta bloquea explícitamente `Estado=Cancelada`. Esa transición de ciclo de vida es responsabilidad exclusiva de DELETE.

Migraciones al arranque: la API aplica `Database.MigrateAsync()` al iniciar. Con múltiples réplicas (como pasa en K8s), todas corren la migración al arrancar, pero EF Core la protege con un lock a nivel de base así que es seguro. De todas formas, en un escenario de más escala convendría moverlo a un Job o init step separado en vez de dejarlo en el arranque de cada réplica.

LimiteCredito requerido para cuentas de crédito: el validator original solo tenía `GreaterThan(0)`, y en FluentValidation eso no falla sobre un `decimal?` nulo. En la práctica dejaba crear cuentas de crédito sin límite, aunque el mensaje de error decía "requerido". Le agregué un `NotNull()` explícito, y quedó cubierto con un test.

El Dockerfile restaura solo el proyecto de la API, no el .sln completo, así la imagen de producción no arrastra el proyecto de tests ni sus paquetes.
