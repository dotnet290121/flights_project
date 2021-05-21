using System;
using System.Security.Claims;
using Domain.Entities;
using FlightsManagmentSystemWebAPI.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightsManagmentSystemWebAPI_Tests
{
    [TestClass]
    public class JwtAuthManagerTests
    {
        private readonly TestHostFixture _testHostFixture = new TestHostFixture();//Initializes the webHost
        private IServiceProvider _serviceProvider;//Service provider used to provide services that registered in the API

        [TestInitialize]
        public void SetUp()
        {
            _serviceProvider = _testHostFixture.ServiceProvider;
        }

        [TestMethod]
        public void ShouldLoadCorrectJwtConfig()
        {
            var jwtConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();//Get the JwtTokenConfig service from the service collection
            Assert.AreEqual("MySecretKeyIsLongEnough", jwtConfig.Secret);//Validate the secret
            Assert.AreEqual(60, jwtConfig.AccessTokenExpiration);//Validate AccessTokenExpiration
            Assert.AreEqual(2160, jwtConfig.RefreshTokenExpiration);//Validate RefreshTokenExpiration
        }

        [TestMethod]
        public void ShouldRotateRefreshToken()
        {
            var jwtConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();//Get the JwtTokenConfig service from the service collection
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            var now = DateTime.Now;
            const string userName = "admin";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Administrator.ToString())
            };

            var tokens1 = jwtAuthManager.GenerateTokens(userName, claims, now.AddMinutes(-jwtConfig.AccessTokenExpiration));//Generate Expired access token
            var tokens2 = jwtAuthManager.Refresh(tokens1.RefreshToken.TokenString, tokens1.AccessToken, now);//Refresh the tokens

            Assert.AreNotEqual(tokens1.AccessToken, tokens2.AccessToken);//Access token before refresh not the same as after refresh
            Assert.AreNotEqual(tokens1.RefreshToken.TokenString, tokens2.RefreshToken.TokenString);//Access token before refresh not the same as after refresh
            Assert.AreEqual(now.AddMinutes(jwtConfig.RefreshTokenExpiration - jwtConfig.AccessTokenExpiration), tokens1.RefreshToken.ExpireAt);//Check that the first refresh token experation time is correct 
            Assert.AreEqual(now.AddMinutes(jwtConfig.RefreshTokenExpiration), tokens2.RefreshToken.ExpireAt);//Check that the second refresh token experation time is correct 
            Assert.AreEqual(userName, tokens2.RefreshToken.UserName);//Check that the second refresh token username is the same as the username provided in the beggining
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenRefreshTokenUsingAnExpiredToken()
        {
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            var jwtTokenConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();//Get the JwtTokenConfig service from the service collection
            const string userName = "admin";
            var now = DateTime.Now;
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Administrator.ToString())
            };

            var jwtAuthResult1 = jwtAuthManager.GenerateTokens(userName, claims, now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration - 1).AddSeconds(1));//Generate expried access token but within the time to refresh of one minute
            jwtAuthManager.Refresh(jwtAuthResult1.RefreshToken.TokenString, jwtAuthResult1.AccessToken, now);//refresh not throws an error

            var jwtAuthResult2 = jwtAuthManager.GenerateTokens(userName, claims, now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration - 1));//Genreate expired access token not in the one minute preiod
            Assert.ThrowsException<SecurityTokenExpiredException>(() => jwtAuthManager.Refresh(jwtAuthResult2.RefreshToken.TokenString, jwtAuthResult2.AccessToken, now));//Refresh will throw an exception
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenRefreshTokenIsForgotten()
        {
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();//Get the IJwtAuthManager service from the service collection
            var jwtTokenConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();//Get the JwtTokenConfig service from the service collection
            var now = DateTime.Now;

            var claims1 = new[]
            {
                new Claim(ClaimTypes.Name,"admin"),
                new Claim(ClaimTypes.Role, UserRoles.Administrator.ToString())
            };
            var tokens1 = jwtAuthManager.GenerateTokens("admin", claims1, now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration));//Generate admin tokens

            var claims2 = new[]
            {
                new Claim(ClaimTypes.Name,"test1"),
                new Claim(ClaimTypes.Role, UserRoles.Administrator.ToString())
            };
            var tokens2 = jwtAuthManager.GenerateTokens("test1", claims2, now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration));//Generate Test1 tokens

            // forgot a token: try to use the refresh token for "test1", but use the access token for "admin"
            var e = Assert.ThrowsException<SecurityTokenException>(() => jwtAuthManager.Refresh(tokens2.RefreshToken.TokenString, tokens1.AccessToken, now));
            Assert.AreEqual("Invalid token", e.Message);
        }
    }
}