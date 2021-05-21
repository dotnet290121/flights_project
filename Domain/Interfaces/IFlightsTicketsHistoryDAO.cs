using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IFlightsTicketsHistoryDAO
    {
        FlightHistory GetFlightHistory(long original_flight_id);
        TicketHistory GetTicketHistory(long original_ticket_id);
        void Add(Flight flight, FlightStatus status);
        void Add(Ticket ticket, TicketStatus status);

    }
}
