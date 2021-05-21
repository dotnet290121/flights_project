using BL;
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
    public class AnonymousUserFacadeTests
    {

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly FlightCenterSystem system = FlightCenterSystem.GetInstance();
        private AnonymousUserFacade anonymous_facade;

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            TestsDAOPGSQL.ClearDB();
        }

        [TestInitialize]
        public void Initialize()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("Log4Net.config"));

            FlightsManagmentSystemConfig.Instance.Init("FlightsManagmentSystemTests.Config.json");

            TestsDAOPGSQL.ClearDB();
            Init_Anonymous_Data();
            anonymous_facade = system.GetFacade<AnonymousUserFacade>();
        }

        private void Execute_Test(Action action, [CallerMemberName] string callerName = "")
        {
            _logger.Debug($"Run {callerName} Test");

            action.Invoke();

            _logger.Debug($"Exit {callerName} Test");
        }

        private void Init_Anonymous_Data()
        {
            _logger.Debug($"Start Init Anonymous Tests Data");

            TestsDAOPGSQL.ClearDB();
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
            system.TryLogin(TestData.Get_AirlineCompanies_Data()[0].User.UserName, TestData.Get_AirlineCompanies_Data()[0].User.Password, out ILoginToken token, out FacadeBase facade);
            LoginToken<AirlineCompany> airline_token = token as LoginToken<AirlineCompany>;
            LoggedInAirlineFacade airline_facade = facade as LoggedInAirlineFacade;
            Flight flight = TestData.Get_Flights_Data_For_Anonymous_Tests()[0];
            Flight flight2 = TestData.Get_Flights_Data_For_Anonymous_Tests()[1];
            Flight flight3 = TestData.Get_Flights_Data_For_Anonymous_Tests()[2];
            Flight flight4 = TestData.Get_Flights_Data_For_Anonymous_Tests()[3];
            Flight flight5 = TestData.Get_Flights_Data_For_Anonymous_Tests()[4];
            Flight flight6 = TestData.Get_Flights_Data_For_Anonymous_Tests()[5];

            long flight_id = airline_facade.CreateFlight(airline_token, flight);
            long flight_id2 = airline_facade.CreateFlight(airline_token, flight2);
            long flight_id3 = airline_facade.CreateFlight(airline_token, flight3);
            long flight_id4 = airline_facade.CreateFlight(airline_token, flight4);
            long flight_id5 = airline_facade.CreateFlight(airline_token, flight5);
            long flight_id6 = airline_facade.CreateFlight(airline_token, flight6);
            flight.Id = flight_id;
            flight2.Id = flight_id2;
            flight3.Id = flight_id3;
            flight4.Id = flight_id4;
            flight5.Id = flight_id5;
            flight6.Id = flight_id6;

            _logger.Debug($"End Init Anonymous Tests Data");
        }

        //[TestMethod]
        //public void Get_All_Flights_By_Origin_Country()
        //{
        //    Execute_Test(() =>
        //    {
        //        Assert.AreEqual(anonymous_facade.GetFlightsByOriginCountry(1).Count, 3);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByOriginCountry(2).Count, 2);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByOriginCountry(3).Count, 1);
        //    });
        //}


        //[TestMethod]
        //public void Get_All_Flights_By_Destination_Country()
        //{
        //    Execute_Test(() =>
        //    {
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDestinationCountry(1).Count, 0);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDestinationCountry(2).Count, 4);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDestinationCountry(3).Count, 2);
        //    });
        //}

        //[TestMethod]
        //public void Get_All_Flighst_By_Departure_Date()
        //{
        //    Execute_Test(() =>
        //    {
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDepatrureDate(DateTime.Now).Count, 0);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDepatrureDate(DateTime.Now.AddDays(1)).Count, 3);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDepatrureDate(DateTime.Now.AddDays(2)).Count, 1);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDepatrureDate(DateTime.Now.AddDays(3)).Count, 1);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByDepatrureDate(DateTime.Now.AddDays(4)).Count, 1);
        //    });
        //}

        //[TestMethod]
        //public void Get_All_Flights_By_Landing_Date()
        //{
        //    Execute_Test(() =>
        //    {
        //        Assert.AreEqual(anonymous_facade.GetFlightsByLandingDate(DateTime.Now.AddDays(2)).Count, 2);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByLandingDate(DateTime.Now.AddDays(3)).Count, 2);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByLandingDate(DateTime.Now.AddDays(4)).Count, 0);
        //        Assert.AreEqual(anonymous_facade.GetFlightsByLandingDate(DateTime.Now.AddDays(5)).Count, 2);
        //    });
        //}

        [TestMethod]
        public void Get_All_Flights_Vacancy()
        {
            Execute_Test(() =>
            {
                Dictionary<Flight, int> flight_vacancy = anonymous_facade.GetAllFlightsVacancy();
                Assert.AreEqual(flight_vacancy.Count, 6);

                for (int i = 0; i < flight_vacancy.Count; i++)
                {
                    Flight flight = TestData.Get_Flights_Data_For_Anonymous_Tests()[i];
                    flight.Id = i + 1;

                    Assert.AreEqual(flight.RemainingTickets, flight_vacancy[flight]);
                }
            });
        }

        [TestMethod]
        public void Search_Flights_With_No_Parameters()
        {
            Execute_Test(() =>
            {
                IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(flights.Count, 6);

            });
        }

        [TestMethod]
        public void Search_Flights_By_Destination_Country()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(0, anonymous_facade.SearchFlights(destinationCountryId: 1).Count);
                Assert.AreEqual(4, anonymous_facade.SearchFlights(destinationCountryId: 2).Count);
                Assert.AreEqual(2, anonymous_facade.SearchFlights(destinationCountryId: 3).Count);
            });
        }

        [TestMethod]
        public void Search_Flights_By_Origin_Country()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(3, anonymous_facade.SearchFlights(originCountryId: 1).Count);
                Assert.AreEqual(2, anonymous_facade.SearchFlights(originCountryId: 2).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(originCountryId: 3).Count);
            });
        }

        [TestMethod]
        public void Search_Flights_By_Origin_And_Destination_Country()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(0, anonymous_facade.SearchFlights(originCountryId: 1, destinationCountryId: 1).Count);
                Assert.AreEqual(2, anonymous_facade.SearchFlights(originCountryId: 1, destinationCountryId: 2).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(originCountryId: 1, destinationCountryId: 3).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(originCountryId: 2, destinationCountryId: 2).Count);
                Assert.AreEqual(0, anonymous_facade.SearchFlights(originCountryId: 2, destinationCountryId: 1).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(originCountryId: 3, destinationCountryId: 2).Count);
            });
        }

        [TestMethod]
        public void Search_Flights_By_Departure_Date()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(0, anonymous_facade.SearchFlights(departureDate: DateTime.Now).Count);
                Assert.AreEqual(3, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(2)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(3)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(4)).Count);

            });
        }

        [TestMethod]
        public void Search_Flights_By_Landing_Date()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(0, anonymous_facade.SearchFlights(landingDate: DateTime.Now).Count);
                Assert.AreEqual(2, anonymous_facade.SearchFlights(landingDate: DateTime.Now.AddDays(2)).Count);
                Assert.AreEqual(2, anonymous_facade.SearchFlights(landingDate: DateTime.Now.AddDays(3)).Count);
                Assert.AreEqual(0, anonymous_facade.SearchFlights(landingDate: DateTime.Now.AddDays(4)).Count);
                Assert.AreEqual(2, anonymous_facade.SearchFlights(landingDate: DateTime.Now.AddDays(5)).Count);

            });
        }

        [TestMethod]
        public void Search_Flights_By_Departure_And_Landing_Date()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(2, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(2)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(3)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(2), landingDate: DateTime.Now.AddDays(3)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(3), landingDate: DateTime.Now.AddDays(5)).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(4), landingDate: DateTime.Now.AddDays(5)).Count);
                Assert.AreEqual(0, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(1)).Count);
            });
        }

        [TestMethod]
        public void Search_Flights_By_Departure_And_Landing_Date_And_Origin_And_Destionations_Countries()
        {
            Execute_Test(() =>
            {
                // IList<Flight> flights = anonymous_facade.SearchFlights();
                Assert.AreEqual(0, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(2), originCountryId: 2, destinationCountryId: 3).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(3), originCountryId: 2, destinationCountryId: 3).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(2), originCountryId: 1, destinationCountryId: 3).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(1), landingDate: DateTime.Now.AddDays(2), originCountryId: 2, destinationCountryId: 2).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(2), landingDate: DateTime.Now.AddDays(3), originCountryId: 3, destinationCountryId: 2).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(3), landingDate: DateTime.Now.AddDays(5), originCountryId: 1, destinationCountryId: 2).Count);
                Assert.AreEqual(1, anonymous_facade.SearchFlights(departureDate: DateTime.Now.AddDays(4), landingDate: DateTime.Now.AddDays(5), originCountryId: 1, destinationCountryId: 2).Count);
            });
        }
    }
}
