namespace Duha.SIMS.Config
{
    public class APIConfiguration : APIConfigRoot
    {
        public string ApiDbConnectionString { get; set; }
        public string JwtTokenSigningKey { get; set; }
        public double DefaultTokenValidityDays { get; set; }
        public string JwtIssuerName { get; set; }
        public string AuthTokenEncryptionKey { get; set; }
        public string AuthTokenDecryptionKey { get; set; }

        public int Time { get; set; }

    }
}
