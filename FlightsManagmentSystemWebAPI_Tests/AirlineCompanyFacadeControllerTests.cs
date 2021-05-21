using AutoMapper;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Controllers;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
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

namespace FlightsManagmentSystemWebAPI_Tests
{
    [TestClass]
    public class AirlineCompanyFacadeControllerTests
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
        }

        [TestMethod]
        public async Task Create_And_Get_New_Flight()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(2),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 15
            };
            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightPostResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var linkGeneratorService = _serviceProvider.GetRequiredService<LinkGenerator>();
            string createdPath = linkGeneratorService.GetPathByAction(nameof(AnonymousFacadeController.GetFlightById), "AnonymousFacade", new { id = flightPostResult.Id });

            Assert.AreEqual(response.Headers.Location.OriginalString, createdPath);

            Assert.AreEqual(flightPostResult.Id, 1);

            var mapperService = _serviceProvider.GetRequiredService<IMapper>();
            Flight mappedFlightInput = mapperService.Map<Flight>(createFlightDTO);
            mappedFlightInput.Id = flightPostResult.Id;
            mappedFlightInput.AirlineCompany = flightPostResult.AirlineCompany;

            TestHelpers.CompareProps(mappedFlightInput, flightPostResult);

            var response2 = await _httpClient.GetAsync($"api/flights/{flightPostResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            FlightDetailsDTO flightGetResult = JsonSerializer.Deserialize<FlightDetailsDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TestHelpers.CompareProps(flightGetResult, mapperService.Map<FlightDetailsDTO>(flightPostResult));
        }

        [TestMethod]
        public async Task Create_New_Flight_With_Dates_In_Past_Should_Return_Bad_Request()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(-12),
                LandingTime = DateTime.Now.AddHours(-6),
                RemainingTickets = 15
            };
            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 2);
        }

        [TestMethod]
        public async Task Create_New_Flight_With_Departure_Date_After_Landing_Date_Should_Return_Bad_Request()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 15
            };
            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 2);
        }

        [TestMethod]
        public async Task Create_New_Flight_With_Country_Id_That_Not_Exists_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 2,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };
            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public async Task Get_List_Of_Flights()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync("api/flights");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 3);

            var response2 = await _httpClient.GetAsync($"api/airline-company/flights");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent2 = await response2.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsByAirlineCompanyListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsByAirlineCompanyListResult.Count, 3);
        }

        [TestMethod]
        public async Task Get_Empty_List_Of_Flights_Should_Return_No_Content()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            var response = await _httpClient.GetAsync("api/flights");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var response2 = await _httpClient.GetAsync($"api/airline-company/flights");

            Assert.AreEqual(HttpStatusCode.NoContent, response2.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Flight()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deleteResponse = await _httpClient.DeleteAsync($"api/flights/{flightResult.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Flight_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/flights/1");

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Remove_Flight_That_Belongs_To_Another_Airline_Should_Return_Forbidden()
        {
            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Arkia",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "Arkiaa",
                    Password = "Pass1234",
                    Email = "Arkia@Arkia.com",
                }
            };

            await TestHelpers.Airline_Company_Login(_httpClient, createAirlineCompanyDTO);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            await TestHelpers.Airline_Company_Login(_httpClient);

            var deleteResponse = await _httpClient.DeleteAsync($"api/flights/1");

            Assert.AreEqual(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task Get_Flight_History_By_Original_Id()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);


            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deleteResponse = await _httpClient.DeleteAsync($"api/flights/{flightResult.Id}");

            var getResponse = await _httpClient.GetAsync($"api/history/flights/{flightResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Get_Flight_History_By_Original_Id_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            var getResponse = await _httpClient.GetAsync($"api/history/flights/1");

            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }


        [TestMethod]
        public async Task Get_Flight_History_By_Original_Id_That_Belong_To_Another_Airline_Should_Return_Forbidden()
        {
            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Arkia",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "Arkiaa",
                    Password = "Pass1234",
                    Email = "Arkia@Arkia.com",
                }
            };

            await TestHelpers.Airline_Company_Login(_httpClient, createAirlineCompanyDTO);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            await _httpClient.DeleteAsync($"api/flights/1");

            await TestHelpers.Airline_Company_Login(_httpClient);

            var getResponse = await _httpClient.GetAsync($"api/history/flights/1");

            Assert.AreEqual(HttpStatusCode.Forbidden, getResponse.StatusCode);
        }


        [TestMethod]
        public async Task Update_Flight()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateFlightDTO updateFlightDTO = new UpdateFlightDTO
            {
                Id = flightResult.Id,
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(13),
                LandingTime = DateTime.Now.AddHours(17),
                RemainingTickets = 10
            };

            var putResponse = await _httpClient.PutAsync($"api/flights/{flightResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Flight_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);
            UpdateFlightDTO updateFlightDTO = new UpdateFlightDTO
            {
                Id = 1,
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(13),
                LandingTime = DateTime.Now.AddHours(17),
                RemainingTickets = 10
            };

            var putResponse = await _httpClient.PutAsync($"api/flights/{updateFlightDTO.Id}",
                new StringContent(JsonSerializer.Serialize(updateFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NotFound, putResponse.StatusCode);
        }


        [TestMethod]
        public async Task Update_Flight_That_Belongs_To_Another_Airline_Should_Return_Forbidden()
        {
            CreateAirlineCompanyDTO createAirlineCompanyDTO = new CreateAirlineCompanyDTO
            {
                Name = "Arkia",
                CountryId = 1,
                User = new CreateUserDTO
                {
                    UserName = "Arkiaa",
                    Password = "Pass1234",
                    Email = "Arkia@Arkia.com",
                }
            };

            await TestHelpers.Airline_Company_Login(_httpClient, createAirlineCompanyDTO);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            await TestHelpers.Airline_Company_Login(_httpClient);

            UpdateFlightDTO updateFlightDTO = new UpdateFlightDTO
            {
                Id = 1,
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(13),
                LandingTime = DateTime.Now.AddHours(17),
                RemainingTickets = 10
            };

            var putResponse = await _httpClient.PutAsync($"api/flights/1",
                new StringContent(JsonSerializer.Serialize(updateFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.AreEqual(HttpStatusCode.Forbidden, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Flight_With_Dates_In_Past_Should_Return_Bad_Request()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateFlightDTO updateFlightDTO = new UpdateFlightDTO
            {
                Id = flightResult.Id,
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(-10),
                LandingTime = DateTime.Now.AddHours(-6),
                RemainingTickets = 10
            };

            var putResponse = await _httpClient.PutAsync($"api/flights/{flightResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);

            var putResponseContent = await putResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(putResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 2);
        }

        [TestMethod]
        public async Task Update_Flight_With_Departure_Date_After_Landing_Date_Should_Return_Bad_Request()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateFlightDTO updateFlightDTO = new UpdateFlightDTO
            {
                Id = flightResult.Id,
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(10),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 10
            };

            var putResponse = await _httpClient.PutAsync($"api/flights/{flightResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);

            var putResponseContent = await putResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidationProblemDetails>(putResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(result.Errors.Count, 2);
        }

        [TestMethod]
        public async Task Update_Flight_With_Country_Id_That_Not_Exists_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(12),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 15
            };

            var response = await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var responseContent = await response.Content.ReadAsStringAsync();
            Flight flightResult = JsonSerializer.Deserialize<Flight>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateFlightDTO updateFlightDTO = new UpdateFlightDTO
            {
                Id = flightResult.Id,
                OriginCountryId = 2,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(10),
                LandingTime = DateTime.Now.AddHours(16),
                RemainingTickets = 10
            };

            var putResponse = await _httpClient.PutAsync($"api/flights/{flightResult.Id}",
                new StringContent(JsonSerializer.Serialize(updateFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NotFound, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Airline_Details()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            UpdateAirlineCompanyDTO updateAirlineCompanyDTO = new UpdateAirlineCompanyDTO
            {
                Id = 1,
                Name = "Arkia",
                CountryId = 1
            };

            var putResponse = await _httpClient.PutAsync($"api/airline-companies",
                new StringContent(JsonSerializer.Serialize(updateAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.AreEqual(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Airline_Details_With_Country_Id_That_Not_Exist_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            UpdateAirlineCompanyDTO updateAirlineCompanyDTO = new UpdateAirlineCompanyDTO
            {
                Id = 1,
                Name = "Arkia",
                CountryId = 2
            };

            var putResponse = await _httpClient.PutAsync($"api/airline-companies",
                new StringContent(JsonSerializer.Serialize(updateAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.AreEqual(HttpStatusCode.NotFound, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Update_Airline_Details_With_Another_Id_Should_Return_Forbidden()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            UpdateAirlineCompanyDTO updateAirlineCompanyDTO = new UpdateAirlineCompanyDTO
            {
                Id = 2,
                Name = "Arkia",
                CountryId = 1
            };

            var putResponse = await _httpClient.PutAsync($"api/airline-companies",
                new StringContent(JsonSerializer.Serialize(updateAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.AreEqual(HttpStatusCode.Forbidden, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task Get_All_Tickets_By_Flight()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(2),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 15
            };
            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            await TestHelpers.Customer_Login(_httpClient);

            long flight_id = 1;

            await _httpClient.PostAsync("api/tickets",
             new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

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

            await _httpClient.PostAsync("api/tickets",
            new StringContent(JsonSerializer.Serialize(flight_id), Encoding.UTF8, MediaTypeNames.Application.Json));

            await TestHelpers.Airline_Company_Login(_httpClient, create_airline: false);

            var response = await _httpClient.GetAsync($"api/airline-company/flights/{flight_id}/tickets");
            var responseContent = await response.Content.ReadAsStringAsync();
            List<TicketDetailsDTO> ticketsResult = JsonSerializer.Deserialize<List<TicketDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(ticketsResult.Count, 2);
        }

        [TestMethod]
        public async Task Get_All_Tickets_By_Flight_That_has_No_Tickets_Should_Return_No_Content()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 1,
                DepartureTime = DateTime.Now.AddHours(2),
                LandingTime = DateTime.Now.AddHours(6),
                RemainingTickets = 15
            };
            await _httpClient.PostAsync("api/flights",
             new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));

            var response = await _httpClient.GetAsync($"api/airline-company/flights/1/tickets");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_All_Tickets_By_Flight_That_Not_Exists_Should_Return_Not_Found()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/airline-company/flights/1/tickets");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_All_Tickets_By_Airline_That_has_No_Tickets_Should_Return_No_Content()
        {
            await TestHelpers.Airline_Company_Login(_httpClient);

            var response = await _httpClient.GetAsync($"api/airline-company/tickets");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
