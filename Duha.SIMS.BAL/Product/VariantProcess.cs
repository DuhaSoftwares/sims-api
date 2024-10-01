﻿using AutoMapper;
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
            // Check the level and LevelId to ensure validity
            if (newVariant.VariantLevel == VariantLevelSM.Level1)
            {
                newVariant.VariantId = null;
                // Level 1 Variant should have LevelId as NULL or 0
                if (newVariant.VariantId != null && newVariant.VariantId != 0)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Level 1 Variants cannot have a LevelId.");
                }
            }
            else if (newVariant.VariantLevel == VariantLevelSM.Level2)
            {
                // Level 2 Variant should have a valid LevelId referencing a Level 1 Variant
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
                // If the level is not 1 or 2, throw an exception
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Invalid Level. Allowed values: 1 or 2.");
            }

            // Map the service model to the entity model
            var variantEntity = _mapper.Map<VariantDM>(newVariant);
            variantEntity.CreatedBy = _loginUserDetail.LoginId;
            variantEntity.CreatedOnUTC = DateTime.UtcNow;

            // Add the new Variant to the database
            await _apiDbContext.Variants.AddAsync(variantEntity);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if(variantEntity.VariantLevel == VariantLevelDM.Level1)
                {
                    var cateoryVariant = new CategoryVariantDM()
                    {
                        VariantId = variantEntity.Id,
                        ProductCategoryId = categoryId,
                        CreatedBy = _loginUserDetail.LoginId,
                        CreatedOnUTC = DateTime.UtcNow,
                    };
                    _apiDbContext.CategoryVariants.Add(cateoryVariant);
                    await _apiDbContext.SaveChangesAsync();
                }
                var response = await GetById(variantEntity.Id);
                return response;
            }

            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while adding the new Variant.");
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
