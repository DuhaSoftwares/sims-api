using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.BAL.Product;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.Product;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.OData.Query;

namespace Duha.SIMS.API.Controllers.Product
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductCategoryController : ApiControllerWithOdataRoot<ProductCategorySM>
    {
        #region Properties
        private readonly ProductCategoryProcess _productCategoryProcess;
        #endregion Properties

        #region Constructor
        public ProductCategoryController(ProductCategoryProcess productCategoryProcess)
            : base(productCategoryProcess)
        {
            _productCategoryProcess = productCategoryProcess;
        }
        #endregion Constructor

        #region Odata
        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductCategorySM>>>> GetAsOdata(ODataQueryOptions<ProductCategorySM> oDataOptions)
        {
            //TODO: validate inputs here probably 
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion Odata

        #region Get All
        [HttpGet()]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoriesSM>>>> GetAllLevel1Categories([FromQuery]int skip, [FromQuery] int top)
        {
            var listSM = await _productCategoryProcess.GetAllProductCategories(skip,top);
            if (listSM != null)
            {
                
                return Ok(ModelConverter.FormNewSuccessResponse(listSM));
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotFound, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        
        #endregion Get All

        #region Get Single

        [HttpGet("extended/{id}")]
        public async Task<ActionResult<ApiResponse<CategoriesSM>>> GetByIdExtended(int id)
        {
            var singleSM = await _productCategoryProcess.GetProductCategoryByIdAsync(id);
            if (singleSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(singleSM);
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_IdNotFound, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("Level")]
        public async Task<ActionResult<ApiResponse<List<ProductCategorySM>>>> GetByLevel([FromQuery] CategoryLevelSM level)
        {
            var listSM = await _productCategoryProcess.GetByLevel(level);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductCategorySM>>> GetById(int id)
        {
            var singleSM = await _productCategoryProcess.GetById(id);
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

        #region Add Category with level Check
        [HttpPost()]
        public async Task<ActionResult<ApiResponse<ProductCategorySM>>> AddCategory([FromBody] ApiRequest<ProductCategorySM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _productCategoryProcess.AddCategoryAsync(innerReq);
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
        #endregion Add Category with level Check

        #region Update Category

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductCategorySM>>> UpdateProductCategory(int id, [FromBody] ApiRequest<ProductCategorySM> apiRequest)
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

            var resp = await _productCategoryProcess.UpdateProductCategory(id, innerReq);
            if (resp != null)
            {
                return Ok(ModelConverter.FormNewSuccessResponse(resp));
            }
            else
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        #endregion Update Category

        #region Delete Category With Updation Its Associations

        #region Delete
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(int id)
        {
            var resp = await _productCategoryProcess.DeleteProductCategoryById(id);
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
        #endregion Delete Category With Updation Its Associations

        #region Count
        

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllLevel1CategoriesCount()
        {
            
            var count = await _productCategoryProcess.GetAllProductCategoriesCount();
            return Ok(ModelConverter.FormNewSuccessResponse(new IntResponseRoot(count, "My Total Level1 categories ")));
        }
        #endregion Count
    }
}
