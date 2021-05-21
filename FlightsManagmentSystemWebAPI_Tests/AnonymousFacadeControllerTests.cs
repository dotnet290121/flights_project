using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace FlightsManagmentSystemWebAPI_Tests
{
    [TestClass]
    public class AnonymousFacadeControllerTests
    {
        private readonly TestHostFixture _testHostFixture = new TestHostFixture();// Initializes the webHost
        private HttpClient _httpClient;//Http client used to send requests to the contoller

        [TestInitialize]
        public async Task SetUp()
        {
            _httpClient = _testHostFixture.Client;
            TestsDAOPGSQL.ClearDB();
            await TestHelpers.Create_Country_For_Tests(_httpClient);
            await TestHelpers.Create_Country_For_Tests(_httpClient, new CreateCountryDTO { Name = "Usa" });
            await TestHelpers.Create_Country_For_Tests(_httpClient, new CreateCountryDTO { Name = "Russia" });
            await TestHelpers.Create_Country_For_Tests(_httpClient, new CreateCountryDTO { Name = "Italy" });

            await TestHelpers.Airline_Company_Login(_httpClient);

            CreateFlightDTO createFlightDTO = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 2,
                DepartureTime = DateTime.Now.AddDays(1),
                LandingTime = DateTime.Now.AddDays(2),
                RemainingTickets = 15
            };
            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO);

            CreateFlightDTO createFlightDTO2 = new CreateFlightDTO
            {
                OriginCountryId = 1,
                DestinationCountryId = 2,
                DepartureTime = DateTime.Now.AddDays(2),
                LandingTime = DateTime.Now.AddDays(3),
                RemainingTickets = 15
            };
            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO2);

            CreateFlightDTO createFlightDTO3 = new CreateFlightDTO
            {
                OriginCountryId = 3,
                DestinationCountryId = 4,
                DepartureTime = DateTime.Now.AddDays(2),
                LandingTime = DateTime.Now.AddDays(3),
                RemainingTickets = 15
            };
            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO3);

            CreateFlightDTO createFlightDTO4 = new CreateFlightDTO
            {
                OriginCountryId = 3,
                DestinationCountryId = 4,
                DepartureTime = DateTime.Now.AddDays(4),
                LandingTime = DateTime.Now.AddDays(5),
                RemainingTickets = 15
            };
            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO4);

            CreateFlightDTO createFlightDTO5 = new CreateFlightDTO
            {
                OriginCountryId = 2,
                DestinationCountryId = 4,
                DepartureTime = DateTime.Now.AddDays(1),
                LandingTime = DateTime.Now.AddDays(2),
                RemainingTickets = 15
            };
            await TestHelpers.Create_Flight_For_Tests(_httpClient, createFlightDTO5);
        }

        [TestMethod]
        public async Task Search_Flight_With_No_Query_Parameters()
        {
            var response = await _httpClient.GetAsync("api/flights/search");
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 5);
        }

        [TestMethod]
        public async Task Search_Flight_By_Origin_Country_Id()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["originCountryId"] = "1";
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 2);
        }

        [TestMethod]
        public async Task Search_Flight_By_Destination_Country_Id()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["detinationCountryId"] = "4";
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 3);
        }

        [TestMethod]
        public async Task Search_Flight_By_Destination_Country_Id_That_Should_Return_No_Content()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["detinationCountryId"] = "1";
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task Search_Flight_By_Origin_And_Destination_Country_Id()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["originCountryId"] = "1";
            query["detinationCountryId"] = "2";
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 2);
        }

        [TestMethod]
        public async Task Search_Flight_By_Departure_Date()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["departureDate"] = DateTime.Now.AddDays(1).ToString();
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 2);
        }


        [TestMethod]
        public async Task Search_Flight_By_Landing_Date()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["landingDate"] = DateTime.Now.AddDays(5).ToString();
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 1);
        }

        [TestMethod]
        public async Task Search_Flight_By_All_Parameters()
        {
            var builder = new UriBuilder();
            builder.Path = "api/flights/search";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["originCountryId"] = "1";
            query["detinationCountryId"] = "2";
            query["departureDate"] = DateTime.Now.AddDays(2).ToString();
            query["landingDate"] = DateTime.Now.AddDays(3).ToString();
            builder.Query = query.ToString();
            string url = builder.ToString();

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            List<FlightDetailsDTO> flightsListResult = JsonSerializer.Deserialize<List<FlightDetailsDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(flightsListResult.Count, 1);
        }
    }
}
