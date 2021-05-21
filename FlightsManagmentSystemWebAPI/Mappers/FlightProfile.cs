using AutoMapper;
using BL.CountriesDictionaryService;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class FlightProfile : Profile
    {
        private readonly CountriesManager _countriesManager = CountriesManager.Instance;

        public FlightProfile()
        {
            CreateMap<CreateFlightDTO, Flight>();
            CreateMap<UpdateFlightDTO, Flight>();
            CreateMap<Flight, FlightDetailsDTO>()
                .ForMember(dest => dest.OriginCountryName,
                            opt => opt.MapFrom(src =>  _countriesManager.GetCountryName(src.OriginCountryId)))
                .ForMember(dest => dest.DestinationCountryName,
                            opt => opt.MapFrom(src => _countriesManager.GetCountryName(src.DestinationCountryId)));
        }
    }
}
