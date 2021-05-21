using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<CreateCustomerDTO, Customer>();
            CreateMap<UpdateCustomerDTO, Customer>();
            CreateMap<Customer, CustomerDetailsDTO>();
        }
    }
}
