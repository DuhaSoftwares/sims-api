using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Duha.SIMS.API.Security
{
    public class JwtHandler
    {
        private readonly string _tokenIssuier;

        public JwtHandler(string issuer)
        {
            _tokenIssuier = issuer;
        }

        public async Task<string> ProtectAsync(string encryptionKey, IEnumerable<Claim> lstClaims, DateTimeOffset issueDateOffset, DateTimeOffset expiryDateOffset, string audience)
        {
            if (encryptionKey.Length < 16)
            {
                throw new Exception("Key length should me more the 16 characters");
            }

            SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey)), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            JwtSecurityToken token = new JwtSecurityToken(_tokenIssuier, audience, lstClaims, issueDateOffset.UtcDateTime, expiryDateOffset.UtcDateTime, signingCredentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> UnprotectToJwtStringAsync(string decryptionKey, string token)
        {
            /*if (decryptionKey.Length < 16)
            {
                throw new Exception("Key length should me more the 16 characters");
            }*/

            return token;
        }

        public async Task<JwtSecurityToken> UnprotectAsync(string decryptionKey, string token)
        {
            string text = await UnprotectToJwtStringAsync(decryptionKey, token);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("Could not unprotect token.");
            }

            return ((SecurityTokenHandler)new JwtSecurityTokenHandler()).ReadToken(text) as JwtSecurityToken;
        }
    }
}
