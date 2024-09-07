using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Warehouse;
using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Duha.SIMS.ServiceModels.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Warehouse
{
    public partial class WarehouseProcess : SIMSBalOdataBase<WarehouseSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext;
        private readonly IMapper _mapper;

        #endregion Properties

        #region Constructor
        public WarehouseProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
            : base(mapper,apiDbContext)
        {
            _apiDbContext = apiDbContext;
            _loginUserDetail = loginUserDetail;
            _mapper = mapper;
        }

        #endregion Constructor

        #region Odata
        public override async Task<IQueryable<WarehouseSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Warehouses;
            IQueryable<WarehouseSM> retSM = await MapEntityAsToQuerable<WarehouseDM, WarehouseSM>(_mapper, entitySet);
            return retSM;
        }

        #endregion Odata


        #region Get
        /// <summary>
        /// Fetches All Warehouses
        /// </summary>
        /// <returns>
        /// If Successful, returns List of WarehouseSM
        /// </returns>
        public async Task<List<WarehouseSM>> GetAllMyWarehouses(string companyCode, int skip, int top)
        {
            var company = await _apiDbContext.ClientCompany.Where(x=>x.CompanyCode == companyCode).FirstOrDefaultAsync();
            if (company == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Company Details Not Found...Add Company First");
            }
            var dm = await _apiDbContext.Warehouses.AsNoTracking().Where(x=>x.ClientCompanyDetailId == company.Id)
                .Skip(skip).Take(top)
                .ToListAsync();
            var sm = _mapper.Map<List<WarehouseSM>>(dm);
            return sm;
        }


        public async Task<int> GetAllMyWarehouseCount(string companyCode)
        {
            var company = await _apiDbContext.ClientCompany.Where(x => x.CompanyCode == companyCode).FirstOrDefaultAsync();
            if (company == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Company Details Not Found...Add Company First");
            }
            var dm = await _apiDbContext.Warehouses.AsNoTracking().Where(x => x.ClientCompanyDetailId == company.Id)           
                .ToListAsync();
            var count = dm.Count();
            return count;
        }

        /// <summary>
        /// Fetches Warehouse By Id
        /// </summary>
        /// <returns>
        /// If Successful, returns WarehouseSM with Base64 image , Otherwise returns null
        /// </returns>

        public async Task<WarehouseSM> GetWarehouseById(int id)
        {
            var warehouse = await _apiDbContext.Warehouses.FindAsync(id);
            string profilePicture = null;
            if (warehouse != null)
            {
                var sm = _mapper.Map<WarehouseSM>(warehouse);
                
                return sm;
            }
            else
            {
                return null;
            }
        }

        #endregion Get

        #region Update Warehouse (Mine)

        /// <summary>
        /// Updates Warehouse Details using Id and WarehouseSM object
        /// </summary>
        /// <param name="Id">Id of Warehouse to Update</param>
        /// <param name="objSM">Object of type ClienetUserSM to Update</param>
        /// <returns>
        /// If Successful, returns Updated WarehouseSM Otherwise return null
        /// </returns>
        public async Task<WarehouseSM> UpdateWarehouseDetails(int id, WarehouseSM objSM)
        {
            if (id == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Please Provide Value to Id");
            }

            if (objSM == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Nothing to Update");

            }

            var objDM = await _apiDbContext.Warehouses
            .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            
            string imageFullPath = null;
            if (objDM != null)
            {
                objSM.Id = objDM.Id;
                objSM.ClientCompanyDetailId = (int)objDM.ClientCompanyDetailId;

               
                objSM.StorageType = (StorageTypeSM)objDM.StorageType;

                var smProperties = objSM.GetType().GetProperties();
                var dmProperties = objDM.GetType().GetProperties();

                foreach (var smProperty in smProperties)
                {
                    var smValue = smProperty.GetValue(objSM, null);

                    // Find the corresponding property in objDM with the same name
                    var dmProperty = dmProperties.FirstOrDefault(p => p.Name == smProperty.Name);

                    if (dmProperty != null)
                    {
                        var dmValue = dmProperty.GetValue(objDM, null);

                        // Check if the value in objSM is null, and update it with the corresponding value from objDM
                        if (smValue == null && dmValue != null)
                        {
                            smProperty.SetValue(objSM, dmValue, null);
                        }
                    }
                }
                _mapper.Map(objSM, objDM);
                objDM.LastModifiedBy = _loginUserDetail.LoginId;
                objDM.LastModifiedOnUTC = DateTime.UtcNow;
                /*await _apiDbContext.SaveChangesAsync();
                return _mapper.Map<UserSM>(objDM);*/

                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    var updatedWarehouse = await GetWarehouseById(id);
                    return updatedWarehouse;
                }
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Something went wrong while Updating Warehouse Details");

            }
            else
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Data to update not found, add as new instead.");

            }
        }

        #endregion Update Warehouse (Mine)


        #region Create Warehouse
        /// <summary>
        /// Method Used for SignUp New Cliet User
        /// </summary>
        /// <param name="signUpSM">SignUp Object to register a new Warehouse</param>
        /// <returns>If successful, returns WarehouseSM Otherwise returns null</returns>
        /// <exception cref="SIMSException"></exception>

        public async Task<WarehouseSM?> CreateWarehouse(string companyCode, WarehouseSM signUpSM)
        {
            if (signUpSM == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Please provide details for Addition of Company");
            }
           
            var company = await _apiDbContext.ClientCompany.Where(c=>c.CompanyCode == companyCode).FirstOrDefaultAsync();
            if(company == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Cannot Add Warehouse as Company Details Not Found");
            }
            var objDM = _mapper.Map<WarehouseDM>(signUpSM);

            objDM.ClientCompanyDetailId = company.Id;
            objDM.CreatedBy = _loginUserDetail.LoginId;
            objDM.CreatedOnUTC = DateTime.UtcNow;

            // Add Warehouse to the database
            await _apiDbContext.Warehouses.AddAsync(objDM);

            // If save changes is successful, return the saved Warehouse
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var createdUser = await GetWarehouseById(objDM.Id);
                return createdUser;
            }
            return null;
        }


        #endregion SignUp Warehouse

        #region Delete

        /// <summary>
        /// Deletes ClientUser by Id from the database 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<DeleteResponseRoot> DeleteWarehouseById(int id)
        {
            var clientToDelete = await _apiDbContext.Warehouses // Load related `ClientCompanyDetail`
                .FirstOrDefaultAsync(f => f.Id == id);
            // Remove the Warehouse
            _apiDbContext.Warehouses.Remove(clientToDelete);

            // Save changes
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Warehouse Deleted Successfully...");
            }

            return new DeleteResponseRoot(false, "User Not found");
        }




        #endregion Delete
    }
}

