using BL;
using BL.LoginService;
using DAL;
using Domain.Entities;
using Domain.Interfaces;
using FlightsManagmentSystemWebAPI.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IFlightCenterSystem _flightCenterSystem;
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IFlightCenterSystem flightCenterSystem, IJwtAuthManager jwtAuthManager, ILogger<AccountController> logger)
        {
            _flightCenterSystem = flightCenterSystem;
            _jwtAuthManager = jwtAuthManager;
            _logger = logger;
        }

        /// <summary>
        /// Login to the system
        /// </summary>
        /// <returns>Login Result</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /LoginRequest
        ///     {
        ///        "username": "CustomerOne",
        ///        "password": "Pass1234"
        ///     }
        /// </remarks>  
        /// <param name="request">Login request that holds the login credentials</param>
        /// <response code="200">Returns Successful Login Result</response>
        /// <response code="400">If one or more of the credential are empty</response> 
        /// <response code="401">If the user credentials are wrong</response> 
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        public ActionResult<LoginResult> Login([FromBody] LoginRequest request)
        {
            if (!_flightCenterSystem.TryLogin(request.UserName, request.Password, out ILoginToken loginToken, out FacadeBase facade))//validate the login credentials
                return Unauthorized();

            dynamic user = loginToken.GetType().GetProperties()[0].GetValue(loginToken);//Get the user from the login token as dynamic object

            string role = user.User.UserRole.ToString();//get the user role

            var claims = new[]//generate claims array
            {
                new Claim(ClaimTypes.Name, user.User.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.User.Id.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("LoginToken", loginToken.ToString()),
            };

            var jwtResult = _jwtAuthManager.GenerateTokens(request.UserName, claims, DateTime.Now);//Invoke GenerateTokens and get JWTAuthResult
            _logger.LogInformation($"User [{request.UserName}] logged in the system.");
            return Ok(new LoginResult
            {
                UserName = request.UserName,
                Role = role,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }

        /// <summary>
        /// Logout of the system
        /// </summary>
        /// <returns>Login Result</returns>
        /// <response code="200">Successful Logout</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Logout()//The logout method invalidates the refresh token on the server-side,
        {                           //In order to invalidate the JWT access token on the server-side block-list strategy can be used or just keep the exp of the token short 
            var userName = User.Identity.Name;
            _jwtAuthManager.RemoveRefreshTokenByUserName(userName);//remove the refresh token from the dictionary
            _logger.LogInformation($"User [{userName}] logged out the system.");
            return Ok();
        }

        /// <summary>
        /// Refresh the JWT access token
        /// </summary>
        /// <returns>Login Result</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /LoginRequest
        ///     {
        ///        "refreshToken": "GGm6UtAP8Eqh6YdRga/aRxzGNydu/tq/39eQ8EfFtHA="
        ///     }
        ///
        /// </remarks>  
        /// <param name="request">Refresh token request that holds the existing refresh token</param>
        /// <response code="200">Returns Successful Login Result after refreshing</response>
        /// <response code="401">If refresh token not provided or if not valid token</response> 
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var userName = User.Identity.Name;
                _logger.LogInformation($"User [{userName}] is trying to refresh JWT token.");

                if (string.IsNullOrWhiteSpace(request.RefreshToken))//check if the request token is not provided
                    return Unauthorized();


                var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");//Get the Jwt access token
                var jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken, DateTime.Now);//Refresh the token and get JWTAuthResult
                _logger.LogInformation($"User [{userName}] has refreshed JWT token.");
                return Ok(new LoginResult
                {
                    UserName = userName,
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message); // return 401 so that the client side can redirect the user to login page
            }
        }

        /// <summary>
        /// Change password of the logged-in user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /ChangePasswordRequest
        ///     {
        ///        "oldPassword": "oldpass123",
        ///        "newPassword": "newpass123"
        ///     }
        ///
        /// </remarks>  
        /// <param name="request">Change password request that holds the old and new passwords</param>
        /// <response code="204">The password was changed successfully</response>
        /// <response code="400">If the old and new passwords are the same. or if the old password is incorrect</response> 
        /// <response code="404">If user not exists any more</response> 
        [HttpPut("change-password")]
        public IActionResult ChangeMyPassword(ChangePasswordRequest request)
        {
            if (request.OldPassword == request.NewPassword)
                return BadRequest();

            IUserDAO userDAOPGSQL = new UserDAOPGSQL();

            string username= User.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;

            User user =userDAOPGSQL.GetUserByUserName(username);

            if (user == null)
                return NotFound();

            if (user.Password != request.OldPassword)
                return BadRequest();

            user.Password = request.NewPassword;
            userDAOPGSQL.Update(user);

            return NoContent();
        }
    }

    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Login result model
    /// </summary>
    public class LoginResult
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Refresh token request model
    /// </summary>
    public class RefreshTokenRequest
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Change password request model
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required]
        [JsonPropertyName("oldPassword")]
        public string OldPassword { get; set; }

        [Required]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; }
    }
}
