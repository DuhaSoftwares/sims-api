using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.API.Security;
using Duha.SIMS.BAL.Token.Base;
using Duha.SIMS.BAL.Warehouse;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.OData.Query;

namespace Duha.SIMS.API.Controllers.Warehouse
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WarehouseController : ApiControllerWithOdataRoot<WarehouseSM>
    {
        private readonly WarehouseProcess _warehouseProcess;
        public WarehouseController(WarehouseProcess warehouseProcess)
            : base(warehouseProcess)
        {
            _warehouseProcess = warehouseProcess;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        //[Authorize(AuthenticationSchemes = RenoBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseSM>>>> GetAsOdata(ODataQueryOptions<WarehouseSM> oDataOptions)
        {

            var retList = await GetAsEntitiesOdata(oDataOptions);

            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }



        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin,ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<WarehouseSM>>> GetById(int id)
        {
            var singleSM = await _warehouseProcess.GetWarehouseById(id);
            if (singleSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(singleSM);
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotFound, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("my")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseSM>>>> GetAllMyWarehouses([FromQuery]int skip, [FromQuery]int top)
        {
            var companyCode = "";
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            else
            {
                companyCode = User.GetCompanyCodeFromCurrentUserClaims();

                if (string.IsNullOrEmpty(companyCode))
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotInClaims));
                }
            }
            var listSM = await _warehouseProcess.GetAllMyWarehouses(companyCode, skip,top);
            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllMyWarehousesCount()
        {
            var companyCode = "";
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            else
            {
                companyCode = User.GetCompanyCodeFromCurrentUserClaims();

                if (string.IsNullOrEmpty(companyCode))
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotInClaims));
                }
            }
            var count = await _warehouseProcess.GetAllMyWarehouseCount(companyCode);
            return Ok(ModelConverter.FormNewSuccessResponse(new IntResponseRoot(count, "My Total Warehouses ")));
        }

        #region Add Company
        [HttpPost("my")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]

        public async Task<ActionResult<ApiResponse<WarehouseSM>>> AddWarehouseDetails([FromBody] ApiRequest<WarehouseSM> apiRequest)
        {
            #region Check Request
            string companyCode = null;
            int userId = 0;

            if (!User.Identity.IsAuthenticated)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Unauthorized User...Plz check your Credentials"));
            }
            else
            {
                companyCode = User.GetCompanyCodeFromCurrentUserClaims();
                userId = User.GetUserRecordIdFromCurrentUserClaims();

                if (string.IsNullOrEmpty(companyCode))
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

            var addedSM = await _warehouseProcess.CreateWarehouse(companyCode,innerReq);
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
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin")]
        public async Task<ActionResult<ApiResponse<WarehouseSM>>> UpdateCompany(int warehouseId, [FromBody] ApiRequest<WarehouseSM> apiRequest)
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
                    var response = await _warehouseProcess.UpdateWarehouseDetails(warehouseId, innerReq);
                    return Ok(ModelConverter.FormNewSuccessResponse(response));
                }
            }

            #endregion Check Request
        }

        #region Delete Endpoints

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin,CompanyAdmin,SuperAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(int id)
        {
            var resp = await _warehouseProcess.DeleteWarehouseById(id);
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
