using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Duha.SIMS.ServiceModels.Product;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Product
{
    public class UnitsProcess : SIMSBalOdataBase<UnitsSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext; 
        #endregion Properties

        #region Constructor
        public UnitsProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _apiDbContext = apiDbContext;
        }
        #endregion Constructor

        #region Odata
        /// <summary>
        /// Gets Service Model Entities For OData
        /// </summary>
        /// <returns>
        /// Return IQueryable UnitsSM
        /// </returns>
        public override async Task<IQueryable<UnitsSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Units;
            var query = entitySet.Select(entity => new UnitsSM
            {
                Id = entity.Id,
                Name = entity.Name,
                Symbol = entity.Symbol,
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
        /// Fetches all the Units from the database
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of UnitsSM otherwise return null
        /// </returns>
        public async Task<List<UnitsSM>> GetAllUnits(int skip, int top)
        {
            var itemsFromDb = await _apiDbContext.Units
                .OrderByDescending(c => c.CreatedOnUTC)
                .Skip(skip).Take(top)
                .ToListAsync();
            var response = new List<UnitsSM>();
            // Return an empty list instead of null if there are no items
            if (itemsFromDb == null || itemsFromDb.Count == 0)
            {
                return null;
            }
            foreach (var item in itemsFromDb) 
            { 
                var sm = await GetUnitsById(item.Id);
                response.Add(sm);
            }
            return response;
        }

        public async Task<int> GetAllUnitsCount()
        {
            var count =  _apiDbContext.Units.AsNoTracking().Count();
            return count;
        }

        #endregion Get All

        #region Get By Id
        /// <summary>
        /// Get Unit By Using UnitId
        /// </summary>
        /// <param name="Id">Using Id of Unit Fetches the respective Unit</param>
        /// <returns> 
        /// If Successful, returns UnitsSM otherwise returns Null.
        /// </returns>
        public async Task<UnitsSM?> GetUnitsById(int Id)
        {
            var singleItemFromDb = await _apiDbContext.Units.FindAsync(Id);

            if (singleItemFromDb == null)
            {
                return null;
            }

            return _mapper.Map<UnitsSM>(singleItemFromDb);
        }

        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new Unit to the database.
        /// </summary>
        /// <param name="UnitsSM">The UnitsSM object representing the new Unit.</param>
        /// <returns>
        /// If successful, returns the added UnitsSM; otherwise, returns null.
        /// </returns>
        public async Task<UnitsSM?> AddUnits(UnitsSM objSM)
        {
            string? UnitImageRelativePath = null;
            if (objSM == null)
                return null;
            var dm = _mapper.Map<UnitsDM>(objSM);
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.CreatedOnUTC = DateTime.UtcNow;            
            await _apiDbContext.Units.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var res = await GetUnitsById(dm.Id);
                return res;
            }
           
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while adding unit");
            
        }
        #endregion Add

        #region Update
        /// <summary>
        /// Updates a Unit in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the Unit to update.</param>
        /// <param name="UnitsSM">The updated UnitsSM object.</param>
        /// <returns>
        /// If successful, returns the updated UnitsSM; otherwise, returns null.
        /// </returns>
        public async Task<UnitsSM?> UpdateUnits(int objIdToUpdate, UnitsSM UnitsSM)
        {
            if (UnitsSM != null && objIdToUpdate > 0)
            {
                //retrieve target product category to update from db
                UnitsDM? objDM = await _apiDbContext.Units.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    UnitsSM.Id = objIdToUpdate;
                    _mapper.Map(UnitsSM, objDM);
                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;

                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        var res = await GetUnitsById(objDM.Id);
                        return res;
                    }
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while updating changes");
                }
                else
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Unit to update not found, add as new instead.");
                }
            }
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Please provide details to add new unit");
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a Unit by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the Unit to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeleteUnitsById(int id)
        {
            // Check if a Unit with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.Units.FindAsync(id);

            if (itemToDelete != null)
            {

                _apiDbContext.Units.Remove(itemToDelete);

                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, $"Unit with Id {id} deleted successfully!");
                }
            }
            // If no Unit with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "Unit not found");
        }
        #endregion Delete
    }
}
