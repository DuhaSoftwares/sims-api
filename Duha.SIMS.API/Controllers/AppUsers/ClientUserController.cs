using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.API.Security;
using Duha.SIMS.BAL.AppUser;
using Duha.SIMS.BAL.Token.Base;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web.Http.OData.Query;

namespace Duha.SIMS.API.Controllers.AppUsers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientUserController : ApiControllerWithOdataRoot<ClientUserSM>
    {
        private readonly ClientUserProcess _clientUserProcess;
        public ClientUserController(ClientUserProcess clientUserProcess) : base(clientUserProcess)
        {
            _clientUserProcess = clientUserProcess;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SystemAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientUserSM>>>> GetAsOdata(ODataQueryOptions<ClientUserSM> oDataOptions)
        {
            
            var retList = await GetAsEntitiesOdata(oDataOptions);

            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }



        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<ClientUserSM>>> GetById(int id)
        {
            var singleSM = await _clientUserProcess.GetClientUserById(id);
            if (singleSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(singleSM);
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotFound, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }


        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<ClientUserSM>>> GetMineById()
        {
            var id = User.GetUserRecordIdFromCurrentUserClaims();
            var singleSM = await _clientUserProcess.GetClientUserById(id);
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
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SystemAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientUserSM>>>> GetAll()
        {
            var singleSM = await _clientUserProcess.GetAllClientUsers();
            return Ok(ModelConverter.FormNewSuccessResponse(singleSM));
        }

        [HttpPost("signUp")]
        //[AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ClientUserSM>>> Post([FromBody] ApiRequest<ClientUserSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _clientUserProcess.SignUpClientUser(innerReq);
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
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<ClientUserSM>>> UpdateClientUser([FromBody] ApiRequest<ClientUserSM> apiRequest)
        {
            #region Check Request
            var innerReq = apiRequest?.ReqData;
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            else
            {
                var userId = User.GetUserRecordIdFromCurrentUserClaims();
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
                    var response = await _clientUserProcess.UpdateClientUserDetails(userId, innerReq);
                    return Ok(ModelConverter.FormNewSuccessResponse(response));
                }
            }

            #endregion Check Request
        }

        #region Delete Endpoints
        [HttpDelete()]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();

            var resp = await _clientUserProcess.DeleteClientUserById(userId);
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
