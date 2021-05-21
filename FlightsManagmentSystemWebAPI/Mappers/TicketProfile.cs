using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Dtos;

namespace FlightsManagmentSystemWebAPI.Mappers
{
    public class TicketProfile:Profile
    {
        public TicketProfile()
        {
            CreateMap<Ticket, TicketDetailsDTO>();
        }
    }
}
