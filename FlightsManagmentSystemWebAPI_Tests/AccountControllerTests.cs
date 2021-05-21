using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Authentication;
using FlightsManagmentSystemWebAPI.Controllers;
using FlightsManagmentSystemWebAPI.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightsManagmentSystemWebAPI_Tests
{
    [TestClass]
    public class AccountControllerTests
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
        public async Task ShouldExpect401WhenLoginWithInvalidCredentials()
        {
            var credentials = new LoginRequest//Demi invalid credentials
            {
                UserName = "admin",
                Password = "invalidPassword"
            };
            var response = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));//post the invalid credentials to the web api
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);//Expect Unauthorized (401) status code to return 
        }

        [TestMethod]
        public async Task ShouldReturnCorrectResponseForSuccessLogin()
        {
            var credentials = new LoginRequest//Demi credentials
            {
                UserName = "admin",
                Password = "9999"
            };
            var loginResponse = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));//post to the controller
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);//Expect status code 200 ok

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();//Get response content as json string
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent);//Desirialize the json string back to LoginResult
            Assert.AreEqual(credentials.UserName, loginResult.UserName);//Username in the LoginResult is the same as the username passed to the contoller
            Assert.AreEqual(UserRoles.Administrator.ToString(), loginResult.Role);//User role is administrator
            Assert.IsFalse(string.IsNullOrWhiteSpace(loginResult.AccessToken));//Access Token is not null and not empty
            Assert.IsFalse(string.IsNullOrWhiteSpace(loginResult.RefreshToken));//Refresh Token is not null and not empty

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            var (principal, jwtSecurityToken) = jwtAuthManager.DecodeJwtToken(loginResult.AccessToken);//User DecodeJwtToken from IJwtAuthManager sevice and get the claims+JWTSecurityToken
            Assert.AreEqual(credentials.UserName, principal.Identity.Name);//The username in the claims should be the same as the username passed to the contoller
            Assert.AreEqual(UserRoles.Administrator.ToString(), principal.FindFirst(ClaimTypes.Role).Value);//User role in the claims is administrator
            Assert.IsNotNull(jwtSecurityToken);

            //Maybe test later that LoginToken in the claims is also valid
        }

        [TestMethod]
        public async Task ShouldBeAbleToLogout()
        {
            var credentials = new LoginRequest//Demi credentials
            {
                UserName = "admin",
                Password = "9999"
            };
            var loginResponse = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));//post to the controller
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();//Get response content as json string
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent);//Desirialize the json string back to LoginResult

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            Assert.IsTrue(jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.ContainsKey(loginResult.RefreshToken));//Check that the refresh tokens dictionary contains the refresh token recived in login result

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResult.AccessToken);//Set the Jwt access token in the request header
            var logoutResponse = await _httpClient.PostAsync("api/account/logout", null);//Post to the contoller to logout
            Assert.AreEqual(HttpStatusCode.OK, logoutResponse.StatusCode);//The response code is 200 ok
            Assert.IsFalse(jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.ContainsKey(loginResult.RefreshToken));//The refresh token dictionary no longer contains the refresh token
        }

        [TestMethod]
        public async Task ShouldCorrectlyRefreshToken()
        {
            const string userName = "admin";//Demi username
            var claims = new[]//Demi claims
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Administrator.ToString())
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            var jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-1));//Generate tokens

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult.AccessToken);//Set the Jwt access token in the request header
            var refreshRequest = new RefreshTokenRequest//Demi refresh token request that holds the refresh token recieved from the JWTAuthResult
            {
                RefreshToken = jwtResult.RefreshToken.TokenString
            };
            var response = await _httpClient.PostAsync("api/account/refresh-token",
                new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, MediaTypeNames.Application.Json));//post to the controller to refresh the token
            var responseContent = await response.Content.ReadAsStringAsync();//Get response content as json string
            var result = JsonSerializer.Deserialize<LoginResult>(responseContent);//Desirialize the json string back to LoginResult
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);//The response code is 200 ok

            var refreshToken2 = jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.GetValueOrDefault(result.RefreshToken);//Get the refresh token from the dictionary
            Assert.AreEqual(refreshToken2.TokenString, result.RefreshToken);//The refresh token in the dictionary is the same as the refresh token recieved after refresh
            Assert.AreNotEqual(refreshToken2.TokenString, jwtResult.RefreshToken.TokenString);//The refresh token in the dictionary is not the same as the refresh token before the refresh
            Assert.AreNotEqual(jwtResult.AccessToken, result.AccessToken);//The refresh token before the refresh is not the same as the refresh token after the refresh
        }


        [TestMethod]
        public async Task ShouldNotAllowToRefreshTokenWhenRefreshTokenIsExpired()
        {
            const string userName = "admin";//Demi username
            var claims = new[]//Demi claims
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Administrator.ToString())
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            var jwtTokenConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();//Get the JwtTokenConfig service from the service collection
            var jwtResult1 = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-jwtTokenConfig.RefreshTokenExpiration - 1));//Generate expired tokens
            var jwtResult2 = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-1));//Generate valid tokens

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult2.AccessToken); //Set valid JWT token in the request header
            var refreshRequest = new RefreshTokenRequest//Set expired refresh token in the RefreshTokenRequest
            {
                RefreshToken = jwtResult1.RefreshToken.TokenString
            };
            var response = await _httpClient.PostAsync("api/account/refresh-token",
                new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, MediaTypeNames.Application.Json)); //Post expired RefreshTokenRequest to the contoller
            var responseContent = await response.Content.ReadAsStringAsync();//Get response content as json string
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);//The response code is 401 Unauthorized 
            Assert.AreEqual("Invalid token", JsonSerializer.Deserialize<string>(responseContent));//The response content should be Invalid token
        }

        [TestMethod]
        public async Task ChangeMyPassword()
        {
            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);
            var response = await _httpClient.PutAsync("api/account/change-password",
            new StringContent(JsonSerializer.Serialize(new ChangePasswordRequest { OldPassword = createCustomerDTO.User.Password, NewPassword = "SomeNewPass" }), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task ChangeMyPassword_With_Old_Password_Should_Return_Bad_Request()
        {
            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);
            var response = await _httpClient.PutAsync("api/account/change-password",
            new StringContent(JsonSerializer.Serialize(new ChangePasswordRequest { OldPassword = createCustomerDTO.User.Password, NewPassword = createCustomerDTO.User.Password }), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task ChangeMyPassword_With_Incorrect_Password_Should_Return_Bad_Request()
        {
            CreateCustomerDTO createCustomerDTO = await TestHelpers.Customer_Login(_httpClient);
            var response = await _httpClient.PutAsync("api/account/change-password",
            new StringContent(JsonSerializer.Serialize(new ChangePasswordRequest { OldPassword = "SomeIncorrectPass", NewPassword = "SomeNewPass" }), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

