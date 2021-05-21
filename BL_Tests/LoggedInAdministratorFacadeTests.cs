using BL;
using BL.Exceptions;
using BL.LoginService;
using ConfigurationService;
using DAL.Exceptions;
using Domain.Entities;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BL_Tests
{
    [TestClass]
    public class LoggedInAdministratorFacadeTests
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly FlightCenterSystem system = FlightCenterSystem.GetInstance();
        private LoggedInAdministratorFacade administrator_facade;
        private LoginToken<Administrator> administrator_token;
        private LoggedInAdministratorFacade administrator_level_one_facade;
        private LoginToken<Administrator> administrator_level_one_token;

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
            Login();
        }

        private void Login()
        {
            string username = "admin";
            string password = "9999";
            system.TryLogin(username, password, out ILoginToken token, out FacadeBase facade);
            administrator_token = token as LoginToken<Administrator>;
            administrator_facade = facade as LoggedInAdministratorFacade;
        }
        private void Init_Admin_Level_One_And_Login()
        {
            _logger.Debug($"Start Init Admin Level One Tests Data");

            Administrator admin_level_one = TestData.Get_Administrators_Data()[0];
            int admin_level_one_id = administrator_facade.CreateNewAdmin(administrator_token, admin_level_one);
            system.TryLogin(admin_level_one.User.UserName, admin_level_one.User.Password, out ILoginToken token, out FacadeBase facade);
            administrator_level_one_token = token as LoginToken<Administrator>;
            administrator_level_one_facade = facade as LoggedInAdministratorFacade;

            _logger.Debug($"End Init Admin Level One Tests Data");
        }

        [TestMethod]
        public void Valid_Main_Administrator_Login()
        {
            Execute_Test(() =>
            {
                string username = "admin";
                string password = "9999";
                bool result = system.TryLogin(username, password, out ILoginToken token, out FacadeBase facade);
                Assert.IsTrue(result);
                Assert.AreEqual(0, administrator_token.User.Id);
                Assert.AreEqual("Admin", administrator_token.User.FirstName);
                Assert.AreEqual("Admin", administrator_token.User.LastName);
                Assert.AreEqual(AdminLevel.Main_Admin, administrator_token.User.Level);
                Assert.AreEqual(0, administrator_token.User.User.Id);
                Assert.AreEqual(username, administrator_token.User.User.UserName);
                Assert.AreEqual(password, administrator_token.User.User.Password);
                Assert.AreEqual("admin@admin.com", administrator_token.User.User.Email);
                Assert.AreEqual(UserRoles.Administrator, administrator_token.User.User.UserRole);
            });
        }

        [TestMethod]
        public void Invalid_Administrator_Login()
        {
            Execute_Test(() =>
            {
                string username = "adminn";
                string password = "9999";
                bool result = system.TryLogin(username, password, out ILoginToken token, out FacadeBase anonymous_facade);
                Assert.IsFalse(result);
                Assert.IsNull(token);
                Assert.IsInstanceOfType(anonymous_facade, typeof(AnonymousUserFacade));

                username = "admin";
                password = "99999";
                bool result2 = system.TryLogin(username, password, out ILoginToken token2, out FacadeBase anonymous_facade2);
                Assert.IsFalse(result2);
                Assert.IsNull(token2);
                Assert.IsInstanceOfType(anonymous_facade2, typeof(AnonymousUserFacade));
            });
        }

        [TestMethod]
        public void Create_And_Get_New_Administrator()
        {
            Execute_Test(() =>
            {
                Administrator demi_administrator = TestData.Get_Administrators_Data()[0];
                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_administrator);
                Assert.AreEqual(admin_id, 1);
                demi_administrator.Id = admin_id;
                Administrator administrator_from_db = administrator_facade.GetAdminById(administrator_token, admin_id);

                TestData.CompareProps(administrator_from_db, demi_administrator);
            });
        }

        [TestMethod]
        public void Create_New_Administrator_Using_Level_One_Admin()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                Administrator demi_administrator = TestData.Get_Administrators_Data()[1];
                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.CreateNewAdmin(administrator_level_one_token, demi_administrator));
            });
        }

        [TestMethod]
        public void Create_New_Administrator_With_Level_Four_Using_Level_One_Admin()
        {
            Init_Admin_Level_One_And_Login();

            Execute_Test(() =>
            {
                Administrator demi_administrator = TestData.Get_Administrators_Data()[0];
                demi_administrator.Level = AdminLevel.Main_Admin;
                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.CreateNewAdmin(administrator_level_one_token, demi_administrator));
            });
        }

        [TestMethod]
        public void Create_And_Get_List_Of_Administrators()
        {
            Execute_Test(() =>
            {
                Administrator[] data = TestData.Get_Administrators_Data();
                Administrator[] demi_administrators = { data[0], data[1], data[2] };
                for (int i = 0; i < demi_administrators.Length; i++)
                {
                    int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_administrators[i]);
                    Assert.AreEqual(admin_id, i + 1);
                    demi_administrators[i].Id = admin_id;
                }

                IList<Administrator> administrators_from_db = administrator_facade.GetAllAdministrators(administrator_token);
                Assert.AreEqual(demi_administrators.Length, administrators_from_db.Count);
                for (int i = 0; i < administrators_from_db.Count; i++)
                {
                    TestData.CompareProps(administrators_from_db[i], demi_administrators[i]);
                }
            });
        }

        [TestMethod]
        public void Get_Administrator_That_Not_Exists()
        {
            Execute_Test(() =>
            {
                Administrator administrator_from_db = administrator_facade.GetAdminById(administrator_token, 1);

                Assert.IsNull(administrator_from_db);
            });
        }

        [TestMethod]
        public void Create_Two_Administrators_With_Same_Username()
        {
            Execute_Test(() =>
            {
                Administrator demi_administrator = TestData.Get_Administrators_Data()[0];
                Administrator demi_administrator_with_same_username = TestData.Get_Administrators_Data()[3];

                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_administrator);
                Assert.AreEqual(admin_id, 1);

                Assert.ThrowsException<RecordAlreadyExistsException>(() => administrator_facade.CreateNewAdmin(administrator_token, demi_administrator_with_same_username));
            });
        }

        [TestMethod]
        public void Create_Two_Administrators_With_Same_Email()
        {
            Execute_Test(() =>
            {
                Administrator demi_administrator = TestData.Get_Administrators_Data()[0];
                Administrator demi_administrator_with_same_email = TestData.Get_Administrators_Data()[4];

                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_administrator);
                Assert.AreEqual(admin_id, 1);

                Assert.ThrowsException<RecordAlreadyExistsException>(() => administrator_facade.CreateNewAdmin(administrator_token, demi_administrator_with_same_email));
            });
        }

        [TestMethod]
        public void Create_And_Get_New_Country()
        {
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];
                int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_country);
                Assert.AreEqual(country_id, 1);
                demi_country.Id = country_id;
                Country country_from_db = administrator_facade.GetCountryById(country_id);

                TestData.CompareProps(country_from_db, demi_country);
            });
        }

        [TestMethod]
        public void Create_And_Get_List_Of_New_Countries()
        {
            Execute_Test(() =>
            {
                Country[] data = TestData.Get_Countries_Data();
                Country[] demi_countries = { data[0], data[1], data[2], data[3], data[4] };
                for (int i = 0; i < demi_countries.Length; i++)
                {
                    int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_countries[i]);
                    Assert.AreEqual(country_id, i + 1);
                    demi_countries[i].Id = country_id;
                }

                IList<Country> countries_from_db = administrator_facade.GetAllCountries();
                Assert.AreEqual(demi_countries.Length, countries_from_db.Count);
                for (int i = 0; i < countries_from_db.Count; i++)
                {
                    TestData.CompareProps(countries_from_db[i], demi_countries[i]);
                }
            });
        }

        [TestMethod]
        public void Create_Two_Countries_With_Same_Name()
        {
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];

                int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_country);
                Assert.AreEqual(country_id, 1);

                Assert.ThrowsException<RecordAlreadyExistsException>(() => administrator_facade.CreateNewCountry(administrator_token, demi_country));
            });
        }

        [TestMethod]
        public void Get_Country_That_Not_Exists()
        {
            Execute_Test(() =>
            {
                Country country_from_db = administrator_facade.GetCountryById(1);

                Assert.IsNull(country_from_db);
            });
        }

        [TestMethod]
        public void Create_And_Get_New_Customer()
        {
            Execute_Test(() =>
            {
                Customer demi_customer = TestData.Get_Customers_Data()[0];
                long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);
                Assert.AreEqual(customer_id, 1);
                demi_customer.Id = customer_id;
                Customer customer_from_db = administrator_facade.GetCustomerById(administrator_token, customer_id);

                TestData.CompareProps(customer_from_db, demi_customer);
            });
        }

        [TestMethod]
        public void Create_And_Get_List_Of_New_Customers()
        {
            Execute_Test(() =>
            {
                Customer[] data = TestData.Get_Customers_Data();
                Customer[] demi_customers = { data[0], data[1], data[2] };
                for (int i = 0; i < demi_customers.Length; i++)
                {
                    long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customers[i]);
                    Assert.AreEqual(customer_id, i + 1);
                    Assert.AreEqual(demi_customers[i].User.Id, i + 1);
                    demi_customers[i].Id = customer_id;
                }

                IList<Customer> customers_from_db = administrator_facade.GetAllCustomers(administrator_token);
                Assert.AreEqual(demi_customers.Length, customers_from_db.Count);
                for (int i = 0; i < customers_from_db.Count; i++)
                {
                    TestData.CompareProps(customers_from_db[i], demi_customers[i]);
                }
            });
        }

        [TestMethod]
        public void Get_Customer_That_Not_Exists()
        {
            Execute_Test(() =>
            {
                Customer customer_from_db = administrator_facade.GetCustomerById(administrator_token, 1);

                Assert.IsNull(customer_from_db);
            });
        }

        [TestMethod]
        public void Create_Two_Customers_With_Same_Phone_Number()
        {
            Execute_Test(() =>
            {
                Customer demi_customer = TestData.Get_Customers_Data()[0];
                Customer demi_customer_with_same_phone_number = TestData.Get_Customers_Data()[3];

                long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);
                Assert.AreEqual(customer_id, 1);
                Assert.AreEqual(demi_customer.User.Id, 1);

                Assert.ThrowsException<RecordAlreadyExistsException>(() => administrator_facade.CreateNewCustomer(administrator_token, demi_customer_with_same_phone_number));
            });
        }

        [TestMethod]
        public void Create_And_Get_New_Airline()
        {
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);

                AirlineCompany demi_airline_company = TestData.Get_AirlineCompanies_Data()[0];
                demi_airline_company.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_company);
                Assert.AreEqual(airline_company_id, 1);
                demi_airline_company.Id = airline_company_id;
                AirlineCompany airline_company_from_db = administrator_facade.GetAirlineCompanyById(airline_company_id);

                TestData.CompareProps(airline_company_from_db, demi_airline_company);
            });
        }

        [TestMethod]
        public void Create_And_Get_List_Of_New_Airlines()
        {
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);
                int country_id2 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[2]);
                int country_id3 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[5]);


                AirlineCompany[] data = TestData.Get_AirlineCompanies_Data();
                AirlineCompany[] demi_airline_companies = { data[0], data[1], data[2] };
                demi_airline_companies[0].CountryId = country_id;
                demi_airline_companies[1].CountryId = country_id2;
                demi_airline_companies[2].CountryId = country_id3;
                for (int i = 0; i < demi_airline_companies.Length; i++)
                {
                    long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_companies[i]);
                    Assert.AreEqual(airline_company_id, i + 1);
                    demi_airline_companies[i].Id = airline_company_id;
                }

                IList<AirlineCompany> airline_companies_from_db = administrator_facade.GetAllAirlineCompanies();
                Assert.AreEqual(demi_airline_companies.Length, airline_companies_from_db.Count);
                for (int i = 0; i < airline_companies_from_db.Count; i++)
                {
                    TestData.CompareProps(airline_companies_from_db[i], demi_airline_companies[i]);
                }
            });
        }

        [TestMethod]
        public void Get_Airline_That_Not_Exists()
        {
            Execute_Test(() =>
            {
                AirlineCompany airline_company_from_db = administrator_facade.GetAirlineCompanyById(1);

                Assert.IsNull(airline_company_from_db);
            });
        }

        [TestMethod]
        public void Update_Airline()
        {
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);
                int country_id2 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[1]);

                AirlineCompany demi_airline_company = TestData.Get_AirlineCompanies_Data()[0];

                demi_airline_company.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_company);
                demi_airline_company.Id = airline_company_id;
                demi_airline_company.Name = "Changed name";
                demi_airline_company.CountryId = country_id2;

                administrator_facade.UpdateAirlineDetails(administrator_token, demi_airline_company);

                AirlineCompany updated_airline_company = administrator_facade.GetAirlineCompanyById(airline_company_id);

                TestData.CompareProps(demi_airline_company, updated_airline_company);
            });
        }

        [TestMethod]
        public void Update_Airline_With_CountryId_That_Not_Exists()
        {
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);

                AirlineCompany demi_airline_company = TestData.Get_AirlineCompanies_Data()[0];

                demi_airline_company.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_company);

                demi_airline_company.Id = airline_company_id;
                demi_airline_company.CountryId = 99;

                Assert.ThrowsException<RelatedRecordNotExistsException>(() => administrator_facade.UpdateAirlineDetails(administrator_token, demi_airline_company));
            });
        }



        [TestMethod]
        public void Update_Admin()
        {
            Execute_Test(() =>
            {
                Administrator demi_admin = TestData.Get_Administrators_Data()[0];

                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_admin);
                demi_admin.Id = admin_id;
                demi_admin.FirstName = "Changed";
                demi_admin.LastName = "Name";
                demi_admin.Level = AdminLevel.Mid_Level_Admin;

                administrator_facade.UpdateAdminDetails(administrator_token, demi_admin);

                Administrator updated_admin = administrator_facade.GetAdminById(administrator_token, admin_id);

                TestData.CompareProps(demi_admin, updated_admin);
            });
        }

        [TestMethod]
        public void Update_Admin_Using_Level_One_Admin()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                Administrator demi_admin = TestData.Get_Administrators_Data()[1];

                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_admin);
                demi_admin.Id = admin_id;
                demi_admin.FirstName = "Changed";
                demi_admin.LastName = "Name";
                demi_admin.Level = AdminLevel.Main_Admin;

                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.UpdateAdminDetails(administrator_level_one_token, demi_admin));
            });
        }



        [TestMethod]
        public void Update_Customer()
        {
            Execute_Test(() =>
            {
                Customer demi_customer = TestData.Get_Customers_Data()[0];

                long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);
                demi_customer.Id = customer_id;
                demi_customer.FirstName = "Changed";
                demi_customer.LastName = "Name";
                demi_customer.Address = "Changed Address";
                demi_customer.CreditCardNumber = "11111111111111111111111";
                demi_customer.PhoneNumber = "11111111111";

                administrator_facade.UpdateCustomerDetails(administrator_token, demi_customer);

                Customer updated_customer = administrator_facade.GetCustomerById(administrator_token, customer_id);

                TestData.CompareProps(demi_customer, updated_customer);
            });
        }


        [TestMethod]
        public void Update_Country()
        {
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];
                int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_country);

                demi_country.Id = country_id;
                demi_country.Name = "Some other name";

                administrator_facade.UpdateCountryDetails(administrator_token, demi_country);

                Country updated_country = administrator_facade.GetCountryById(country_id);

                TestData.CompareProps(demi_country, updated_country);
            });
        }

        [TestMethod]
        public void Update_Country_With_Same_Name()
        {
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];
                Country demi_country2 = TestData.Get_Countries_Data()[1];

                int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_country);
                int country_id2 = administrator_facade.CreateNewCountry(administrator_token, demi_country2);

                demi_country2.Id = country_id2;
                demi_country2.Name = demi_country.Name;

                Assert.ThrowsException<RecordAlreadyExistsException>(() => administrator_facade.UpdateCountryDetails(administrator_token, demi_country2));
            });
        }

        [TestMethod]
        public void Remove_Country()
        {
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];
                int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_country);

                demi_country.Id = country_id;

                administrator_facade.RemoveCountry(administrator_token, demi_country);

                Assert.AreEqual(administrator_facade.GetAllCountries().Count, 0);
            });
        }

        [TestMethod]
        public void Remove_Country_Using_Level_One_Admin_Should_Throw_NotAllowedAdminActionException()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];
                int country_id = administrator_facade.CreateNewCountry(administrator_token, demi_country);

                demi_country.Id = country_id;

                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.RemoveCountry(administrator_level_one_token, demi_country));
            });
        }

        [TestMethod]
        public void Add_Country_Using_Level_Two_Admin_Should_Throw_NotAllowedAdminActionException()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                Country demi_country = TestData.Get_Countries_Data()[0];

                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.CreateNewCountry(administrator_level_one_token, demi_country));
            });
        }

        [TestMethod]
        public void Remove_Customer()
        {
            Execute_Test(() =>
            {
                Customer demi_customer = TestData.Get_Customers_Data()[0];
                long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);

                demi_customer.Id = customer_id;

                administrator_facade.RemoveCustomer(administrator_token, demi_customer);

                Assert.AreEqual(administrator_facade.GetAllCustomers(administrator_token).Count, 0);
            });
        }

        [TestMethod]
        public void Remove_Customer_With_Ticket()
        {
            Execute_Test(() =>
            {

                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);
                int country_id2 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[1]);
                int country_id3 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[2]);

                AirlineCompany demi_airlineCompany = TestData.Get_AirlineCompanies_Data()[0];
                demi_airlineCompany.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airlineCompany);
                demi_airlineCompany.Id = airline_company_id;

                system.TryLogin(demi_airlineCompany.User.UserName, demi_airlineCompany.User.Password, out ILoginToken token, out FacadeBase facade);
                LoggedInAirlineFacade airline_facade = facade as LoggedInAirlineFacade;
                LoginToken<AirlineCompany> airline_token = token as LoginToken<AirlineCompany>;

                Flight demi_flight = TestData.Get_Flights_Data()[0];
                long flight_id = airline_facade.CreateFlight(airline_token, demi_flight);
                demi_flight.Id = flight_id;

                Customer demi_customer = TestData.Get_Customers_Data()[0];
                long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);
                demi_customer.Id = customer_id;

                system.TryLogin(demi_customer.User.UserName, demi_customer.User.Password, out ILoginToken token2, out FacadeBase facade2);
                LoggedInCustomerFacade customer_facade = facade2 as LoggedInCustomerFacade;
                LoginToken<Customer> customer_token = token2 as LoginToken<Customer>;

                Ticket ticket = customer_facade.PurchaseTicket(customer_token, demi_flight);
                Assert.AreEqual(customer_facade.GetAllMyTickets(customer_token).Count, 1);


                administrator_facade.RemoveCustomer(administrator_token, demi_customer);

                Assert.AreEqual(administrator_facade.GetAllCustomers(administrator_token).Count, 0);
                Assert.AreEqual(customer_facade.GetAllMyTickets(customer_token).Count, 0);
                Assert.AreEqual(customer_facade.GetTicketHistoryByOriginalId(customer_token, ticket.Id).Id, 1);
            });
        }

        [TestMethod]
        public void Remove_Customer_Using_Level_One_Admin_Should_Throw_NotAllowedAdminActionException()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                Customer demi_customer = TestData.Get_Customers_Data()[0];
                long customer_id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);

                demi_customer.Id = customer_id;

                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.RemoveCustomer(administrator_level_one_token, demi_customer));
            });
        }

        [TestMethod]
        public void Remove_Admin()
        {
            Execute_Test(() =>
            {
                Administrator demi_admin = TestData.Get_Administrators_Data()[0];
                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_admin);

                demi_admin.Id = admin_id;

                administrator_facade.RemoveAdmin(administrator_token, demi_admin);

                Assert.AreEqual(administrator_facade.GetAllAdministrators(administrator_token).Count, 0);
            });
        }

        [TestMethod]
        public void Remove_Admin_Using_Level_One_Admin_Should_Throw_NotAllowedAdminActionException()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                Administrator demi_admin = TestData.Get_Administrators_Data()[1];
                int admin_id = administrator_facade.CreateNewAdmin(administrator_token, demi_admin);

                demi_admin.Id = admin_id;

                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.RemoveAdmin(administrator_level_one_token, demi_admin));
            });
        }

        [TestMethod]
        public void Remove_Airline_Company()
        {
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);

                AirlineCompany demi_airline_company = TestData.Get_AirlineCompanies_Data()[0];
                demi_airline_company.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_company);

                demi_airline_company.Id = airline_company_id;

                administrator_facade.RemoveAirline(administrator_token, demi_airline_company);

                Assert.AreEqual(administrator_facade.GetAllAirlineCompanies().Count, 0);
            });
        }

        [TestMethod]
        public void Remove_Airline_Company_With_Flight_And_Ticket()
        {
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);
                int country_id2 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[1]);
                int country_id3 = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[2]);

                AirlineCompany demi_airline_company = TestData.Get_AirlineCompanies_Data()[0];
                demi_airline_company.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_company);
                demi_airline_company.Id = airline_company_id;

                system.TryLogin(demi_airline_company.User.UserName, demi_airline_company.User.Password, out ILoginToken token, out FacadeBase facade);
                LoggedInAirlineFacade airlineFacade = facade as LoggedInAirlineFacade;
                LoginToken<AirlineCompany> airlineToken = token as LoginToken<AirlineCompany>;

                Flight demi_flight = TestData.Get_Flights_Data()[0];
                long flight_id=airlineFacade.CreateFlight(airlineToken,demi_flight);
                demi_flight.Id = flight_id;

                Customer demi_customer = TestData.Get_Customers_Data()[0];
                demi_customer.Id = administrator_facade.CreateNewCustomer(administrator_token, demi_customer);

                system.TryLogin(demi_customer.User.UserName, demi_customer.User.Password, out ILoginToken token2, out FacadeBase facade2);
                LoggedInCustomerFacade customerFacade = facade2 as LoggedInCustomerFacade;
                LoginToken<Customer> customerToken = token2 as LoginToken<Customer>;

                Ticket ticket=customerFacade.PurchaseTicket(customerToken, demi_flight);

                administrator_facade.RemoveAirline(administrator_token, demi_airline_company);

                Assert.AreEqual(administrator_facade.GetAllAirlineCompanies().Count, 0);
                Assert.AreEqual(administrator_facade.GetAllFlights().Count, 0);
                Assert.AreEqual(airlineFacade.GetAllTickets(airlineToken).Count, 0);
                Assert.AreEqual(airlineFacade.GetFlightHistoryByOriginalId(airlineToken,demi_flight.Id).Id, 1);
                Assert.AreEqual(customerFacade.GetTicketHistoryByOriginalId(customerToken,ticket.Id).Id, 1);
            });
        }

        [TestMethod]
        public void Remove_Airline_Company_Using_Level_One_Admin()
        {
            Init_Admin_Level_One_And_Login();
            Execute_Test(() =>
            {
                int country_id = administrator_facade.CreateNewCountry(administrator_token, TestData.Get_Countries_Data()[0]);

                AirlineCompany demi_airline_company = TestData.Get_AirlineCompanies_Data()[0];
                demi_airline_company.CountryId = country_id;
                long airline_company_id = administrator_facade.CreateNewAirlineCompany(administrator_token, demi_airline_company);

                demi_airline_company.Id = airline_company_id;

                Assert.ThrowsException<NotAllowedAdminActionException>(() => administrator_level_one_facade.RemoveAirline(administrator_level_one_token, demi_airline_company));
            });
        }
    }
}
