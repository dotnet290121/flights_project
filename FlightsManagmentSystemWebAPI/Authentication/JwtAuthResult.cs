using System.Text.Json.Serialization;

namespace FlightsManagmentSystemWebAPI.Authentication
{
    /// <summary>
    /// The result model that returned when user is authenticated
    /// </summary>
    public class JwtAuthResult
    {
        /// <summary>
        /// The JWT access token
        /// </summary>
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        /// <summary>
        /// A refresh token
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public RefreshToken RefreshToken { get; set; }
    }
}
