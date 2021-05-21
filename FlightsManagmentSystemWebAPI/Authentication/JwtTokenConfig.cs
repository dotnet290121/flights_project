
namespace FlightsManagmentSystemWebAPI.Authentication
{
    /// <summary>
    /// Holder for the configuration values required for the jwt authentication
    /// </summary>
    public class JwtTokenConfig
    {
        /// <summary>
        /// Secret key to sign the tokens
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// The issuer of the assigned tokens
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// The audience of the assigned tokens
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// Exparation time of the token in minutes
        /// </summary>
        public int AccessTokenExpiration { get; set; }
        /// <summary>
        /// Exparation time of the refresh token in minutes
        /// </summary>
        public int RefreshTokenExpiration { get; set; }
    }

}
