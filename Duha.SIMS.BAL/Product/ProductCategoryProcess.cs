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
                Level = (CategoryLevelSM)entity.Level,
                LevelId = (int)entity.LevelId,
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
        /// Fetches all the ProductCategory with its associated categories from the database
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM otherwise return null
        /// </returns>
        public async Task<List<CategoriesSM>> GetAllProductCategories(int skip, int top)
        {
            // Fetch all level 1 categories (categories with LevelId == null or 0)
            var level1Categories = await _apiDbContext.ProductCategories
                .Where(c => c.LevelId == null || c.LevelId == 0)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var categoriesList = new List<CategoriesSM>();

            // Iterate over each level 1 category
            foreach (var level1Category in level1Categories)
            {
                // Fetch level 2 categories associated with the current level 1 category
                var level2Categories = await _apiDbContext.ProductCategories
                    .Where(c => c.LevelId == level1Category.Id)
                    .ToListAsync();

                var associatedLevels = new List<Level2CategoriesSM>();

                // Iterate over each level 2 category to find level 3 categories
                foreach (var level2Category in level2Categories)
                {
                    // Fetch level 3 categories associated with the current level 2 category
                    var level3Categories = await _apiDbContext.ProductCategories
                        .Where(c => c.LevelId == level2Category.Id)
                        .ToListAsync();

                    // Map level 2 category along with its level 3 categories
                    var level2CategoryModel = new Level2CategoriesSM
                    {
                        Level2Category = _mapper.Map<ProductCategorySM>(level2Category),
                        Level3Categories = _mapper.Map<List<ProductCategorySM>>(level3Categories)
                    };

                    associatedLevels.Add(level2CategoryModel);
                }

                // Map level 1 category along with its associated levels (level 2 and level 3)
                var categoryModel = new CategoriesSM
                {
                    Level1Category = _mapper.Map<ProductCategorySM>(level1Category),
                    AssociatedLevels = associatedLevels
                };

                categoriesList.Add(categoryModel);
            }

            return categoriesList;
        }



        public async Task<int> GetAllProductCategoriesCount()
        {
            var count = _apiDbContext.ProductCategories.AsNoTracking().Where(x=>x.Level == CategoryLevelDM.Level1).Count();
            return count;
        }

        #endregion Get All

        #region Get By Id (Level 1)
        /// <summary>
        /// Get ProductCategory By Id with associated levels
        /// </summary>
        /// <param name="Id">Using Id of ProductCategory Fetches the respective ProductCategory</param>
        /// <returns> 
        /// If Successful, returns ProductCategorySM otherwise returns Null.
        /// </returns>
        public async Task<CategoriesSM> GetProductCategoryByIdAsync(int id)
        {
            // Fetch the category by Id
            var level1Category = await _apiDbContext.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (level1Category == null || (level1Category.LevelId != null && level1Category.LevelId != 0))
            {
                // If the category is not found or is not a Level 1 category, return null or throw an error
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Category not found or not a Level 1 category.");
            }

            // Fetch associated Level 2 categories
            var level2Categories = await _apiDbContext.ProductCategories
                .Where(c => c.LevelId == level1Category.Id)
                .ToListAsync();

            var associatedLevels = new List<Level2CategoriesSM>();

            // Fetch Level 3 categories for each Level 2 category
            foreach (var level2Category in level2Categories)
            {
                var level3Categories = await _apiDbContext.ProductCategories
                    .Where(c => c.LevelId == level2Category.Id)
                    .ToListAsync();

                var level2CategoryModel = new Level2CategoriesSM
                {
                    Level2Category = _mapper.Map<ProductCategorySM>(level2Category),
                    Level3Categories = _mapper.Map<List<ProductCategorySM>>(level3Categories)
                };

                associatedLevels.Add(level2CategoryModel);
            }

            // Map the Level 1 category along with its associated levels (Level 2 and Level 3)
            var categoryModel = new CategoriesSM
            {
                Level1Category = _mapper.Map<ProductCategorySM>(level1Category),
                AssociatedLevels = associatedLevels
            };

            return categoryModel;
        }


        #endregion Get By Id (Level 1)

        #region Get by Id
        /// <summary>
        /// Get ProductCategory By Id 
        /// </summary>
        /// <param name="Id">Using Id of ProductCategory Fetches the respective ProductCategory</param>
        /// <returns> 
        /// If Successful, returns ProductCategorySM otherwise returns Null.
        /// </returns>
        public async Task<ProductCategorySM> GetById(int id)
        {
            var category = await _apiDbContext.ProductCategories.FindAsync(id);
            if(category == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Category Not Found");
            }
            return _mapper.Map<ProductCategorySM>(category);
        }

        #endregion Get by Id

        #region Add
        /// <summary>
        /// Adds a new ProductCategory to the database.
        /// </summary>
        /// <param name="ProductCategorySM">The ProductCategory object representing the new ProductCategory.</param>
        /// <returns>
        /// If successful, returns the added ProductCategory; otherwise, returns null.
        /// </returns>
        public async Task<ProductCategorySM> AddCategoryAsync(ProductCategorySM newCategory)
        {
            // Check the level and LevelId to ensure validity
            if (newCategory.Level == CategoryLevelSM.Level1)
            {
                // Level 1 category should have LevelId as NULL or 0
                if (newCategory.LevelId != null && newCategory.LevelId != 0)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Level 1 categories cannot have a LevelId.");
                }
            }
            else if (newCategory.Level == CategoryLevelSM.Level2)
            {
                // Level 2 category should have a valid LevelId referencing a Level 1 category
                var parentLevel1Category = await _apiDbContext.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == newCategory.LevelId && c.Level == CategoryLevelDM.Level1);

                if (parentLevel1Category == null)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog,"Invalid LevelId for Level 2 category. No matching Level 1 category found.");
                }
            }
            else if (newCategory.Level == CategoryLevelSM.Level3)
            {
                // Level 3 category should have a valid LevelId referencing a Level 2 category
                var parentLevel2Category = await _apiDbContext.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == newCategory.LevelId && c.Level == CategoryLevelDM.Level2);

                if (parentLevel2Category == null)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog,"Invalid LevelId for Level 3 category. No matching Level 2 category found.");
                }
            }
            else
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog,"Invalid Level. Allowed values: 1, 2, or 3.");
            }

            // Map the service model to the entity model
            var categoryEntity = _mapper.Map<ProductCategoryDM>(newCategory);

            // Add the new category to the database
            await _apiDbContext.ProductCategories.AddAsync(categoryEntity);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                var response = await GetById(categoryEntity.Id);
                return response;
            }

            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while adding new category");
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
                    objSM.LevelId = (int)objDM.LevelId;
                    objSM.Level = (CategoryLevelSM)objDM.Level;
                    _mapper.Map(objSM, objDM);

                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;


                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        var sm = await GetById(objIdToUpdate);
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
            // Fetch the category by Id
            var categoryToDelete = await _apiDbContext.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoryToDelete == null)
            {
                return new DeleteResponseRoot(false, "ProductCategory not found");
            }

            // If it's a Level 1 category, delete associated Level 2 and Level 3 categories
            if (categoryToDelete.Level == CategoryLevelDM.Level1)
            {
                // Fetch associated Level 2 categories
                var level2Categories = await _apiDbContext.ProductCategories
                    .Where(c => c.LevelId == categoryToDelete.Id)
                    .ToListAsync();

                foreach (var level2Category in level2Categories)
                {
                    // Fetch associated Level 3 categories for each Level 2 category
                    var level3Categories = await _apiDbContext.ProductCategories
                        .Where(c => c.LevelId == level2Category.Id)
                        .ToListAsync();

                    // Remove associated Level 3 categories
                    _apiDbContext.ProductCategories.RemoveRange(level3Categories);

                    // Remove Level 2 category
                    _apiDbContext.ProductCategories.Remove(level2Category);
                }

                // Remove Level 1 category
                _apiDbContext.ProductCategories.Remove(categoryToDelete);
            }
            // If it's a Level 2 category, delete associated Level 3 categories
            else if (categoryToDelete.Level == CategoryLevelDM.Level2)
            {
                // Fetch associated Level 3 categories
                var level3Categories = await _apiDbContext.ProductCategories
                    .Where(c => c.LevelId == categoryToDelete.Id)
                    .ToListAsync();

                // Remove associated Level 3 categories
                _apiDbContext.ProductCategories.RemoveRange(level3Categories);

                // Remove Level 2 category
                _apiDbContext.ProductCategories.Remove(categoryToDelete);
            }
            // If it's a Level 3 category, delete only that category
            else
            {
                // Remove the Level 3 category
                _apiDbContext.ProductCategories.Remove(categoryToDelete);
            }

            // Save changes to the database
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, $"ProductCategory and its associated Categories deleted successfully!");
        }

        #endregion Delete
    }
}
