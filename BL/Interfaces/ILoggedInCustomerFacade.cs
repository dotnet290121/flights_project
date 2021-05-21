using BL.LoginService;
using Domain.Entities;
using System.Collections.Generic;

namespace BL.Interfaces
{
    public interface ILoggedInCustomerFacade: IAnonymousUserFacade
    {
        IList<Flight> GetAllMyFlights(LoginToken<Customer> token);
        IList<Ticket> GetAllMyTickets(LoginToken<Customer> token);
        Ticket GetTicketById(LoginToken<Customer> token, long id);
        Ticket PurchaseTicket(LoginToken<Customer> token, Flight flight);
        void CancelTicket(LoginToken<Customer> token, Ticket ticket);
        TicketHistory GetTicketHistoryByOriginalId(LoginToken<Customer> token, long original_id);
    }
}
