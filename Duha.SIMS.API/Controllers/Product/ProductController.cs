using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.BAL.Product;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.Product;
using System.Web.Http.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace Duha.SIMS.API.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ApiControllerWithOdataRoot<ProductSM>
    {
        #region Properties
        private readonly ProductProcess _productProcess;
        #endregion Properties

        #region Constructor
        public ProductController(ProductProcess ProductProcess)
            : base(ProductProcess)
        {
            _productProcess = ProductProcess;
        }
        #endregion Constructor

        #region Odata
        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSM>>>> GetAsOdata(ODataQueryOptions<ProductSM> oDataOptions)
        {
            //TODO: validate inputs here probably 
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion Odata

        #region Get All
        [HttpGet()]
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetAll([FromQuery] int skip, [FromQuery] int top)
        {
            var listSM = await _productProcess.GetAllProducts(skip,top);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }


        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCount()
        {
            var countRes = await _productProcess.GetAllProductsCount();

            // Check if the list is empty and return a meaningful response
            

            return Ok(ModelConverter.FormNewSuccessResponse(new IntResponseRoot(countRes,"Total Products ")));
        }

        #endregion Get All

        #region Get Single

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductSM>>> GetById(int id)
        {
            var singleSM = await _productProcess.GetProductsById(id);
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
        public async Task<ActionResult<ApiResponse<ProductSM>>> Post([FromBody] ApiRequest<ProductSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _productProcess.AddProduct(innerReq);
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
        public async Task<ActionResult<ApiResponse<ProductSM>>> Put(int id, [FromBody] ApiRequest<ProductSM> apiRequest)
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

            var resp = await _productProcess.UpdateProducts(id, innerReq);
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
            var resp = await _productProcess.DeleteProductsById(id);
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
    }
}
