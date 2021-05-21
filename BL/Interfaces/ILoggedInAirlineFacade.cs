using BL.LoginService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Interfaces
{
    public interface ILoggedInAirlineFacade:IAnonymousUserFacade
    {
        IList<Ticket> GetAllTickets(LoginToken<AirlineCompany> token);
        IList<Ticket> GetAllTicketsByFlight(LoginToken<AirlineCompany> token, Flight flight);
        IList<Flight> GetAllFlights(LoginToken<AirlineCompany> token);
        void CancelFlight(LoginToken<AirlineCompany> token, Flight flight);
        long CreateFlight(LoginToken<AirlineCompany> token, Flight flight);
        void UpdateFlight(LoginToken<AirlineCompany> token, Flight flight);
        void MofidyAirlineDetails(LoginToken<AirlineCompany> token, AirlineCompany airline);
        FlightHistory GetFlightHistoryByOriginalId(LoginToken<AirlineCompany> token, long original_id);

    }
}
