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
    public class VariantController : ApiControllerWithOdataRoot<VariantSM>
    {
        #region Properties
        private readonly VariantProcess _variantProcess;
        #endregion Properties

        #region Constructor
        public VariantController(VariantProcess variantProcess)
            : base(variantProcess)
        {
            _variantProcess = variantProcess;
        }
        #endregion Constructor

        #region Odata
        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VariantSM>>>> GetAsOdata(ODataQueryOptions<VariantSM> oDataOptions)
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
            var listSM = await _variantProcess.GetAllVariants(skip,top);
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
        public async Task<ActionResult<ApiResponse<VariantsSM>>> GetByIdExtended(int id)
        {
            var singleSM = await _variantProcess.GetVariantByIdAsync(id);
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
        public async Task<ActionResult<ApiResponse<List<VariantSM>>>> GetByLevel([FromQuery] VariantLevelSM level)
        {
            var listSM = await _variantProcess.GetByLevel(level);

            return Ok(ModelConverter.FormNewSuccessResponse(listSM));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<VariantSM>>> GetById(int id)
        {
            var singleSM = await _variantProcess.GetById(id);
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
        public async Task<ActionResult<ApiResponse<VariantSM>>> AddCategory([FromBody] ApiRequest<VariantSM> apiRequest, [FromQuery] int categoryId)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request

            var addedSM = await _variantProcess.AddVariantAsync(innerReq, categoryId);
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
        public async Task<ActionResult<ApiResponse<VariantSM>>> UpdateProductCategory(int id, [FromBody] ApiRequest<VariantSM> apiRequest)
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

            var resp = await _variantProcess.UpdateVariant(id, innerReq);
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
            var resp = await _variantProcess.DeleteVariantById(id);
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
            
            var count = await _variantProcess.GetAllVariantsCount();
            return Ok(ModelConverter.FormNewSuccessResponse(new IntResponseRoot(count, "My Total Level1 Variants  ")));
        }
        #endregion Count
    }
}
