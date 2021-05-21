using BL.LoginService;
using ConfigurationService;
using DAL;
using Domain.Entities;
using Domain.Interfaces;
using log4net;
using System;
using System.Reflection;
using System.Threading;

namespace BL
{
    public class FlightCenterSystem : IFlightCenterSystem
    {

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //Single instance of the class
        private static FlightCenterSystem _instance;
        //Key that is used to lock while creating the instance
        private static readonly object key = new object();
        //public login service in order to have access to the TryLogin method
        private readonly ILoginService loginService = new LoginService.LoginService();
        //Get the invokation time of the thread that will run the backup
        private static string _invokation_time = FlightsManagmentSystemConfig.Instance.WorkTime;


        private FlightCenterSystem()
        {
            //Create new thread and pass it a method to run
            Thread thread = new Thread(StoreFlightDetailsHistory);
            //Run the method
            thread.Start(_invokation_time);
        }

        /// <summary>
        /// Back-up method that will run each day at a certain time
        /// </summary>
        /// <param name="timeSpan">The time of the day the method will run</param>
        private static void StoreFlightDetailsHistory(object timeSpan)
        {
            //If the format of the invokation time is not correct the method will not run
            if (!TimeSpan.TryParse(timeSpan.ToString(), out TimeSpan ts))
            {
                _logger.Error("Time to backup flights details history is configured in wrong format");
                return;
            }

            //check how many seconds left till backup
            double secondsToGo = (ts - DateTime.Now.TimeOfDay).TotalSeconds;
            if (secondsToGo < 0)//if the seconds number is negative (meaning the tome is passed for today), add 24 hours
                secondsToGo += (24 * 60 * 60);

            Thread.Sleep(new TimeSpan(0, 0, (int)secondsToGo));//Sleeo till invokation time
            _logger.Debug($"Backup will start in {secondsToGo} seconds");

            while (true)//will run each 24 hours while the system us up
            {
                _logger.Info("Starting backup...");

                IFlightDAO flightDAO = new FlightDAOPGSQL();
                ITicketDAO ticketDAO = new TicketDAOPGSQL();
                IFlightsTicketsHistoryDAO flightsTicketsHistoryDAO = new FlightsTicketsHistoryDAOPGSQL();
                var flights_with_tickets = flightDAO.GetFlightsWithTicketsAfterLanding(3 * 60 * 60);//get all flight with tickets that landed 3+ hours ago inside dictionary
                int flights_count = 0;
                int tickets_count = 0;

                foreach (Flight flight in flights_with_tickets.Keys)//Run over all the keys (flights) in the dictionary
                {
                    flightsTicketsHistoryDAO.Add(flight, FlightStatus.Landed);//Add the flight to history table

                    foreach (Ticket ticket in flights_with_tickets[flight])//Run over all the tickets of the flight
                    {
                        if (ticket.Id != 0)//If there is no tickets associated with the flight there will be one ticket with id, we don't want to add this ticket to the history
                        {
                            flightsTicketsHistoryDAO.Add(ticket, TicketStatus.Redeemed);//Add ticket to history table
                            ticketDAO.Remove(ticket);//Remove the ticket from original table
                            tickets_count++;
                        }
                    }

                    flightDAO.Remove(flight);//Remove the flight from original table
                    flights_count++;
                }

                _logger.Info($"Backed up {flights_count} flights and {tickets_count} tickets");
                Thread.Sleep(new TimeSpan(24, 0, 0));
            }
        }

        public static FlightCenterSystem GetInstance()
        {

            if (_instance == null)
            {
                lock (key)
                {
                    if (_instance == null)
                    {
                        _instance = new FlightCenterSystem();
                    }
                }
            }

            return _instance;
        }

        public T GetFacade<T>() where T : FacadeBase, new()
        {
            return new T();
        }

        public bool TryLogin(string user_name, string password, out ILoginToken token, out FacadeBase facade)
        {
            token = null;
            facade = GetFacade<AnonymousUserFacade>();

            bool is_success = loginService.TryLogin(user_name, password, out ILoginToken login_token, out FacadeBase facade_base);

            if (is_success)
            {
                token = login_token;
                facade = facade_base;
            }

            return is_success;
        }
    }
}
