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
    [Route("api/v1/[controller]")]
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
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetAll([FromQuery] int skip=0, [FromQuery] int top=10)
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

        #region Get Single and List
        [HttpGet("id")]
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetProductDetails(int id)
        {
            var listSM = await _productProcess.GetProductDetailsById(id);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        [HttpGet("supplier/id")]
        public async Task<ActionResult<ApiResponse<List<ProductSM>>>> GetSupplierProductDetails(int id)
        {
            var listSM = await _productProcess.GetProductsBySupplierId(id);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        [HttpGet("productId/productDetailId")]
        public async Task<ActionResult<ApiResponse<ProductSM>>> GetProductDetail(int productId, int productDetailId)
        {
            var listSM = await _productProcess.GetProductDetailsByIdAndProductDetailId(productId, productDetailId);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        #endregion Get Single

        #region Add

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductSM>>> Post([FromBody] ApiRequest<CreateProductSM> apiRequest)
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
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        #endregion Add

        #region Put
        [HttpPut("{id}/{productDetailsId}")]
        public async Task<ActionResult<ApiResponse<ProductSM>>> Put(int id,int productDetailsId, [FromBody] ApiRequest<ProductSM> apiRequest)
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

            var resp = await _productProcess.UpdateProductByIdAndProductDetailId(id, productDetailsId, innerReq);
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
