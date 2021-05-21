using BL.Exceptions;
using BL.Interfaces;
using BL.LoginService;
using Domain.Entities;
using System.Collections.Generic;
using System.Reflection;

namespace BL
{
    public class LoggedInAirlineFacade : AnonymousUserFacade, ILoggedInAirlineFacade
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LoggedInAirlineFacade() : base()
        {
        }

        public void CancelFlight(LoginToken<AirlineCompany> token, Flight flight)
        {
            Execute(() =>
            {
                if (token.User != flight.AirlineCompany)
                    throw new NotAllowedAirlineActionException($"Airline company {token.User.Name} not allowed to cancel flight {flight.Id} that belongs to {flight.AirlineCompany.Name}");

                IList<Ticket> tickets = _ticketDAO.GetTicketsByFlight(flight);
                if (tickets.Count > 0)
                    foreach (var ticket in tickets)
                    {
                        _flightsTicketsHistoryDAO.Add(ticket, TicketStatus.Cancelled_By_Company);
                        _ticketDAO.Remove(ticket);
                    }

                _flightsTicketsHistoryDAO.Add(flight, FlightStatus.Cancelled_By_Company);
                _flightDAO.Remove(flight);
            }, new { Token = token, Flight = flight }, _logger);
        }

        public long CreateFlight(LoginToken<AirlineCompany> token, Flight flight)
        {
            long result = 0;

            result = Execute(() =>
            {
                flight.AirlineCompany = token.User;

                result = _flightDAO.Add(flight);
                return result;
            }, new { Token = token, Flight = flight }, _logger);

            return result;
        }

        public IList<Flight> GetAllFlights(LoginToken<AirlineCompany> token)
        {
            IList<Flight> result = null;

            result = Execute(() => _flightDAO.GetFlightsByAirlineCompany(token.User), new { Token = token }, _logger);

            return result;
        }

        public IList<Ticket> GetAllTickets(LoginToken<AirlineCompany> token)
        {
            IList<Ticket> result = null;

            result = Execute(() => _ticketDAO.GetTicketsByAirlineCompany(token.User), new { Token = token }, _logger);

            return result;
        }

        public IList<Ticket> GetAllTicketsByFlight(LoginToken<AirlineCompany> token, Flight flight)
        {
            IList<Ticket> result = null;

            result = Execute(() => {

                if (token.User != flight.AirlineCompany)
                    throw new NotAllowedAirlineActionException($"Airline company {token.User.Name} not allowed to get tickets of flight {flight.Id} that belongs to {flight.AirlineCompany.Name}");

                result = _ticketDAO.GetTicketsByFlight(flight);

                return result;
                
            }, new { Token = token, Flight = flight }, _logger);

            return result;
        }

        public FlightHistory GetFlightHistoryByOriginalId(LoginToken<AirlineCompany> token, long original_id)
        {
            FlightHistory result = null;

            result = Execute(() =>
            {
                result = _flightsTicketsHistoryDAO.GetFlightHistory(original_id);

                if (result != null && token.User.Id != result.AirlineCompanyId)
                    throw new NotAllowedAirlineActionException($"{token.User.Name} company not allowed to view the details of {result.AirlineCompanyName}'s cancelled flight id: {result.OriginalId}");

                return result;

            }, new { Token = token, OriginalId = original_id }, _logger);

            return result;
        }

        public void MofidyAirlineDetails(LoginToken<AirlineCompany> token, AirlineCompany airlineCompany)
        {
            Execute(() =>
            {
                if (token.User != airlineCompany)
                    throw new NotAllowedAirlineActionException($"{token.User.Name} company not allowed to modify the details of {airlineCompany.Name} company");

                _airlineDAO.Update(airlineCompany);
            }, new { Token = token, AirlineCompany = airlineCompany }, _logger);
        }

        public void UpdateFlight(LoginToken<AirlineCompany> token, Flight flight)
        {
            Execute(() =>
            {
                if (token.User != flight.AirlineCompany)
                    throw new NotAllowedAirlineActionException($"Airline company {token.User.Name} not allowed to update flight {flight.Id} that belongs to {flight.AirlineCompany.Name}");

                _flightDAO.Update(flight);
            }, new { Token = token, Flight = flight }, _logger);
        }
    }
}
