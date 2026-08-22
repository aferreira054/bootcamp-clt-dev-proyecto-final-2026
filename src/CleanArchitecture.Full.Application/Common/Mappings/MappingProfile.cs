// src/CleanArchitecture.Full.Application/Common/Mappings/MappingProfile.cs
using AutoMapper;
using CleanArchitecture.Full.Application.DTOs;
using CleanArchitecture.Full.Domain.Entities;

namespace CleanArchitecture.Full.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Cuenta, CuentaResumenDto>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.Moneda, opt => opt.MapFrom(src => src.Moneda.ToString()))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .MaxDepth(5);

        CreateMap<Cuenta, CuentaDetalleDto>()
            .IncludeBase<Cuenta, CuentaResumenDto>();
    }
}