using BL;
using BL.Exceptions;
using BL.Interfaces;
using BL.LoginService;
using ConfigurationService;
using Domain.Entities;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BL_Tests
{
    [TestClass]
    public class LoggedInAirlineFacadeTests
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly FlightCenterSystem system = FlightCenterSystem.GetInstance();
        private LoggedInAirlineFacade airline_facade;
        private LoginToken<AirlineCompany> airline_token;

        private void Execute_Test(Action action, [CallerMemberName] string callerName = "")
        {
            _logger.Debug($"Run {callerName} Test");

            action.Invoke();

            _logger.Debug($"Exit {callerName} Test");
        }

        [TestInitialize]
        public void Initialize()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("Log4Net.config"));

            FlightsManagmentSystemConfig.Instance.Init("FlightsManagmentSystemTests.Config.json");

            TestsDAOPGSQL.ClearDB();
            Init_Airline_Facade_Data();
        }

        private void Init_Airline_Facade_Data()
        {
            _logger.Debug($"Start Init Airline Company Tests Data");

            string username = "admin";
            string password = "9999";
            system.TryLogin(username, password, out ILoginToken admin_token, out FacadeBase admin_facade);
            LoggedInAdministratorFacade loggedInAdministratorFacade = admin_facade as LoggedInAdministratorFacade;
            LoginToken<Administrator> adminLoginToken = admin_token as LoginToken<Administrator>;
            int country_id = loggedInAdministratorFacade.CreateNewCountry(adminLoginToken, TestData.Get_Countries_Data()[0]);
            loggedInAdministratorFacade.CreateNewCountry(adminLoginToken, TestData.Get_Countries_Data()[1]);
            loggedInAdministratorFacade.CreateNewCountry(adminLoginToken, TestData.Get_Countries_Data()[2]);
            loggedInAdministratorFacade.CreateNewCustomer(adminLoginToken, TestData.Get_Customers_Data()[0]);
            AirlineCompany airlineCompany = TestData.Get_AirlineCompanies_Data()[0];
            airlineCompany.CountryId = country_id;
            loggedInAdministratorFacade.CreateNewAirlineCompany(adminLoginToken, airlineCompany);
            Login();
            Flight flight = TestData.Get_Flights_Data()[3];
            Flight flight2 = TestData.Get_Flights_Data()[4];
            long flight_id = airline_facade.CreateFlight(airline_token, flight);
            long flight_id2 = airline_facade.CreateFlight(airline_token, flight2);
            flight.Id = flight_id;
            flight2.Id = flight_id2;

            system.TryLogin(TestData.Get_Customers_Data()[0].User.UserName, TestData.Get_Customers_Data()[0].User.Password, out ILoginToken customer_token, out FacadeBase customer_facade);
            LoggedInCustomerFacade loggedInCustomerFacade = customer_facade as LoggedInCustomerFacade;
            LoginToken<Customer> customerLoginToken = customer_token as LoginToken<Customer>;
            loggedInCustomerFacade.PurchaseTicket(customerLoginToken, flight);
            loggedInCustomerFacade.PurchaseTicket(customerLoginToken, flight2);

            _logger.Debug($"End Init Airline Company Tests Data");
        }

        private void Login()
        {
            system.TryLogin(TestData.Get_AirlineCompanies_Data()[0].User.UserName, TestData.Get_AirlineCompanies_Data()[0].User.Password, out ILoginToken token, out FacadeBase facade);
            airline_token = token as LoginToken<AirlineCompany>;
            airline_facade = facade as LoggedInAirlineFacade;
        }

        [TestMethod]
        public void Create_And_Get_New_Flight()
        {
            Execute_Test(() =>
            {
                Flight demi_flight = TestData.Get_Flights_Data()[0];
                long flight_id = airline_facade.CreateFlight(airline_token, demi_flight);
                Assert.AreEqual(flight_id, 3);
                demi_flight.Id = flight_id;
                Flight flight_from_db = airline_facade.GetFlightById((int)flight_id);

                TestData.CompareProps(flight_from_db, demi_flight, true);
            });
        }

        [TestMethod]
        public void Create_And_Get_List_Of_New_Flight()
        {
            Execute_Test(() =>
            {
                Flight[] data = TestData.Get_Flights_Data();
                Flight[] demi_flights = { data[0], data[1], data[2] };

                for (int i = 0; i < demi_flights.Length; i++)
                {
                    long flight_id = airline_facade.CreateFlight(airline_token, demi_flights[i]);
                    Assert.AreEqual(flight_id, i + 3);
                    demi_flights[i].Id = flight_id;
                }

                IList<Flight> flights_from_db = airline_facade.GetAllFlights();
                Assert.AreEqual(demi_flights.Length + 2, flights_from_db.Count);

                for (int i = 2; i < flights_from_db.Count; i++)
                {
                    TestData.CompareProps(flights_from_db[i], demi_flights[i - 2], true);
                }
            });
        }

        [TestMethod]
        public void Update_Flight()
        {
            Execute_Test(() =>
            {
                Flight demi_flight = TestData.Get_Flights_Data()[0];
                long flight_id = airline_facade.CreateFlight(airline_token, demi_flight);
                demi_flight.Id = flight_id;
                demi_flight.LandingTime = DateTime.Now.AddYears(1);
                demi_flight.LandingTime = DateTime.Now.AddYears(1).AddDays(1);
                demi_flight.OriginCountryId = 1;
                demi_flight.DestinationCountryId = 1;
                demi_flight.RemainingTickets = 0;

                airline_facade.UpdateFlight(airline_token, demi_flight);

                Flight flight_from_db = airline_facade.GetFlightById((int)flight_id);

                TestData.CompareProps(flight_from_db, demi_flight, true);
            });
        }

        [TestMethod]
        public void Remove_Flight()
        {
            Execute_Test(() =>
            {
                Flight demi_flight = TestData.Get_Flights_Data()[0];
                long flight_id = airline_facade.CreateFlight(airline_token, demi_flight);
                demi_flight.Id = flight_id;

                airline_facade.CancelFlight(airline_token, demi_flight);

                Assert.AreEqual(airline_facade.GetAllFlights(airline_token).Count, 2);

                FlightHistory flightHistory = airline_facade.GetFlightHistoryByOriginalId(airline_token, flight_id);
                Assert.AreEqual(flightHistory.Id, 1);
                Assert.AreEqual(flightHistory.AirlineCompanyId, demi_flight.AirlineCompany.Id);
                Assert.AreEqual(flightHistory.AirlineCompanyName, demi_flight.AirlineCompany.Name);
                TestData.CompareDates(flightHistory.DepartureTime, demi_flight.DepartureTime);
                TestData.CompareDates(flightHistory.LandingTime, demi_flight.LandingTime);
                Assert.AreEqual(flightHistory.OriginCountryId, demi_flight.OriginCountryId);
                Assert.AreEqual(flightHistory.DestinationCountryId, demi_flight.DestinationCountryId);
                Assert.AreEqual(flightHistory.RemainingTickets, demi_flight.RemainingTickets);
                Assert.AreEqual(flightHistory.FlightStatus, FlightStatus.Cancelled_By_Company);
            });
        }

        [TestMethod]
        public void Remove_Flight_With_Ticket()
        {
            Execute_Test(() =>
            {
                Flight demi_flight = TestData.Get_Flights_Data()[3];
                demi_flight.AirlineCompany = airline_token.User;
                demi_flight.Id = 1;

                airline_facade.CancelFlight(airline_token, demi_flight);

                Assert.AreEqual(airline_facade.GetAllFlights(airline_token).Count, 1);

                FlightHistory flightHistory = airline_facade.GetFlightHistoryByOriginalId(airline_token, demi_flight.Id);

                Assert.AreEqual(airline_facade.GetAllTicketsByFlight(airline_token, demi_flight).Count, 0);
                Assert.AreEqual(airline_facade.GetFlightHistoryByOriginalId(airline_token, demi_flight.Id).Id, 1);
            });
        }

        [TestMethod]
        public void Change_Airline_Details()
        {
            Execute_Test(() =>
            {
                AirlineCompany airlineCompany = airline_token.User;
                airlineCompany.Name = "Changed Name";
                airlineCompany.CountryId = 3;

                airline_facade.MofidyAirlineDetails(airline_token, airlineCompany);

                AirlineCompany airline_from_db = airline_facade.GetAirlineCompanyById(airlineCompany.Id);

                TestData.CompareProps(airlineCompany, airline_from_db);
            });
        }

        [TestMethod]
        public void Get_List_Of_Tickets()
        {
            Execute_Test(() =>
            {
                IList<Ticket> tickets = airline_facade.GetAllTickets(airline_token);
                Assert.AreEqual(tickets.Count, 2);
            });
        }
    }
}
