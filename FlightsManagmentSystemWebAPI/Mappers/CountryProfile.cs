using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class CountryProfile : Profile
    {
        public CountryProfile()
        {
            CreateMap<CreateCountryDTO, Country>();
            CreateMap<UpdateCountryDTO, Country>();
        }
    }
}
