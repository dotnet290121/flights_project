using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class AdministratorProfile: Profile
    {
        public AdministratorProfile()
        {
            CreateMap<CreateAdministratorDTO, Administrator>();
            CreateMap<UpdateAdministratorDTO, Administrator>();
            CreateMap<Administrator, AdministratorDetailsDTO>();
        }
    }
}
