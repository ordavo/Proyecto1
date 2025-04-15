using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicios
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>().ReverseMap();
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();   
        }
    }
}
