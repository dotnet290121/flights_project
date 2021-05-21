using BL.Exceptions;
using BL.Interfaces;
using BL.LoginService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BL
{
    public class LoggedInCustomerFacade : AnonymousUserFacade, ILoggedInCustomerFacade
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private object key = new object();

        public LoggedInCustomerFacade() : base()
        {
        }

        public void CancelTicket(LoginToken<Customer> token, Ticket ticket)
        {
            Execute(() =>
            {
                if (token.User != ticket.Customer)
                    throw new WrongCustomerException($"Customer {token.User.User.UserName} not allowed to cancel ticket {ticket.Id} that belogns to customer with id {ticket.Customer.Id}");

                _ticketDAO.Remove(ticket);

                Flight flight = _flightDAO.Get(ticket.Flight.Id);
                flight.RemainingTickets++;//maybe add this to the procedure of the cancel
                _flightDAO.Update(flight);

                _flightsTicketsHistoryDAO.Add(ticket, TicketStatus.Cancelled_By_Customer);
            }, new { Token = token, Ticket = ticket }, _logger);
        }

        public IList<Flight> GetAllMyFlights(LoginToken<Customer> token)
        {
            IList<Flight> result = null;

            result = Execute(() => _flightDAO.GetFlightsByCustomer(token.User), new { Token = token }, _logger);

            return result;
        }

        public IList<Ticket> GetAllMyTickets(LoginToken<Customer> token)
        {
            IList<Ticket> result = null;

            result = Execute(() => _ticketDAO.GetTicketsByCustomer(token.User), new { Token = token }, _logger);

            return result;
        }

        public Ticket GetTicketById(LoginToken<Customer> token, long id)
        {
            Ticket result = null;

            result = Execute(() =>
            {
                result = _ticketDAO.Get(id);

                if (result != null && token.User != result.Customer)
                    throw new WrongCustomerException($"Customer {token.User.User.UserName} not allowed to get details of ticket {result.Id} that belogns to customer with id {result.Customer.Id}");

                return result;
            }, new { Token = token, Id = id }, _logger);

            return result;
        }

        public TicketHistory GetTicketHistoryByOriginalId(LoginToken<Customer> token, long original_id)
        {
            TicketHistory result = null;

            result = Execute(() =>
            {
                result = _flightsTicketsHistoryDAO.GetTicketHistory(original_id);


                if (result != null && token.User.Id != result.CustomerId)
                    throw new WrongCustomerException($"Customer {token.User.User.UserName} not allowed to get details of ticket history {result.Id} that belogns to customer with id {result.CustomerId}");

                return result;

            }, new { Token = token, OriginalId = original_id }, _logger);

            return result;
        }

        public Ticket PurchaseTicket(LoginToken<Customer> token, Flight flight)
        {
            Ticket result = null;

            result = Execute(() =>
            {
                Flight flight_from_db = _flightDAO.Get(flight.Id);

                if (flight_from_db.RemainingTickets <= 0)//If there are no tickets left throw exception
                    throw new TicketPurchaseFailedException($"User {token.User.User.UserName} failed to purchase ticket to flight {flight.Id}. No tickets left",PurchaseFailReason.No_Tickets_Left);

                if (flight_from_db.DepartureTime <= DateTime.Now.AddMinutes(15))//If there are no tickets left throw exception
                    throw new TicketPurchaseFailedException($"User {token.User.User.UserName} failed to purchase ticket to flight {flight.Id}. No tickets left", PurchaseFailReason.Flight_Took_Off);

                //maybe add this to the procedure of add ticket
                flight_from_db.RemainingTickets--;//Remove one ticket from the remaining tickets
                _flightDAO.Update(flight_from_db);//Update the flight


                Ticket ticket = new Ticket(flight_from_db, token.User);//Create new ticket
                long ticket_id = _ticketDAO.Add(ticket);//Add the ticket
                ticket.Id = ticket_id;

                return ticket;
            }, new { Token = token, Flight = flight }, _logger);

            return result;
        }
    }
}
