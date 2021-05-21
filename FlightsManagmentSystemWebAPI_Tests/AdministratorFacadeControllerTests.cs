using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Controllers;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI_Tests
{
    [TestClass]
    public class AdministratorFacadeControllerTests
    {
        private readonly TestHostFixture _testHostFixture = new TestHostFixture();// Initializes the webHost
        private HttpClient _httpClient;//Http client used to send requests to the contoller
        private IServiceProvider _serviceProvider;//Service provider used to provide services that registered in the API

        [TestInitialize]
        public void SetUp()
        {
            _httpClient = _testHostFixture.Client;
            _serviceProvider = _testHostFixture.ServiceProvider;
            TestsDAOPGSQL.ClearDB();
        }

        [TestMethod]
        public async Task Create_And_Get_New_Country()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Country countryResult = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var linkGeneratorService = _serviceProvider.GetRequiredService<LinkGenerator>();
            string createdPath = linkGeneratorService.GetPathByAction(nameof(AnonymousFacadeController.GetCountryById), "AnonymousFacade", new { id = countryResult.Id });

            Assert.AreEqual(response.Headers.Location, createdPath);

            Assert.AreEqual(countryResult.Id, 1);

            var mapperService = _serviceProvider.GetRequiredService<IMapper>();
            Country mappedCountryInput = mapperService.Map<Country>(createCountryDTO);
            mappedCountryInput.Id = countryResult.Id;
            TestHelpers.CompareProps(mappedCountryInput, countryResult);

            var response2 = await _httpClient.GetAsync($"api/countries/{countryResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            Country countryGetResult = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TestHelpers.CompareProps(countryGetResult, countryResult);
        }

        [TestMethod]
        public async Task Get_Country_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/countries/1");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_List_Of_Countries()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            CreateCountryDTO createCountryDTO2 = new CreateCountryDTO { Name = "USA" };
            CreateCountryDTO createCountryDTO3 = new CreateCountryDTO { Name = "Russia" };
            await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO2), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO3), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/countries");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            List<Country> countryListResult = JsonSerializer.Deserialize<List<Country>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(countryListResult.Count, 3);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_Countries_Should_Return_No_Content()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/countries");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Invalid_Country_Should_Return_Bad_Request()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "" };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Two_Countries_With_Same_Name_Should_Return_Conflict()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response2 = await _httpClient.PostAsync("api/countries",
               new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, response2.StatusCode);
        }

        [TestMethod]
        public async Task Create_And_Get_New_Customer()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer",
                LastName = "Custom",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1234",
                    Email = "ACustomer@gmail.com",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Customer customerPostResult = JsonSerializer.Deserialize<Customer>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var linkGeneratorService = _serviceProvider.GetRequiredService<LinkGenerator>();
            string createdPath = linkGeneratorService.GetPathByAction(nameof(AdministratorFacadeController.GetCustomerById), "AdministratorFacade", new { id = customerPostResult.Id });

            Assert.AreEqual(response.Headers.Location.OriginalString, createdPath);

            Assert.AreEqual(customerPostResult.Id, 1);

            var mapperService = _serviceProvider.GetRequiredService<IMapper>();
            Customer mappedCustomerInput = mapperService.Map<Customer>(createCustomerDTO);
            mappedCustomerInput.Id = customerPostResult.Id;
            mappedCustomerInput.User.Id = customerPostResult.User.Id;
            mappedCustomerInput.User.UserRole = UserRoles.Customer;

            TestHelpers.CompareProps(mappedCustomerInput, customerPostResult);

            var response2 = await _httpClient.GetAsync($"api/customers/{customerPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            CustomerDetailsDTO customerGetResult = JsonSerializer.Deserialize<CustomerDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TestHelpers.CompareProps(customerGetResult, mapperService.Map<CustomerDetailsDTO>(customerPostResult));
        }

        [TestMethod]
        public async Task Get_Customer_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/customers/1");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_List_Of_Customers()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "CustomerA",
                LastName = "CustomA",
                Address = null,
                PhoneNumber = "052-5552223",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1111",
                    Email = "ACustomer@gmail.com",
                }
            };
            CreateCustomerDTO createCustomerDTO2 = new CreateCustomerDTO
            {
                FirstName = "CustomerB",
                LastName = "CustomB",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerB",
                    Password = "Pass9876",
                    Email = "BCustomer@gmail.com",
                }
            };
            CreateCustomerDTO createCustomerDTO3 = new CreateCustomerDTO
            {
                FirstName = "CustomerC",
                LastName = "CustomC",
                Address = null,
                PhoneNumber = "052-7496351",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerC",
                    Password = "Pass1234",
                    Email = "CCustomer@gmail.com",
                }
            };
            await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO2), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO3), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/customers");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            List<CustomerDetailsDTO> customersListResult = JsonSerializer.Deserialize<List<CustomerDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(customersListResult.Count, 3);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_Customers_Should_Return_No_Content()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/customers");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Invalid_Customer_Should_Return_Bad_Request()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "",
                LastName = "",
                Address = null,
                PhoneNumber = "",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "",
                    Password = "",
                    Email = "",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 6);
        }

        [TestMethod]
        public async Task Create_Two_Customers_With_Same_Email_Should_Return_Conflict()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer",
                LastName = "Custom",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1234",
                    Email = "ACustomer@gmail.com",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
                  new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            createCustomerDTO.PhoneNumber = "052-7654321";
            createCustomerDTO.User.UserName = "CustomerB";

            var response2 = await _httpClient.PostAsync("api/customers",
            new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, response2.StatusCode);
        }

        [TestMethod]
        public async Task Create_Two_Customers_With_Same_Username_Should_Return_Conflict()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer",
                LastName = "Custom",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1234",
                    Email = "ACustomer@gmail.com",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
                  new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            createCustomerDTO.PhoneNumber = "052-7654321";
            createCustomerDTO.User.Email = "BCustomer@gmail.com";

            var response2 = await _httpClient.PostAsync("api/customers",
            new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, response2.StatusCode);
        }

        [TestMethod]
        public async Task Create_Two_Customers_With_Same_Phone_Should_Return_Conflict()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer",
                LastName = "Custom",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1234",
                    Email = "ACustomer@gmail.com",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
                  new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            createCustomerDTO.User.UserName = "CustomerB";
            createCustomerDTO.User.Email = "BCustomer@gmail.com";

            var response2 = await _httpClient.PostAsync("api/customers",
            new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, response2.StatusCode);
        }

        [TestMethod]
        public async Task Create_And_Get_New_Airline_Company()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com",
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            AirlineCompany airlineCompanyPostResult = JsonSerializer.Deserialize<AirlineCompany>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var linkGeneratorService = _serviceProvider.GetRequiredService<LinkGenerator>();
            string createdPath = linkGeneratorService.GetPathByAction(nameof(AnonymousFacadeController.GetAirlineCompanyById), "AnonymousFacade", new { id = airlineCompanyPostResult.Id });

            Assert.AreEqual(response.Headers.Location.OriginalString, createdPath);

            Assert.AreEqual(airlineCompanyPostResult.Id, 1);

            var mapperService = _serviceProvider.GetRequiredService<IMapper>();
            AirlineCompany mappedAirlineCompanyInput = mapperService.Map<AirlineCompany>(createAirlineCompanyDTO);
            mappedAirlineCompanyInput.Id = airlineCompanyPostResult.Id;
            mappedAirlineCompanyInput.User.Id = airlineCompanyPostResult.User.Id;
            mappedAirlineCompanyInput.User.UserRole = UserRoles.Airline_Company;
            TestHelpers.CompareProps(mappedAirlineCompanyInput, airlineCompanyPostResult);

            var response2 = await _httpClient.GetAsync($"api/airline-companies/{airlineCompanyPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            AirlineCompanyDetailsDTO airlineCompanyGetResult = JsonSerializer.Deserialize<AirlineCompanyDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TestHelpers.CompareProps(airlineCompanyGetResult, mapperService.Map<AirlineCompanyDetailsDTO>(airlineCompanyPostResult));
        }

        [TestMethod]
        public async Task Get_Airline_Company_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/airline-companies/1");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_List_Of_Airline_Companies()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com",
                }
            };
            CreateAirlineCompanyDTO createAirlineCompanyDTO2 = new CreateAirlineCompanyDTO
            {
                Name = "Arkia",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ArkiaIL",
                    Password = "Pass9999",
                    Email = "Arkia@Arkia.com",
                }
            };
            CreateAirlineCompanyDTO createAirlineCompanyDTO3 = new CreateAirlineCompanyDTO
            {
                Name = "Israir",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "Israir",
                    Password = "Pass8888",
                    Email = "Israir@Israir.com",
                }
            };
            await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO2), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO3), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/airline-companies");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            List<AirlineCompanyDetailsDTO> airlineCompaniesListResult = JsonSerializer.Deserialize<List<AirlineCompanyDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(airlineCompaniesListResult.Count, 3);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_Airline_Companies_Should_Return_No_Content()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/airline-companies");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Invalid_Airline_Company_Should_Return_Bad_Request()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "",
                    Password = "",
                    Email = "",
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 4);
        }


        [TestMethod]
        public async Task Create_Two_Airline_Companies_With_Same_Name_Should_Return_Conflict()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com",
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
                  new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            createAirlineCompanyDTO.User.UserName = "ElAlIL2";
            createAirlineCompanyDTO.User.Email = "Elal2@Elal.com";

            var response2 = await _httpClient.PostAsync("api/airline-companies",
            new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, response2.StatusCode);
        }

        [TestMethod]
        public async Task Create_Airline_Company_With_Country_Id_That_Not_Exist_Should_Return_NotFound()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com",
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
                  new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_And_Get_New_Administrator()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAdministratorDTO createAdministratorDTO = new CreateAdministratorDTO
            {
                FirstName = "admin",
                LastName = "admin",
                Level = AdminLevel.Mid_Level_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin1",
                    Password = "Pass1234",
                    Email = "admin1@admin.com",
                }
            };
            var response = await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Administrator administratorPostResult = JsonSerializer.Deserialize<Administrator>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var linkGeneratorService = _serviceProvider.GetRequiredService<LinkGenerator>();
            string createdPath = linkGeneratorService.GetPathByAction(nameof(AdministratorFacadeController.GetAdminById), "AdministratorFacade", new { id = administratorPostResult.Id });

            Assert.AreEqual(response.Headers.Location.OriginalString, createdPath);

            Assert.AreEqual(administratorPostResult.Id, 1);

            var mapperService = _serviceProvider.GetRequiredService<IMapper>();
            Administrator mappedAdministratorInput = mapperService.Map<Administrator>(createAdministratorDTO);
            mappedAdministratorInput.Id = administratorPostResult.Id;
            mappedAdministratorInput.User.Id = administratorPostResult.User.Id;
            mappedAdministratorInput.User.UserRole = UserRoles.Administrator;
            TestHelpers.CompareProps(mappedAdministratorInput, administratorPostResult);

            var response2 = await _httpClient.GetAsync($"api/administrators/{administratorPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            AdministratorDetailsDTO administratorGetResult = JsonSerializer.Deserialize<AdministratorDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TestHelpers.CompareProps(administratorGetResult, mapperService.Map<AdministratorDetailsDTO>(administratorPostResult));
        }

        [TestMethod]
        public async Task Get_Administrator_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/administrators/1");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_List_Of_Administrators()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAdministratorDTO createAdministratorDTO = new CreateAdministratorDTO
            {
                FirstName = "admin1",
                LastName = "admin1",
                Level = AdminLevel.Junior_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin1",
                    Password = "Pass1234",
                    Email = "admin1@admin.com",
                }
            };
            CreateAdministratorDTO createAdministratorDTO2 = new CreateAdministratorDTO
            {
                FirstName = "admin2",
                LastName = "admin2",
                Level = AdminLevel.Mid_Level_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin2",
                    Password = "Pass1234",
                    Email = "admin2@admin.com",
                }
            };
            CreateAdministratorDTO createAdministratorDTO3 = new CreateAdministratorDTO
            {
                FirstName = "admin3",
                LastName = "admin3",
                Level = AdminLevel.Senior_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin3",
                    Password = "Pass1234",
                    Email = "admin3@admin.com",
                }
            };
            await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO2), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO3), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/administrators");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            List<AdministratorDetailsDTO> administratorsListResult = JsonSerializer.Deserialize<List<AdministratorDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(administratorsListResult.Count, 3);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_Administrators_Should_Return_No_Content()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/administrators");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Invalid_Administrator_Should_Return_Bad_Request()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAdministratorDTO createAdministratorDTO = new CreateAdministratorDTO
            {
                FirstName = "",
                LastName = "",
                Level = AdminLevel.Mid_Level_Admin,
                User = new CreateUserDTO
                {
                    UserName = "",
                    Password = "",
                    Email = "",
                }
            };
            var response = await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 5);
        }

        [TestMethod]
        public async Task Remove_Country()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Country countryResult = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deleteResponse = await _httpClient.DeleteAsync($"api/countries/{countryResult.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Country_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/countries/1");

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Country_That_Has_Related_Airline_Companies_Should_Return_Not_Modified()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com",
                }
            };
            await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var deleteResponse = await _httpClient.DeleteAsync($"api/countries/1");

            Assert.AreEqual(HttpStatusCode.NotModified, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Customer()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer",
                LastName = "Custom",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1234",
                    Email = "ACustomer@gmail.com",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Customer customerResult = JsonSerializer.Deserialize<Customer>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deleteResponse = await _httpClient.DeleteAsync($"api/customers/{customerResult.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }


        [TestMethod]
        public async Task Remove_Customer_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/customers/1");

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Administrator()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAdministratorDTO createAdministratorDTO = new CreateAdministratorDTO
            {
                FirstName = "admin",
                LastName = "admin",
                Level = AdminLevel.Mid_Level_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin1",
                    Password = "Pass1234",
                    Email = "admin1@admin.com",
                }
            };
            var response = await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Administrator administratorResult = JsonSerializer.Deserialize<Administrator>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deleteResponse = await _httpClient.DeleteAsync($"api/administrators/{administratorResult.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Administrator_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/administrators/1");

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Airline_Company()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com",
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            AirlineCompany airlineCompanyResult = JsonSerializer.Deserialize<AirlineCompany>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deleteResponse = await _httpClient.DeleteAsync($"api/airline-companies/{airlineCompanyResult.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Airline_Company_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/airline-companies/1");

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Country()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel", };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Country countryResult = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCountryDTO updateCountryDTO = new UpdateCountryDTO { Id = countryResult.Id, Name = "USA" };

            var putResponse = await _httpClient.PutAsync($"api/countries/{countryResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Country_With_Different_Id_Than_Url_Should_Return_Bad_Rquest()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel", };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Country countryResult = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCountryDTO updateCountryDTO = new UpdateCountryDTO { Id = countryResult.Id, Name = "USA" };

            var putResponse = await _httpClient.PutAsync("api/countries/2",
                new StringContent(JsonSerializer.Serialize(updateCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Country_With_Same_Name_As_Another_Country_Should_Return_Conflict()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "USA", };
            await _httpClient.PostAsync("api/countries",
              new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateCountryDTO createCountryDTO2 = new CreateCountryDTO { Name = "Israel", };
            var response = await _httpClient.PostAsync("api/countries",
             new StringContent(JsonSerializer.Serialize(createCountryDTO2), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Country countryResult = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCountryDTO updateCountryDTO = new UpdateCountryDTO { Id = countryResult.Id, Name = "USA" };

            var putResponse = await _httpClient.PutAsync($"api/countries/{countryResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Administrator()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAdministratorDTO createAdministratorDTO = new CreateAdministratorDTO
            {
                FirstName = "admin",
                LastName = "admin",
                Level = AdminLevel.Mid_Level_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin1",
                    Password = "Pass1234",
                    Email = "admin1@admin.com",
                }
            };
            var response = await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Administrator administratorResult = JsonSerializer.Deserialize<Administrator>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateAdministratorDTO updateAdministratorDTO = new UpdateAdministratorDTO
            {
                Id = administratorResult.Id,
                FirstName = "admin",
                LastName = "admin",
                Level = AdminLevel.Junior_Admin,
            };

            var putResponse = await _httpClient.PutAsync($"api/administrators/{administratorResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Administrator_With_Different_Id_Than_Url_Should_Return_Bad_Rquest()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateAdministratorDTO createAdministratorDTO = new CreateAdministratorDTO
            {
                FirstName = "admin",
                LastName = "admin",
                Level = AdminLevel.Mid_Level_Admin,
                User = new CreateUserDTO
                {
                    UserName = "admin1",
                    Password = "Pass1234",
                    Email = "admin1@admin.com",
                }
            };
            var response = await _httpClient.PostAsync("api/administrators",
             new StringContent(JsonSerializer.Serialize(createAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Administrator administratorResult = JsonSerializer.Deserialize<Administrator>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateAdministratorDTO updateAdministratorDTO = new UpdateAdministratorDTO
            {
                Id = administratorResult.Id,
                FirstName = "admin",
                LastName = "admin",
                Level = AdminLevel.Junior_Admin,
            };

            var putResponse = await _httpClient.PutAsync($"api/administrators/2",
                new StringContent(JsonSerializer.Serialize(updateAdministratorDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Customer()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer",
                LastName = "Custom",
                Address = null,
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerA",
                    Password = "Pass1234",
                    Email = "ACustomer@gmail.com",
                }
            };
            var response = await _httpClient.PostAsync("api/customers",
             new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Customer customerResult = JsonSerializer.Deserialize<Customer>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCustomerDTO updateCustomerDTO = new UpdateCustomerDTO
            {
                Id = customerResult.Id,
                FirstName = "Customer",
                LastName = "Custom",
                Address = "Ashdod",
                PhoneNumber = "052-1234567",
                CreditCardNumber = null,
            };

            var putResponse = await _httpClient.PutAsync($"api/customers/{customerResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Airline_Company()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com"
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            AirlineCompany airlineCompanyResult = JsonSerializer.Deserialize<AirlineCompany>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateAirlineCompanyDTO updateAirlineCompanyDTO = new UpdateAirlineCompanyDTO
            {
                Id = airlineCompanyResult.Id,
                Name = "Arkia",
                CountryId = 1
            };

            var putResponse = await _httpClient.PutAsync($"api/airline-companies/{airlineCompanyResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Airline_Company_With_Country_Id_That_Not_Exist_Should_Return_NotFound()
        {
            await TestHelpers.Main_Admin_Login(_httpClient);

            CreateCountryDTO createCountryDTO = new CreateCountryDTO { Name = "Israel" };
            await _httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Elal",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "ElAlIL",
                    Password = "Pass1234",
                    Email = "Elal@Elal.com"
                }
            };
            var response = await _httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            AirlineCompany airlineCompanyResult = JsonSerializer.Deserialize<AirlineCompany>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateAirlineCompanyDTO updateAirlineCompanyDTO = new UpdateAirlineCompanyDTO
            {
                Id = airlineCompanyResult.Id,
                Name = "Arkia",
                CountryId = 2
            };

            var putResponse = await _httpClient.PutAsync($"api/airline-companies/{airlineCompanyResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NotFound, putResponse.StatusCode);
        }
    }
}
