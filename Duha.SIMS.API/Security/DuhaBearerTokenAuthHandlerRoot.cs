using Duha.SIMS.BAL.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Duha.SIMS.API.Security
{
    public class DuhaBearerTokenAuthHandlerRoot : AuthenticationHandler<DuhaAuthenticationSchemeOptions>
    {
        private string _failureMsg = string.Empty;
        private readonly JwtHandler _jwtHandler;

        public const string DefaultSchema = "RenoBearerSchema";

        public DuhaBearerTokenAuthHandlerRoot(IOptionsMonitor<DuhaAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, JwtHandler jwtHandler)
            : base(options, logger, encoder, clock)
        {
            _jwtHandler = jwtHandler;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                string tokenString = GetRequestBearerTokenValue("Authorization");
                if (!string.IsNullOrEmpty(tokenString))
                {
                    AuthenticationTicket authTicket = await ValidateTokenAndGetTicket(tokenString);
                    if (authTicket != null)
                    {
                        DateTimeOffset utcNow = base.Clock.UtcNow;
                        DateTimeOffset? expiresUtc = authTicket.Properties.ExpiresUtc;
                        if (utcNow > expiresUtc)
                        {
                            return GetFailureResult("Token is expired.");
                        }

                        return AuthenticateResult.Success(authTicket);
                    }

                    return GetFailureResult("Could not unprotect token");
                }

                return GetFailureResult("Token is null or empty.");
            }
            catch (Exception)
            {
                return GetFailureResult("Could not unprotect token");
            }
        }

        protected string GetRequestBearerTokenValue(string key)
        {
            if (base.Request.Headers.TryGetValue(key, out var value))
            {
                string text = value.ToString();
                return (text.Split(' ').Count() > 1) ? text.Split(' ')[1] : "";
            }

            return null;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            return base.HandleForbiddenAsync(properties).ContinueWith(delegate
            {
                if (!string.IsNullOrWhiteSpace(_failureMsg))
                {
                    base.Response.Body.WriteAsync(Encoding.ASCII.GetBytes(_failureMsg));
                }
            });
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            return base.HandleChallengeAsync(properties).ContinueWith(delegate
            {
                if (!string.IsNullOrWhiteSpace(_failureMsg))
                {
                    base.Response.Body.WriteAsync(Encoding.ASCII.GetBytes(_failureMsg));
                }
            });
        }

        protected async Task<AuthenticationTicket> ValidateTokenAndGetTicket(string tokenString)
        {
            JwtSecurityToken jwtAuthTicket = await _jwtHandler.UnprotectAsync(base.OptionsMonitor.CurrentValue.JwtTokenSigningKey, tokenString);
            if (jwtAuthTicket != null)
            {
                AuthenticationProperties authProps = new AuthenticationProperties
                {
                    IssuedUtc = jwtAuthTicket.ValidFrom.ToUniversalTime(),
                    ExpiresUtc = jwtAuthTicket.ValidTo.ToUniversalTime()
                };
                ClaimsIdentity claimId = new ClaimsIdentity(jwtAuthTicket.Claims, "Bearer");
                ClaimsPrincipal claimPrin = new ClaimsPrincipal(claimId);
                return new AuthenticationTicket(claimPrin, authProps, "DuhaBearerSchema");
            }

            return null;
        }

        protected AuthenticateResult GetFailureResult(string message)
        {
            AuthenticationProperties properties = new AuthenticationProperties();
            _failureMsg = message;
            return AuthenticateResult.Fail(message, properties);
        }
    }
}