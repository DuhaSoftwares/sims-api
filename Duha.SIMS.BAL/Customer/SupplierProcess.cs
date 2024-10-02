using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Customer;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Customer;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Customer
{
    public class SupplierProcess : SIMSBalOdataBase<SupplierSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext;
        #endregion Properties

        #region Constructor
        public SupplierProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
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
        /// Return IQueryable SupplierSM
        /// </returns>
        public override async Task<IQueryable<SupplierSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Suppliers;
            var query = entitySet.Select(entity => new SupplierSM
            {
                Name = entity.Name,
                Country = entity.Country,
                City = entity.City,
                ZipCode = entity.ZipCode,
                CompanyName = entity.CompanyName,
                EmailId = entity.EmailId,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
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
        /// Fetches all the Suppliers from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of SupplierSM otherwise return null
        /// </returns>
        public async Task<List<SupplierSM>> GetAllSuppliers(int skip, int top)
        {
            var itemsFromDb = await _apiDbContext.Suppliers
                .OrderByDescending(c => c.CreatedOnUTC)
                .Skip(skip).Take(top)
                .ToListAsync();
            if (itemsFromDb.Any())
            {
                return _mapper.Map<List<SupplierSM>>(itemsFromDb);
            }
            return null;
        }

        public async Task<int> GetAllSuppliersCount()
        {
            var count = _apiDbContext.Suppliers.AsNoTracking().Count();
            return count;
        }

        #endregion Get All

        #region Get By Id
        /// <summary>
        /// Get Supplier By Using SupplierId
        /// </summary>
        /// <param name="Id">Using Id of Supplier Fetches the respective Supplier</param>
        /// <returns> 
        /// If Successful, returns SupplierSM otherwise returns Null.
        /// </returns>
        /// <exception cref="ShopWaveException"></exception>
        public async Task<SupplierSM?> GetSupplierById(int Id)
        {
            var singleItemFromDb = await _apiDbContext.Suppliers.FindAsync(Id);

            if (singleItemFromDb == null)
            {
                return null;
            }

            return _mapper.Map<SupplierSM>(singleItemFromDb);
        }



        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new Supplier to the database.
        /// </summary>
        /// <param name="SupplierSM">The SupplierSM object representing the new Supplier.</param>
        /// <returns>
        /// If successful, returns the added SupplierSM; otherwise, returns null.
        /// </returns>
        public async Task<SupplierSM?> AddSupplier(SupplierSM objSM)
        {
            string? SupplierImageRelativePath = null;
            if (objSM == null)
                return null;
            var dm = _mapper.Map<SupplierDM>(objSM);
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.CreatedOnUTC = DateTime.UtcNow;
            
            await _apiDbContext.Suppliers.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<SupplierSM>(dm);
            }
            if (SupplierImageRelativePath != null)
            {
                string fullImagePath = Path.GetFullPath(SupplierImageRelativePath);
                if (File.Exists(fullImagePath))
                    File.Delete(fullImagePath);
            }
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while saving the changes");

        }
        #endregion Add

        #region Update
        /// <summary>
        /// Updates a Supplier in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the Supplier to update.</param>
        /// <param name="SupplierSM">The updated SupplierSM object.</param>
        /// <returns>
        /// If successful, returns the updated SupplierSM; otherwise, returns null.
        /// </returns>
        public async Task<SupplierSM?> UpdateSupplier(int objIdToUpdate, SupplierSM objSM)
        {
            if (objSM != null && objIdToUpdate > 0)
            {
                //retrieve target product category to update from db
                SupplierDM? objDM = await _apiDbContext.Suppliers.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    objSM.Id = objIdToUpdate;
                    _mapper.Map(objSM, objDM);
                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;
                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        return _mapper.Map<SupplierSM>(objDM);
                    }
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while Adding Supplier details");
                }
                else
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Supplier to update not found, add as new instead.");
                }
            }
            return null;
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a Supplier by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the Supplier to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeleteSupplierById(int id)
        {
            // Check if a Supplier with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.Suppliers.FindAsync(id);

            if (itemToDelete != null)
            {

                _apiDbContext.Suppliers.Remove(itemToDelete);

                // If save changes is successful, return a success response
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, $"Supplier with Id {id} deleted successfully!");
                }
            }
            // If no Supplier with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "Supplier not found");
        }
        #endregion Delete

        #region Private Functions
        #endregion Private Functions
    }
}
