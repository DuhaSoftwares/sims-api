using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Customer;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Customer;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Customer
{
    public class CustomerProcess : SIMSBalOdataBase<CustomerSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext;
        #endregion Properties

        #region Constructor
        public CustomerProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
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
        /// Return IQueryable CustomerSM
        /// </returns>
        public override async Task<IQueryable<CustomerSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Customers;
            var query = entitySet.Select(entity => new CustomerSM
            {
                Name = entity.Name,
                Country = entity.Country,
                City = entity.City,
                ZipCode = entity.ZipCode,
                CustomerGroup = (CustomerGroupSM)entity.CustomerGroup,
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
        /// Fetches all the Customers from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of CustomerSM otherwise return null
        /// </returns>
        public async Task<List<CustomerSM>> GetAllCustomers(int skip, int top)
        {
            var itemsFromDb = await _apiDbContext.Customers
                .OrderByDescending(c => c.CreatedOnUTC)
                .Skip(skip).Take(top)
                .ToListAsync();
            if (itemsFromDb.Any())
            {
                return _mapper.Map<List<CustomerSM>>(itemsFromDb);
            }
            return null;
        }

        public async Task<int> GetAllCustomersCount()
        {
            var count = _apiDbContext.Customers.AsNoTracking().Count();
            return count;
        }

        #endregion Get All

        #region Get By Id
        /// <summary>
        /// Get Customer By Using CustomerId
        /// </summary>
        /// <param name="Id">Using Id of Customer Fetches the respective Customer</param>
        /// <returns> 
        /// If Successful, returns CustomerSM otherwise returns Null.
        /// </returns>
        /// <exception cref="ShopWaveException"></exception>
        public async Task<CustomerSM?> GetCustomerById(int Id)
        {
            var singleItemFromDb = await _apiDbContext.Customers.FindAsync(Id);

            if (singleItemFromDb == null)
            {
                return null;
            }

            return _mapper.Map<CustomerSM>(singleItemFromDb);
        }



        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new Customer to the database.
        /// </summary>
        /// <param name="CustomerSM">The CustomerSM object representing the new Customer.</param>
        /// <returns>
        /// If successful, returns the added CustomerSM; otherwise, returns null.
        /// </returns>
        public async Task<CustomerSM?> AddCustomer(CustomerSM objSM)
        {
            string? CustomerImageRelativePath = null;
            if (objSM == null)
                return null;
            var dm = _mapper.Map<CustomerDM>(objSM);
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.CreatedOnUTC = DateTime.UtcNow;
            
            await _apiDbContext.Customers.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<CustomerSM>(dm);
            }
            if (CustomerImageRelativePath != null)
            {
                string fullImagePath = Path.GetFullPath(CustomerImageRelativePath);
                if (File.Exists(fullImagePath))
                    File.Delete(fullImagePath);
            }
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while saving the changes");

        }
        #endregion Add

        #region Update
        /// <summary>
        /// Updates a Customer in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the Customer to update.</param>
        /// <param name="CustomerSM">The updated CustomerSM object.</param>
        /// <returns>
        /// If successful, returns the updated CustomerSM; otherwise, returns null.
        /// </returns>
        public async Task<CustomerSM?> UpdateCustomer(int objIdToUpdate, CustomerSM objSM)
        {
            if (objSM != null && objIdToUpdate > 0)
            {
                //retrieve target product category to update from db
                CustomerDM? objDM = await _apiDbContext.Customers.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    objSM.Id = objIdToUpdate;
                    _mapper.Map(objSM, objDM);
                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;
                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        return _mapper.Map<CustomerSM>(objDM);
                    }
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while Adding customer details");
                }
                else
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Customer to update not found, add as new instead.");
                }
            }
            return null;
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a Customer by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the Customer to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeleteCustomerById(int id)
        {
            // Check if a Customer with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.Customers.FindAsync(id);

            if (itemToDelete != null)
            {

                _apiDbContext.Customers.Remove(itemToDelete);

                // If save changes is successful, return a success response
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, $"Customer with Id {id} deleted successfully!");
                }
            }
            // If no Customer with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "Customer not found");
        }
        #endregion Delete

        #region Private Functions
        #endregion Private Functions
    }
}
