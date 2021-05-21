using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class AirlineCompanyProfile : Profile
    {
        public AirlineCompanyProfile()
        {
            CreateMap<CreateAirlineCompanyDTO, AirlineCompany>();
            CreateMap<UpdateAirlineCompanyDTO, AirlineCompany>();
            CreateMap<AirlineCompany, AirlineCompanyDetailsDTO>();
        }
    }
}
