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
    public class VariantProcess : SIMSBalOdataBase<VariantSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        #endregion Properties

        #region Constructor
        public VariantProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }
        #endregion Constructor

        #region Odata
        public override async Task<IQueryable<VariantSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Variants;
            var query = entitySet.Select(entity => new VariantSM
            {
                Name = entity.Name,
                VariantLevel = (VariantLevelSM)entity.VariantLevel,
                VariantId = (int)entity.VariantId,
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
        /// Fetches all the Variant with its associated Variants from the database
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of VariantSM otherwise return null
        /// </returns>
        public async Task<List<VariantsSM>> GetAllVariants(int skip, int top)
        {
            // Fetch all level 1 Variants (Variants with LevelId == null or 0)
            var level1Variants = await _apiDbContext.Variants
                .Where(c => c.VariantLevel == VariantLevelDM.Level1 && (c.VariantId == null || c.VariantId == 0))
                .OrderByDescending(c => c.CreatedOnUTC)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var variantsList = new List<VariantsSM>();

            // Iterate over each level 1 Variant
            foreach (var level1Variant in level1Variants)
            {
                // Fetch level 2 Variants associated with the current level 1 Variant
                var level2Variants = await _apiDbContext.Variants
                    .Where(c => c.VariantId == level1Variant.Id && c.VariantLevel == VariantLevelDM.Level2)
                    .OrderByDescending(c => c.CreatedOnUTC)
                    .ToListAsync();

                // Map to VariantSM for Level 1 and Level 2
                var VariantSM = new VariantsSM
                {
                    // Map level 1 Variant
                    Level1Variant = _mapper.Map<VariantSM>(level1Variant),

                    // Map level 2 Variants
                    Level2Variants = _mapper.Map<List<VariantSM>>(level2Variants)
                };

                variantsList.Add(VariantSM);
            }

            return variantsList;
        }


        public async Task<List<VariantSM>> GetByLevel(VariantLevelSM level)
        {
            var list = await _apiDbContext.Variants.Where(x=>x.VariantLevel == (VariantLevelDM)level)
                .OrderByDescending(c => c.CreatedOnUTC)
                .ToListAsync();
            if(list.Count == 0)
            {
                return null;
            }
            return _mapper.Map<List<VariantSM>>(list);
        }

        public async Task<int> GetAllVariantsCount()
        {
            var count = _apiDbContext.Variants.AsNoTracking().Where(x=>x.VariantLevel == VariantLevelDM.Level1).Count();
            return count;
        }

        #endregion Get All

        #region Get By Id (Level 1)
        /// <summary>
        /// Get Variant By Id with associated levels
        /// </summary>
        /// <param name="Id">Using Id of Variant Fetches the respective Variant</param>
        /// <returns> 
        /// If Successful, returns VariantSM otherwise returns Null.
        /// </returns>
        public async Task<VariantsSM> GetVariantByIdAsync(int id)
        {
            // Fetch the Variant by Id
            var level1Variant = await _apiDbContext.Variants
                .FirstOrDefaultAsync(c => c.Id == id);

            if (level1Variant == null || (level1Variant.VariantId != null && level1Variant.VariantId != 0))
            {
                // If the Variant is not found or is not a Level 1 Variant, return null or throw an error
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Variant not found or not a Level 1 Variant.");
            }

            // Fetch associated Level 2 Variants
            var level2Variants = await _apiDbContext.Variants
                .Where(c => c.VariantId == level1Variant.Id)
                .OrderByDescending(c => c.CreatedOnUTC)
                .ToListAsync();

            // Map the Level 1 Variant along with its associated levels (Level 2 and Level 3)
            var VariantModel = new VariantsSM
            {
                Level1Variant = _mapper.Map<VariantSM>(level1Variant),
                Level2Variants = _mapper.Map<List<VariantSM>>(level2Variants)
            };

            return VariantModel;
        }


        #endregion Get By Id (Level 1)

        #region Get Product variants
        /// <summary>
        /// Fetches variants of a product
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns></returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<List<VariantsSM>> GetProductVariantsByProductId(int productId)
        {
            // Fetch all product variants for the specified product ID
            var productVariants = await _apiDbContext.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .Include(pv => pv.VariantLevel1) // Include Level 1 Variant Details
                .Include(pv => pv.VariantLevel2) // Include Level 2 Variant Details
                .ToListAsync();

            // Group the variants by the Level 1 variant (VariantLevel1Id)
            var groupedVariants = productVariants.GroupBy(pv => pv.VariantLevel1Id).ToList();

            // Create a list to store the final response
            var response = new List<VariantsSM>();

            // Iterate through each group and map the results to VariantsSM
            foreach (var group in groupedVariants)
            {
                // Get the Level 1 variant details
                var level1Variant = group.First().VariantLevel1;

                // Get all Level 2 variants under the same Level 1 variant
                var level2Variants = group
                    .Select(pv => pv.VariantLevel2)
                    .Where(l2 => l2 != null) // Avoid null values
                    .Select(l2 => new VariantSM
                    {
                        Id = l2.Id,
                        Name = l2.Name,
                        CreatedBy = l2.CreatedBy,
                        CreatedOnUTC = l2.CreatedOnUTC,
                        LastModifiedBy = l2.LastModifiedBy,
                        LastModifiedOnUTC = l2.LastModifiedOnUTC,
                    }).ToList();

                // Create the VariantsSM object and add it to the response
                var variantsSM = new VariantsSM
                {
                    Level1Variant = new VariantSM
                    {
                        Id = level1Variant.Id,
                        Name = level1Variant.Name,
                        CreatedBy = level1Variant.CreatedBy,
                        CreatedOnUTC = level1Variant.CreatedOnUTC,
                        LastModifiedBy = level1Variant.LastModifiedBy,
                        LastModifiedOnUTC = level1Variant.LastModifiedOnUTC,
                    },
                    Level2Variants = level2Variants
                };

                response.Add(variantsSM);
            }

            return response;
        }


        #endregion Get Product variants

        #region Get Category Variants
        /// <summary>
        /// Get variants using categoryId
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<List<VariantsSM>> GetCategoryVariantsByCategoryId(int categoryId)
        {
            var level1CatId = 0;
            var category = await _apiDbContext.ProductCategories.Where(x => x.Id == categoryId && x.Level == CategoryLevelDM.Level1).FirstOrDefaultAsync();
            if(category != null)
            {
                level1CatId = category.Id;
                
            }
            var level2Category = await _apiDbContext.ProductCategories.Where(x => x.Id == categoryId && x.Level == CategoryLevelDM.Level2).FirstOrDefaultAsync();
            if (level2Category != null)
            {
                level1CatId = (int)level2Category.LevelId;
            }
            if(level1CatId == 0)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Category Not Found");
            }

            // Fetch all product variants for the specified product ID
            var productVariantIds = await _apiDbContext.CategoryVariants
                .Where(pv => pv.ProductCategoryId == level1CatId)
                .Select(pv => pv.VariantId)
                .Distinct()
                .ToListAsync();
            // Create a list to store the final response
            var response = new List<VariantsSM>();

            // Iterate through each group and map the results to VariantsSM
            foreach (var id in productVariantIds)
            {
                // Get the Level 1 variant details
                var level1Variant = _mapper.Map<VariantSM>(await _apiDbContext.Variants.FindAsync(id));

                // Get all Level 2 variants under the same Level 1 variant
                var level2Variants = _mapper.Map<List<VariantSM>>(await _apiDbContext.Variants.Where(x=>x.VariantId == id).ToListAsync());

                // Create the VariantsSM object and add it to the response
                var variantsSM = new VariantsSM
                {
                    Level1Variant = level1Variant,
                    
                    Level2Variants = level2Variants
                };

                response.Add(variantsSM);
            }

            return response;
        }

        #endregion Get Category Variants

        #region Get by Id
        /// <summary>
        /// Get Variant By Id 
        /// </summary>
        /// <param name="Id">Using Id of Variant Fetches the respective Variant</param>
        /// <returns> 
        /// If Successful, returns VariantSM otherwise returns Null.
        /// </returns>
        public async Task<VariantSM> GetById(int id)
        {
            var Variant = await _apiDbContext.Variants.FindAsync(id);
            if(Variant == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Variant Not Found");
            }
            return _mapper.Map<VariantSM>(Variant);
        }

        #endregion Get by Id

        #region Add
        /// <summary>
        /// Adds a new Variant to the database.
        /// </summary>
        /// <param name="VariantSM">The Variant object representing the new Variant.</param>
        /// <returns>
        /// If successful, returns the added Variant; otherwise, returns null.
        /// </returns>
        public async Task<VariantSM> AddVariantAsync(VariantSM newVariant, int categoryId)
        {
            // Begin transaction
            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            // Check the level and LevelId to ensure validity
            if (newVariant.VariantLevel == VariantLevelSM.Level1)
            {
                newVariant.VariantId = null;
            }
            else if (newVariant.VariantLevel == VariantLevelSM.Level2)
            {
                // Level 2 Variant must have a valid LevelId referencing a Level 1 Variant
                var parentLevel1Variant = await _apiDbContext.Variants
                    .OrderByDescending(c => c.CreatedOnUTC)
                    .FirstOrDefaultAsync(c => c.Id == newVariant.VariantId && c.VariantLevel == VariantLevelDM.Level1);

                if (parentLevel1Variant == null)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Invalid LevelId for Level 2 Variant. No matching Level 1 Variant found.");
                }
            }
            else
            {
                // Invalid level exception
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Invalid Variant Level. Allowed values: 1 or 2.");
            }

            // Map the service model to the entity model
            var variantEntity = _mapper.Map<VariantDM>(newVariant);
            variantEntity.CreatedBy = _loginUserDetail.LoginId;
            variantEntity.CreatedOnUTC = DateTime.UtcNow;

            // Add the new variant to the database
            await _apiDbContext.Variants.AddAsync(variantEntity);
            await _apiDbContext.SaveChangesAsync(); // Save variant before proceeding to Category association

            // Check if the category exists and is Level 1
            

            // If the variant is Level 1, associate it with the category
            if (variantEntity.VariantLevel == VariantLevelDM.Level1)
            {
                var categoryExist = await _apiDbContext.ProductCategories.AnyAsync(x => x.Id == categoryId && x.Level == CategoryLevelDM.Level1);
                if (!categoryExist)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog,
                        "The specified category either does not exist or is not a Level 1 category. Please check the category details and try again.");
                }
                var categoryVariant = new CategoryVariantDM()
                {
                    VariantId = variantEntity.Id,
                    ProductCategoryId = categoryId,
                    CreatedBy = _loginUserDetail.LoginId,
                    CreatedOnUTC = DateTime.UtcNow,
                };

                _apiDbContext.CategoryVariants.Add(categoryVariant);
                await _apiDbContext.SaveChangesAsync();
            }

            // Commit the transaction
            await transaction.CommitAsync();

            // Retrieve and return the newly added variant details
            var response = await GetById(variantEntity.Id);
            return response;
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
        public async Task<VariantSM?> UpdateVariant(int objIdToUpdate, VariantSM objSM)
        {
            if (objSM != null && objIdToUpdate > 0)
            {
                //retrieves target user invoice from db
                var objDM = await _apiDbContext.Variants.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    objSM.Id = objIdToUpdate;
                    objSM.VariantId = (int)objDM.VariantId;
                    objSM.VariantLevel = (VariantLevelSM)objDM.VariantLevel;
                    _mapper.Map(objSM, objDM);

                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;


                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        var sm = await GetById(objIdToUpdate);
                        return sm;
                    }
                    throw new SIMSException(ApiErrorTypeSM.Fatal_Log, $"Something went wrong while Updating Variant Details");
                }
                else
                {
                    throw new SIMSException(ApiErrorTypeSM.Fatal_Log, $"Variant not found: {objIdToUpdate}");
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
        public async Task<DeleteResponseRoot> DeleteVariantById(int id)
        {
            // Fetch the Variant by Id
            var VariantToDelete = await _apiDbContext.Variants
                .FirstOrDefaultAsync(c => c.Id == id);

            if (VariantToDelete == null)
            {
                return new DeleteResponseRoot(false, "Variant not found");
            }

            // If it's a Level 1 Variant, delete associated Level 2 Variants
            if (VariantToDelete.VariantLevel == VariantLevelDM.Level1)
            {
                // Fetch associated Level 2 Variants
                var level2Variants = await _apiDbContext.Variants
                    .Where(c => c.VariantId == VariantToDelete.Id)
                    .ToListAsync();

                // Remove Level 2 Variants
                _apiDbContext.Variants.RemoveRange(level2Variants);

                // Remove Level 1 Variant
                _apiDbContext.Variants.Remove(VariantToDelete);
            }
            // If it's a Level 2 Variant, delete only that Variant
            else if (VariantToDelete.VariantLevel == VariantLevelDM.Level2)
            {
                // Remove Level 2 Variant
                _apiDbContext.Variants.Remove(VariantToDelete);
            }
            else
            {
                return new DeleteResponseRoot(false, "Invalid Variant level.");
            }

            // Save changes to the database
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Variant and its associated Variants deleted successfully!");
        }


        #endregion Delete
    }
}
