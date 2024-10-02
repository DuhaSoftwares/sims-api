using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.API.Security;
using Duha.SIMS.BAL.AppUser;
using Duha.SIMS.BAL.Token.Base;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.OData.Query;

namespace Duha.SIMS.API.Controllers.AppUsers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ApplicationUserController : ApiControllerWithOdataRoot<ApplicationUserSM>
    {
        private readonly ApplicationUserProcess _applicationUserProcess;
        public ApplicationUserController(ApplicationUserProcess applicationUserProcess) : base(applicationUserProcess)
        {
            _applicationUserProcess = applicationUserProcess;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = " SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationUserSM>>>> GetAsOdata(ODataQueryOptions<ApplicationUserSM> oDataOptions)
        {
            
            var retList = await GetAsEntitiesOdata(oDataOptions);

            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }



        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = " SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ApplicationUserSM>>> GetById(int id)
        {
            var singleSM = await _applicationUserProcess.GetApplicationUserById(id);
            if (singleSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(singleSM);
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotFound, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet()]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = " SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationUserSM>>>> GetAll()
        {
            var singleSM = await _applicationUserProcess.GetAllApplicationUsers();
            return Ok(ModelConverter.FormNewSuccessResponse(singleSM));
        }

        [HttpPost("signUp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ApplicationUserSM>>> Post([FromBody] ApiRequest<ApplicationUserSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _applicationUserProcess.SignUpApplicationUser(innerReq);
            if (addedSM != null)
            {
                return CreatedAtAction(nameof(GetById), new
                {
                    id = addedSM.Id
                }, ModelConverter.FormNewSuccessResponse(addedSM));
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPut()]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ApplicationUserSM>>> UpdateApplicationUser([FromBody] ApiRequest<ApplicationUserSM> apiRequest)
        {
            #region Check Request
            var innerReq = apiRequest?.ReqData;
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized Admin...Plz check your Credentials"));
            }
            else
            {
                var userId = User.GetUserRecordIdFromCurrentUserClaims();
                var userRole = User.GetUserRoleTypeFromCurrentUserClaims();
                if (userId <= 0 )
                {
                    return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdInvalid, ApiErrorTypeSM.InvalidInputData_NoLog));
                }

                if (innerReq == null)
                {
                    return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
                }

                else
                {
                    var response = await _applicationUserProcess.UpdateApplicationUserDetails(userId, innerReq, userRole);
                    return Ok(ModelConverter.FormNewSuccessResponse(response));
                }
            }

            #endregion Check Request
        }

        #region Delete Endpoints

         [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SystemAdmin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            var userRole = User.GetUserRoleTypeFromCurrentUserClaims();
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            var companyCode = User.GetCompanyCodeFromCurrentUserClaims();

            var resp = await _applicationUserProcess.DeleteApplicationUserById(userId, userRole);
            if (resp != null && resp.DeleteResult)
            {
                return Ok(ModelConverter.FormNewSuccessResponse(resp));
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(resp?.DeleteMessage, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        #endregion Delete Endpoints
    }
}
