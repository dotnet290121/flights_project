using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Authentication
{
    public class JwtAuthManager : IJwtAuthManager
    {
        // getter for the _userRefreshTokens ConcurrentDictionary
        // Immutable means that the state of the object cannot be changed after it's created
        public IImmutableDictionary<string, RefreshToken> UsersRefreshTokensReadOnlyDictionary => _usersRefreshTokens.ToImmutableDictionary();
        private readonly ConcurrentDictionary<string, RefreshToken> _usersRefreshTokens;  // can store in a database or a distributed cache (maybe redis?)
        private readonly JwtTokenConfig _jwtTokenConfig;//DI for the configuration of the JWT that is registered as singleton
        private readonly byte[] _secret;//The secret key that is used to sign the tokens

        public JwtAuthManager(JwtTokenConfig jwtTokenConfig)
        {
            _jwtTokenConfig = jwtTokenConfig;
            _usersRefreshTokens = new ConcurrentDictionary<string, RefreshToken>();
            _secret = Encoding.UTF8.GetBytes(jwtTokenConfig.Secret);//Get the secret from the configuration and encode it
        }

        /// <summary>
        /// Removes all the expired refresh tokens from the dictionary
        /// </summary>
        /// <param name="now">Current time</param>
        public void RemoveExpiredRefreshTokens(DateTime now)//Might want to run once a day like th backuo
        {
            var expiredTokens = _usersRefreshTokens.Where(x => x.Value.ExpireAt < now).ToList();//Gets all the expired tokens from the dictionary into list
            foreach (var expiredToken in expiredTokens)
                _usersRefreshTokens.TryRemove(expiredToken.Key, out _);//run over the list and try to remove each token from the dictionary

        }

        /// <summary>
        /// Remove the refresh token of given user
        /// </summary>
        /// <param name="userName">The username of the user</param>
        public void RemoveRefreshTokenByUserName(string userName) // can be more specific to ip, user agent, device name, etc,
                                                                  // so that if I logout from the computer I would still remain logged in on other devices
        {
            var refreshTokens = _usersRefreshTokens.Where(x => x.Value.UserName == userName).ToList();//Gets all the refresh tokens of the user from the dictionary into a list
            foreach (var refreshToken in refreshTokens)
                _usersRefreshTokens.TryRemove(refreshToken.Key, out _);//run over the list and try to remove each token from the dictionary
        }


        /// <summary>
        /// Generate the JWT access token and the refresh token
        /// </summary>
        /// <param name="username">Name of the user for which to generate the token</param>
        /// <param name="claims">The claims to include in the JWT access token</param>
        /// <param name="now">Current time</param>
        /// <returns>JwtAuthResult containing the JWT access token and the refresh token</returns>
        public JwtAuthResult GenerateTokens(string username, Claim[] claims, DateTime now)
        {
            bool shouldAddAudienceClaim = string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);//checks whether the Audience is not defined in the claims
            var jwtToken = new JwtSecurityToken(
                _jwtTokenConfig.Issuer,//Provide the issuer from the configuration
                shouldAddAudienceClaim ? _jwtTokenConfig.Audience : string.Empty,//If the Audience is not provided in the claims, add audience from configuration else leave it empty, it will be added later
                claims,//the provided claims (might include the audience)
                expires: now.AddMinutes(_jwtTokenConfig.AccessTokenExpiration),//set experation time as per the value from configuration
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256Signature));//Sign the token with the secret key
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);//Write the token into a string

            var refreshToken = new RefreshToken//Create refresh token
            {
                UserName = username,//The name of the user it belongs to
                TokenString = GenerateRefreshTokenString(),//Generate string as a token
                ExpireAt = now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration)//Set experation
            };
            _usersRefreshTokens.AddOrUpdate(refreshToken.TokenString, refreshToken, (s, t) => refreshToken);//Adds or updates a value in the refresh tokens dictonary 

            return new JwtAuthResult //Return the result containing both JWT token and refresh token
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Refresh and generate new JWT access token and refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token hold by the client</param>
        /// <param name="accessToken">The access token hold by the client</param>
        /// <param name="now">Current time</param>
        /// <returns></returns>
        public JwtAuthResult Refresh(string refreshToken, string accessToken, DateTime now)
        {
            var (principal, jwtToken) = DecodeJwtToken(accessToken);//Decode the JWT access token to confirm that we get an authentic identity and get the claims principal + JwtSecurityToken
            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature))//If the token is null or the security algorithm not equal to HmacSha256Signature throw an exception
                throw new SecurityTokenException("Invalid token");

            var userName = principal.Identity?.Name;
            if (!_usersRefreshTokens.TryGetValue(refreshToken, out var existingRefreshToken))//If the refresh token not exists in dictionary throw an exception
                throw new SecurityTokenException("Invalid token");

            if (existingRefreshToken.UserName != userName || existingRefreshToken.ExpireAt < now)//If the username in the dictionary is different than the username in the claims principal throw an exception
                throw new SecurityTokenException("Invalid token");

            //Generate the tokens and return them
            return GenerateTokens(userName, principal.Claims.ToArray(), now); //This is why need to recover the original claims (claims principal)
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public (ClaimsPrincipal, JwtSecurityToken) DecodeJwtToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))//If the token not provided throw exception
                throw new SecurityTokenException("Invalid token");

            var principal = new JwtSecurityTokenHandler()//Read the claims into the claim princiapl
                .ValidateToken(token,                   //Validates the token against the validation parameters
                    new TokenValidationParameters       //The parameters must match those in the startup class    
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _jwtTokenConfig.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(_secret),
                        ValidAudience = _jwtTokenConfig.Audience,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    },
                    out var validatedToken);// out SecurityToken
            return (principal, validatedToken as JwtSecurityToken);
        }

        /// <summary>
        /// Generate random string as refresh tokem
        /// </summary>
        /// <returns></returns>
        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];//new byte array
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);//fill the array with random numbers

            return Convert.ToBase64String(randomNumber);//Genrate string from the array
        }
    }
}
