using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Duha.SIMS.ServiceModels.Product;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Product
{
    public class ProductCategoryProcess : SIMSBalOdataBase<ProductCategorySM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        #endregion Properties

        #region Constructor
        public ProductCategoryProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }
        #endregion Constructor

        #region Odata
        public override async Task<IQueryable<ProductCategorySM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.ProductCategories;
            var query = entitySet.Select(entity => new ProductCategorySM
            {
                Name = entity.Name,
                Description = entity.Description,
                Id = entity.Id,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedOnUTC = entity.CreatedOnUTC,
                LastModifiedOnUTC = entity.LastModifiedOnUTC,
            });

            // Return the projected query as IQueryable
            return await Task.FromResult(query);
        }

        #endregion Odata

        #region Get All
        /// <summary>
        /// Fetches all the ProductCategory from the database
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM otherwise return null
        /// </returns>
        public async Task<List<ProductCategorySM>> GetAllProductCategories(int skip, int top)
        {
            var itemsFromDb = await _apiDbContext.ProductCategories
                .Skip(skip).Take(top)
                .ToListAsync();
            var response = _mapper.Map<List<ProductCategorySM>>(itemsFromDb);
            return response;
        }

        public async Task<int> GetAllProductCategoriesCount()
        {
            var count = _apiDbContext.ProductCategories.AsNoTracking().Count();
            return count;
        }

        #endregion Get All

        #region Get By Id
        /// <summary>
        /// Get ProductCategory By Id
        /// </summary>
        /// <param name="Id">Using Id of ProductCategory Fetches the respective ProductCategory</param>
        /// <returns> 
        /// If Successful, returns ProductCategorySM otherwise returns Null.
        /// </returns>
        public async Task<ProductCategorySM?> GetProductCategoryById(int Id)
        {
            var singleItemFromDb = await _apiDbContext.ProductCategories.FindAsync(Id);

            if (singleItemFromDb == null)
            {
                return null;
            }

            return _mapper.Map<ProductCategorySM>(singleItemFromDb);
        }



        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new ProductCategory to the database.
        /// </summary>
        /// <param name="ProductCategorySM">The ProductCategory object representing the new ProductCategory.</param>
        /// <returns>
        /// If successful, returns the added ProductCategory; otherwise, returns null.
        /// </returns>
        public async Task<ProductCategorySM?> AddProductCategory(ProductCategorySM objSM)
        {
            if (objSM == null)
                return null;
            var dm = _mapper.Map<ProductCategoryDM>(objSM);
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.CreatedOnUTC = DateTime.UtcNow;
            await _apiDbContext.ProductCategories.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var response = await GetProductCategoryById(dm.Id);
                return response;
            }
            
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while saving the changes");

        }
        #endregion Add

        #region Update
        /// <summary>
        /// Updates a CompanyApiConfiguration in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the CompanyApiConfigurationSM to update.</param>
        /// <param name="objSM">The updated CompanyApiConfigurationSM object.</param>
        /// <returns>
        /// If successful, returns the updated CompanyApiConfigurationSM otherwise, returns null.
        /// </returns>
        public async Task<ProductCategorySM?> UpdateProductCategory(int objIdToUpdate, ProductCategorySM objSM)
        {
            if (objSM != null && objIdToUpdate > 0)
            {
                //retrieves target user invoice from db
                var objDM = await _apiDbContext.ProductCategories.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    objSM.Id = objIdToUpdate;
                    _mapper.Map(objSM, objDM);

                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;

                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        var sm = await GetProductCategoryById(objIdToUpdate);
                        return sm;
                    }
                    throw new SIMSException(ApiErrorTypeSM.Fatal_Log, $"Something went wrong while Updating ProductCategory Details");
                }
                else
                {
                    throw new SIMSException(ApiErrorTypeSM.Fatal_Log, $"ProductCategory not found: {objIdToUpdate}");
                }
            }

            return null;
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a brand by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the brand to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeleteProductCategoryById(int id)
        {
            // Check if a brand with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.ProductCategories.FindAsync(id);

            if (itemToDelete != null)
            {
                // Remove the user invoice from the database
                _apiDbContext.ProductCategories.Remove(itemToDelete);
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, $"ProductCategory with Id {id} deleted successfully!");
                }
            }
            // If no brand with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "ProductCategory not found");
        }
        #endregion Delete
    }
}
