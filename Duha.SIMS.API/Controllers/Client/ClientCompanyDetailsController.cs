using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.API.Security;
using Duha.SIMS.BAL.Client;
using Duha.SIMS.BAL.Token.Base;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Client;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.OData.Query;
namespace Duha.SIMS.API.Controllers.Client
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientCompanyDetailsController : ApiControllerWithOdataRoot<ClientCompanyDetailSM>
    {
        private readonly ClientCompanyDetailsProcess _clientCompanyDetailsProcess;
        public ClientCompanyDetailsController(ClientCompanyDetailsProcess clientCompanyDetailsProcess)
            :base(clientCompanyDetailsProcess)
        { 
            _clientCompanyDetailsProcess = clientCompanyDetailsProcess;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        //[Authorize(AuthenticationSchemes = RenoBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientCompanyDetailSM>>>> GetAsOdata(ODataQueryOptions<ClientCompanyDetailSM> oDataOptions)
        {

            var retList = await GetAsEntitiesOdata(oDataOptions);

            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }



        [HttpGet("{id}")]
        //[Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<ClientCompanyDetailSM>>> GetById(int id)
        {
            var singleSM = await _clientCompanyDetailsProcess.GetCompanyById(id);
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
        //[Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientCompanyDetailSM>>>> GetAll()
        {
            var singleSM = await _clientCompanyDetailsProcess.GetAllCompanyDetails();
            return Ok(ModelConverter.FormNewSuccessResponse(singleSM));
        }

        #region Add Company
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin,SystemAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ClientCompanyDetailSM>>> AddCompanyDetails([FromBody] ApiRequest<ClientCompanyDetailSM> apiRequest)
        {
            #region Check Request
            string userRole = null;
            int userId = 0;

            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            else
            {
                userRole = User.GetUserRoleTypeFromCurrentUserClaims();
                userId = User.GetUserRecordIdFromCurrentUserClaims();

                if (string.IsNullOrEmpty(userRole) || userId == 0)
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotInClaims));
                }
            }

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }

            #endregion Check Request

            var addedSM = await _clientCompanyDetailsProcess.AddUserCompany(innerReq, userId, userRole);
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
        #endregion Add Company


        [HttpPut("my")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ClientCompanyDetailSM>>> UpdateCompany(int id, [FromBody] ApiRequest<ClientCompanyDetailSM> apiRequest)
        {
            #region Check Request
            var innerReq = apiRequest?.ReqData;
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized Farmer...Plz check your Credentials"));
            }
            else
            {
                var userId = User.GetUserRecordIdFromCurrentUserClaims();
                var companyCode = User.GetCompanyCodeFromCurrentUserClaims();
                if (userId <= 0)
                {
                    return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdInvalid, ApiErrorTypeSM.InvalidInputData_NoLog));
                }

                if (innerReq == null)
                {
                    return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
                }

                else
                {
                    var response = await _clientCompanyDetailsProcess.UpdateClientUserDetails(id, innerReq);
                    return Ok(ModelConverter.FormNewSuccessResponse(response));
                }
            }

            #endregion Check Request
        }

        #region Delete Endpoints

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin,SuperAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(int id)
        {
            /*if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }*/
            var userRole = User.GetUserRoleTypeFromCurrentUserClaims();
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            var companyCode = User.GetCompanyCodeFromCurrentUserClaims();

            var resp = await _clientCompanyDetailsProcess.DeleteClientUserById(id);
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
