{{- define "cqrs-cuenta.fullname" -}}
{{ .Release.Name }}
{{- end -}}

{{- define "cqrs-cuenta.labels" -}}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{- define "cqrs-cuenta.connectionString" -}}
Host={{ .Release.Name }}-postgres;Port=5432;Database={{ .Values.postgres.credentials.database }};Username={{ .Values.postgres.credentials.user }};Password={{ .Values.postgres.credentials.password }}
{{- end -}}
