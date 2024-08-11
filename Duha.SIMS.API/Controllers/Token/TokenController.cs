using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.API.Security;
using Duha.SIMS.BAL.Token;
using Duha.SIMS.Config;
using Duha.SIMS.ServiceModels.AppUsers.Login;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.v1.General.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Duha.SIMS.API.Controllers.Token
{
    [Route("api/[controller]")]
    public partial class TokenController : ApiControllerRoot
    {
        private readonly TokenProcess _tokenProcess;
        private readonly JwtHandler _jwtHandler;
        private readonly APIConfiguration _apiConfiguration;
        public TokenController(TokenProcess TokenProcess, JwtHandler jwtHandler, APIConfiguration aPIConfiguration)
        {
            _tokenProcess = TokenProcess;
            _jwtHandler = jwtHandler;
            _apiConfiguration = aPIConfiguration;
        }
        #region ValidateLoginAndGenerateToken

        [HttpPost]
        [Route("ValidateLoginAndGenerateToken")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseSM>>> ValidateLoginAndGenerateToken(ApiRequest<TokenRequestSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_Log));
            }

            if (string.IsNullOrWhiteSpace(innerReq.LoginId) || string.IsNullOrWhiteSpace(innerReq.Password) || innerReq.RoleType == RoleTypeSM.Unknown)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_InvalidRequiredDataInputs));
            }

            #endregion Check Request

            (LoginUserSM userSM, int compId) = await _tokenProcess.ValidateLoginAndGenerateToken(innerReq);
            if (userSM == null)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_UserNotFound,
                    ApiErrorTypeSM.InvalidInputData_Log));
            }
            else if (userSM.LoginStatus == LoginStatusSM.Disabled)
            {
                return Unauthorized(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_UserDisabled, ApiErrorTypeSM.Access_Denied_Log));
            }
            else if (userSM.LoginStatus == LoginStatusSM.PasswordResetRequired)
            {
                return Unauthorized(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_UserPasswordResetRequired, ApiErrorTypeSM.Access_Denied_Log));
            }
            else if (!userSM.IsEmailConfirmed || !userSM.IsPhoneNumberConfirmed)
            {
                return Unauthorized(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_UserNotVerified, ApiErrorTypeSM.Access_Denied_Log));
            }
            else
            {
                ICollection<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,innerReq.LoginId),
                    new Claim(ClaimTypes.Role,innerReq.RoleType.ToString()),
                    new Claim(ClaimTypes.GivenName,userSM.FirstName + " " + userSM.MiddleName + " " +userSM.LastName ),
                    new Claim(ClaimTypes.Email,userSM.EmailId),
                    new Claim(DomainConstantsRoot.ClaimsRoot.Claim_DbRecordId,userSM.Id.ToString())
                };
                if (compId != default)
                {
                    claims.Add(new Claim(DomainConstantsRoot.ClaimsRoot.Claim_ClientCode, innerReq.CompanyCode));
                    claims.Add(new Claim(DomainConstantsRoot.ClaimsRoot.Claim_ClientId, compId.ToString()));
                }
                var expiryDate = DateTime.Now.AddDays(_apiConfiguration.DefaultTokenValidityDays);
                var token = await _jwtHandler.ProtectAsync(_apiConfiguration.JwtTokenSigningKey, claims, new DateTimeOffset(DateTime.Now), new DateTimeOffset(expiryDate), "SIMS");
                // here if user is derived class, all properties will be sent
                var tokenResponse = new TokenResponseSM()
                {
                    AccessToken = token,
                    LoginUserDetails = userSM,
                    ExpiresUtc = expiryDate,
                    ClientCompanyId = compId
                };
                return Ok(ModelConverter.FormNewSuccessResponse(tokenResponse));
            }
        }
        #endregion ValidateLoginAndGenerateToken

    }
}