using Domain.ExtentionMethods;
using DAL;
using Domain.Interfaces;
using log4net;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BL
{
    public abstract class FacadeBase
    {
        protected IAirlineDAO _airlineDAO;
        protected ICountryDAO _countryDAO;
        protected ICustomerDAO _customerDAO;
        protected IAdminDAO _adminDAO;
        protected IUserDAO _userDAO;
        protected IFlightDAO _flightDAO;
        protected ITicketDAO _ticketDAO;
        protected IFlightsTicketsHistoryDAO _flightsTicketsHistoryDAO;

        protected FacadeBase()
        {
            _airlineDAO = new AirlineDAOPGSQL();
            _countryDAO = new CountryDAOPGSQL();
            _customerDAO = new CustomerDAOPGSQL();
            _adminDAO = new AdminDAOPGSQL();
            _userDAO = new UserDAOPGSQL();
            _flightDAO = new FlightDAOPGSQL();
            _ticketDAO = new TicketDAOPGSQL();
            _flightsTicketsHistoryDAO = new FlightsTicketsHistoryDAOPGSQL();
        }

        protected T Execute<T>(Func<T> func, object props_holder, ILog _logger, [CallerMemberName] string callerName = "")
        {
            _logger.Debug($"Enter {callerName}({props_holder.GenerateString()})");

            T result = default;

            try
            {
                result = func.Invoke();
            }
            finally
            {
                if (result == null)
                    _logger.Debug($"Exit {callerName}. Result: null");
                
                else if (result.GetType().GetInterfaces().Contains(typeof(IEnumerable)))
                    _logger.Debug($"Exit {callerName}. Result: {((IEnumerable)result).BuildString()}");
                
                else
                    _logger.Debug($"Exit {callerName}. Result: {result}");
            }
            return result;
        }

        protected void Execute(Action action, object props_holder, ILog _logger, [CallerMemberName] string callerName = "")
        {
            _logger.Debug($"Enter {callerName}({props_holder.GenerateString()})");

            try
            {
                action.Invoke();
            }
            finally
            {
                _logger.Debug($"Exit {callerName}.");
            }
        }
    }
}
