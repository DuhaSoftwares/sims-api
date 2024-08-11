namespace Duha.SIMS.ServiceModels.v1.General.Token
{
    public class TokenResponseRoot
    {
        public string AccessToken { get; set; }

        public DateTime ExpiresUtc { get; set; }
    }
}