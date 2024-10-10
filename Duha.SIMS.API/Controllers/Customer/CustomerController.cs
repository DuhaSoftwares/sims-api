using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.BAL.Customer;
using Duha.SIMS.BAL.Token.Base;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Customer;
using Duha.SIMS.ServiceModels.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.OData.Query;

namespace Duha.SIMS.API.Controllers.Customer
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CustomerController : ApiControllerWithOdataRoot<CustomerSM>
    {
        #region Properties
        private readonly CustomerProcess _customerProcess;
        #endregion Properties

        #region Constructor
        public CustomerController(CustomerProcess customerProcess)
            : base(customerProcess)
        {
            _customerProcess = customerProcess;
        }
        #endregion Constructor

        #region Odata
        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerSM>>>> GetAsOdata(ODataQueryOptions<CustomerSM> oDataOptions)
        {
            //TODO: validate inputs here probably 
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion Odata

        #region Get All
        [HttpGet()]
        //[Authorize(AuthenticationSchemes = DuhaBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "CompanyAdmin,ClientAdmin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse<List<CustomerSM>>>> GetAll([FromQuery] int skip, [FromQuery] int top)
        {
            //var userId = User.GetUserRecordIdFromCurrentUserClaims();
            var listSM = await _customerProcess.GetAllCustomers(skip, top);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCount()
        {
            var countRes = await _customerProcess.GetAllCustomersCount();

            // Check if the list is empty and return a meaningful response


            return Ok(ModelConverter.FormNewSuccessResponse(new IntResponseRoot(countRes, "Total customers ")));
        }

        #endregion Get All

        #region Get Single

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerSM>>> GetById(int id)
        {
            var singleSM = await _customerProcess.GetCustomerById(id);
            if (singleSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(singleSM);
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotFound, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        #endregion Get Single

        #region Add

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerSM>>> Post([FromBody] ApiRequest<CustomerSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _customerProcess.AddCustomer(innerReq);
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
        #endregion Add

        #region Put
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerSM>>> Put(int id, [FromBody] ApiRequest<CustomerSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            if (id <= 0)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdInvalid, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var resp = await _customerProcess.UpdateCustomer(id, innerReq);
            if (resp != null)
            {
                return Ok(ModelConverter.FormNewSuccessResponse(resp));
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        #endregion Put

        #region Delete
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(int id)
        {
            var resp = await _customerProcess.DeleteCustomerById(id);
            if (resp != null && resp.DeleteResult)
            {
                return Ok(ModelConverter.FormNewSuccessResponse(resp));
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(resp?.DeleteMessage, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        #endregion Delete

        #region Transactions

        [HttpPost("transaction")]
        public async Task<ActionResult<ApiResponse<MoneyTransactionHistorySM>>> MoneyTransaction([FromBody] ApiRequest<MoneyTransactionHistorySM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _customerProcess.MoneyTransactionByCustomer(innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        #endregion Transactions

        #region Outstanding Balance

        [HttpGet("outstandingBalance")]
        public async Task<ActionResult<ApiResponse<OutstandingBalanceHistorySM>>> OutstandingBalance(int customerId)
        {
            #region Check Request

            if (customerId < 1)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));

            }

            #endregion Check Request

            var addedSM = await _customerProcess.OutstandingBalance(customerId);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        #endregion Outstanding Balance
    }
}
