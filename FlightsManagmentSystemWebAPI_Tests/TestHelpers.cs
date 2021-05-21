using Domain.Entities;
using Domain.Interfaces;
using FlightsManagmentSystemWebAPI.Controllers;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI_Tests
{
    internal static class TestHelpers
    {
        internal static async Task Create_Country_For_Tests(HttpClient httpClient, CreateCountryDTO createCountryDTO = null)
        {
            await Main_Admin_Login(httpClient);

            if (createCountryDTO == null)
                createCountryDTO = new CreateCountryDTO { Name = "Israel" };

            await httpClient.PostAsync("api/countries",
                new StringContent(JsonSerializer.Serialize(createCountryDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        private static async Task Create_Airline_Company_For_Tests(HttpClient httpClient, CreateAirlineCompanyDTO createAirlineCompanyDTO)
        {
            await httpClient.PostAsync("api/airline-companies",
             new StringContent(JsonSerializer.Serialize(createAirlineCompanyDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        internal static async Task Create_Flight_For_Tests(HttpClient httpClient, CreateFlightDTO createFlightDTO = null)
        {

            if (createFlightDTO == null)
            {
                createFlightDTO = new CreateFlightDTO
                {
                    OriginCountryId = 1,
                    DestinationCountryId = 1,
                    DepartureTime = DateTime.Now.AddHours(2),
                    LandingTime = DateTime.Now.AddHours(6),
                    RemainingTickets = 15
                };
            }

            await httpClient.PostAsync("api/flights",
                        new StringContent(JsonSerializer.Serialize(createFlightDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        internal static async Task<CreateCustomerDTO> Customer_Login(HttpClient httpClient, CreateCustomerDTO createCustomerDTO = null)
        {
            if (createCustomerDTO == null)
            {
                createCustomerDTO = new CreateCustomerDTO
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
            }

            await Main_Admin_Login(httpClient);

            await Create_Customer_For_Tests(httpClient, createCustomerDTO);

            var credentials = new LoginRequest//Demi credentials
            {
                UserName = createCustomerDTO.User.UserName,
                Password = createCustomerDTO.User.Password
            };
            var loginResponse = await httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();//Get response content as json string
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent);//Desirialize the json string back to LoginResult

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResult.AccessToken);//Set the Jwt access token in the request header

            return createCustomerDTO;
        }

        private static async Task Create_Customer_For_Tests(HttpClient httpClient, CreateCustomerDTO createCustomerDTO)
        {
            await httpClient.PostAsync("api/customers",
                       new StringContent(JsonSerializer.Serialize(createCustomerDTO), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        internal static async Task Main_Admin_Login(HttpClient httpClient)
        {
            var credentials = new LoginRequest//Demi credentials
            {
                UserName = "admin",
                Password = "9999"
            };
            var loginResponse = await httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();//Get response content as json string
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent);//Desirialize the json string back to LoginResult

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResult.AccessToken);//Set the Jwt access token in the request header
        }

        internal static async Task Airline_Company_Login(HttpClient httpClient, CreateAirlineCompanyDTO createAirlineCompanyDTO = null, bool create_airline = true)
        {
            if (createAirlineCompanyDTO == null)
            {
                createAirlineCompanyDTO = new CreateAirlineCompanyDTO
                {
                    Name = "El Al",
                    CountryId = 1,
                    User = new CreateUserDTO
                    {
                        UserName = "ElAlIL",
                        Password = "Pass1234",
                        Email = "Elal@Elal.com",
                    }
                };
            }

            if (create_airline)
            {
                await Main_Admin_Login(httpClient);

                await Create_Airline_Company_For_Tests(httpClient, createAirlineCompanyDTO);
            }

            var credentials = new LoginRequest//Demi credentials
            {
                UserName = createAirlineCompanyDTO.User.UserName,
                Password = createAirlineCompanyDTO.User.Password
            };
            var loginResponse = await httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();//Get response content as json string
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent);//Desirialize the json string back to LoginResult

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResult.AccessToken);//Set the Jwt access token in the request header
        }

        internal static void CompareProps(object object_a, object object_b, bool ignore_user = false)
        {

            Type type_a = object_a.GetType();
            Type type_b = object_b.GetType();
            Assert.AreEqual(type_a, type_b);
            PropertyInfo[] props_a = type_a.GetProperties();
            PropertyInfo[] props_b = type_a.GetProperties();

            for (int i = 0; i < props_a.Length; i++)
            {
                Type prop_type = props_a[i].PropertyType;
                if (ignore_user && prop_type == typeof(User))
                    continue;

                if (prop_type.GetInterfaces().Contains(typeof(IPoco)) || prop_type.GetInterfaces().Contains(typeof(IDetailsDTO)))
                {
                    if (props_a[i].GetValue(object_a) == null && props_a[i].GetValue(object_b) == null)
                        continue;

                    CompareProps(props_a[i].GetValue(object_a), props_a[i].GetValue(object_b), ignore_user);
                }
                else if (prop_type == typeof(DateTime))
                {
                    CompareDates((DateTime)props_a[i].GetValue(object_a), (DateTime)props_b[i].GetValue(object_b));
                }
                else
                {
                    Assert.AreEqual(props_a[i].GetValue(object_a), props_b[i].GetValue(object_b));
                }
            }
        }

        internal static void CompareDates(DateTime object_a, DateTime object_b)
        {
            Assert.AreEqual(object_a.Year, object_b.Year);
            Assert.AreEqual(object_a.Month, object_b.Month);
            Assert.AreEqual(object_a.Day, object_b.Day);
            Assert.AreEqual(object_a.Hour, object_b.Hour);
            Assert.AreEqual(object_a.Minute, object_b.Minute);
        }
    }
}
