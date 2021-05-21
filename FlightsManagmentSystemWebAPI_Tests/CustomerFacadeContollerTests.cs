using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Controllers;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace FlightsManagmentSystemWebAPI_Tests
{
    [TestClass]
    public class CustomerFacadeContollerTests
    {
        private readonly TestHostFixture _testHostFixture = new TestHostFixture();// Initializes the webHost
        private HttpClient _httpClient;//Http client used to send requests to the contoller
        private IServiceProvider _serviceProvider;//Service provider used to provide services that registered in the API

        [TestInitialize]
        public async Task SetUp()
        {
            _httpClient = _testHostFixture.Client;
            _serviceProvider = _testHostFixture.ServiceProvider;
            TestsDAOPGSQL.ClearDB();
            await TestHelpers.Create_Country_For_Tests(_httpClient);
            await TestHelpers.Airline_Company_Login(_httpClient);
            await TestHelpers.Create_Flight_For_Tests(_httpClient);
        }

        [TestMethod]
        public async Task Purchase_And_Get_New_Ticket()
        {
            await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 1;

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            TicketDetailsDTO ticketPostResult = JsonSerializer.Deserialize<TicketDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var linkGeneratorService = _serviceProvider.GetRequiredService<LinkGenerator>();
            string createdPath = linkGeneratorService.GetPathByAction(nameof(CustomerFacadeController.GetTicketById), "CustomerFacade", new { id = ticketPostResult.Id });

            Assert.AreEqual(response.Headers.Location.OriginalString, createdPath);

            Assert.AreEqual(ticketPostResult.Id, 1);

            Assert.AreEqual(ticketPostResult.Flight.Id, 1);

            Assert.AreEqual(ticketPostResult.Customer.Id, 1);

            var response2 = await _httpClient.GetAsync($"api/tickets/{ticketPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            TicketDetailsDTO ticketGetResult = JsonSerializer.Deserialize<TicketDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            TestHelpers.CompareProps(ticketGetResult, ticketPostResult);
        }

        [TestMethod]
        public async Task Get_Ticket_Of_Another_Customer_Should_Return_Forbidden()
        {
            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer2",
                LastName = "Custom2",
                Address = null,
                PhoneNumber = "052-5555512",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerB",
                    Password = "Pass1234",
                    Email = "BCustomer@gmail.com",
                }
            };

            await TestHelpers.Customer_Login(_httpClient, createCustomerDTO);

            long flight_id = 1;

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            TicketDetailsDTO ticketPostResult = JsonSerializer.Deserialize<TicketDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await TestHelpers.Customer_Login(_httpClient);

            var response2 = await _httpClient.GetAsync($"api/tickets/{ticketPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.Forbidden, response2.StatusCode);
        }

        [TestMethod]
        public async Task Get_Ticket_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Customer_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/tickets/1");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Purchase_Ticket_With_Flight_Id_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 2;

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Purchase_Two_Ticket_For_The_Same_Flight_Should_Return_Conflict()
        {
            await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 1;

            await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [TestMethod]
        public async Task Purchase_Ticket_For_Flight_With_No_Remaining_Tickets_Should_Return_Gone()
        {
            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(2),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 0
            };

            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO);

            await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 2;

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Gone, response.StatusCode);
        }

        [TestMethod]
        public async Task Purchase_Ticket_For_Flight_That_Left_Should_Return_Gone()
        {
            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddSeconds(5),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 100
            };

            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO);

            await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 2;

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.Gone, response.StatusCode);
        }


        [TestMethod]
        public async Task Purchase_And_Get_List_Of_Tickets()
        {

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(2),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 1
            };

            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO);

            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 1;

            await _httpClient.PostAsync("api/tickets",
                new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            flight_id = 2;

            await _httpClient.PostAsync("api/tickets",
                new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/customer/tickets");
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> ticketsResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(ticketsResult.Count, 2);
        }

        [TestMethod]
        public async Task Purchase_And_Get_List_Of_Flights()
        {

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(2),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 1
            };

            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO);

            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 1;

            await _httpClient.PostAsync("api/tickets",
                new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            flight_id = 2;

            await _httpClient.PostAsync("api/tickets",
                new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/customer/flights");
            var responseContent = await response.Content.ReadAsStringAsync();
            List<Flight> flightsResult = JsonSerializer.Deserialize<List<Flight>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(flightsResult.Count, 2);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_My_Flights_Should_Return_No_Content()
        {
            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/customer/flights");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_My_Tickets_Should_Return_No_Content()
        {
            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/customer/tickets");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Cancel_Ticket()
        {
            await TestHelpers.Customer_Login(_httpClient);
            long flight_id = 1;

            await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.DeleteAsync("api/tickets/1");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Cancel_Ticket_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Customer_Login(_httpClient);

            var response = await _httpClient.DeleteAsync("api/tickets/1");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Cancel_Ticket_That_Belong_To_Another_Customer_Should_Return_Forbidden()
        {
            CreateCustomerDTO createCustomerDTO = new CreateCustomerDTO
            {
                FirstName = "Customer2",
                LastName = "Custom2",
                Address = null,
                PhoneNumber = "052-5555512",
                CreditCardNumber = null,
                User = new CreateUserDTO
                {
                    UserName = "CustomerB",
                    Password = "Pass1234",
                    Email = "BCustomer@gmail.com",
                }
            };

            await TestHelpers.Customer_Login(_httpClient, createCustomerDTO);

            long flight_id = 1;

            var response = await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            TicketDetailsDTO ticketPostResult = JsonSerializer.Deserialize<TicketDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await TestHelpers.Customer_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/tickets/{ticketPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }
    }
}
