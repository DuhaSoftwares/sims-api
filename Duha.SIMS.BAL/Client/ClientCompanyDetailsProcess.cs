using AutoMapper;
using Duha.SIMS.BAL.AppUser;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.BAL.Interface;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.Client;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Client
{
    public partial class ClientCompanyDetailsProcess : LoginUserProcess<ClientCompanyDetailSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;

        #endregion Properties

        #region Constructor
        public ClientCompanyDetailsProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext, IPasswordEncryptHelper passwordEncryptHelper)
            : base(mapper, apiDbContext)
        {
            _apiDbContext = apiDbContext;
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
        }

        #endregion Constructor

        #region Odata
        public override async Task<IQueryable<ClientCompanyDetailSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.ClientCompany;
            IQueryable<ClientCompanyDetailSM> retSM = await MapEntityAsToQuerable<ClientCompanyDetailDM, ClientCompanyDetailSM>(_mapper, entitySet);
            return retSM;
        }

        #endregion Odata


        #region Get
        /// <summary>
        /// Fetches All Client Company Detail
        /// </summary>
        /// <returns>
        /// If Successful, returns List of ClientCompanyDetailSM
        /// </returns>
        public async Task<List<ClientCompanyDetailSM>> GetAllCompanyDetails()
        {
            var dm = await _apiDbContext.ClientCompany.AsNoTracking().ToListAsync();
            var sm = _mapper.Map<List<ClientCompanyDetailSM>>(dm);
            return sm;
        }

        /// <summary>
        /// Fetches  Client CompanyDetail By Id
        /// </summary>
        /// <returns>
        /// If Successful, returns ClientCompanyDetailSM with Base64 image , Otherwise returns null
        /// </returns>

        public async Task<ClientCompanyDetailSM> GetCompanyById(int id)
        {
            var dm = await _apiDbContext.ClientCompany.FindAsync(id);
            string profilePicture = null;
            if (dm != null)
            {
                var sm = _mapper.Map<ClientCompanyDetailSM>(dm);
                if (!string.IsNullOrEmpty(sm.CompanyLogoPath))
                {
                    profilePicture = await ConvertToBase64(sm.CompanyLogoPath);
                    sm.CompanyLogoPath = profilePicture;
                }
                return sm;
            }
            else
            {
                return null;
            }
        }

        #endregion Get

        #region Update Client User (Mine)

        /// <summary>
        /// Updates ClientCompany Detail using Id and ClientCompanyDetailSM object
        /// </summary>
        /// <param name="Id">Id of ClientCompany to Update</param>
        /// <param name="objSM">Object of type ClientCompanyDetailSM to Update</param>
        /// <returns>
        /// If Successful, returns Updated ClientCompanyDetailSM Otherwise return null
        /// </returns>
        public async Task<ClientCompanyDetailSM> UpdateClientUserDetails(int id, ClientCompanyDetailSM objSM)
        {
            if (id == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Please Provide Value to Id");
            }

            if (objSM == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Nothing to Update");

            }
            var imageFullPath = "";
            var objDM = await _apiDbContext.ClientCompany
            .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            if (objDM != null)
            {
                objSM.Id = objDM.Id;
                objSM.ClientCompanyAddressId = (int)objDM.ClientCompanyAddressId;
                objSM.CompanyCode = objDM.CompanyCode;

                if (!string.IsNullOrEmpty(objSM.CompanyLogoPath))
                {
                    imageFullPath = Path.GetFullPath(objDM.CompanyLogoPath);
                    var IsCompanyLogoUpdated = await UpdateCompanyLogo(objDM.CompanyCode, objSM.CompanyLogoPath);
                    if (IsCompanyLogoUpdated != null)
                    {
                        objSM.CompanyLogoPath = null;
                    }
                }
                else
                {
                    objSM.CompanyLogoPath = objDM.CompanyLogoPath;
                }

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
                    var updatedClientUser = await GetCompanyById(id);
                    return updatedClientUser;
                }
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Something went wrong while Updating Company Details");

            }
            else
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Data to update not found, add as new instead.");

            }
        }

        #endregion Update Client User (Mine)

        #region Update Company Logo
        /// <summary>
        /// Updates Company Logo
        /// </summary>
        /// <param name="companyCode"> </param>
        /// <param name="base64String">Base64 string which represents the profile picture of a Client User</param>
        /// <returns>
        /// Returns message (string) response
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        private async Task<string> UpdateCompanyLogo(string companyCode, string base64String)
        {
            var imageFullPath = "";
            if (!string.IsNullOrEmpty(companyCode))
            {

                var objDM = await _apiDbContext.ClientCompany.FirstOrDefaultAsync(s => s.CompanyCode == companyCode);

                if (objDM == null)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Client Company Not Found");
                }

                if (!string.IsNullOrEmpty(objDM.CompanyLogoPath))
                {
                    imageFullPath = Path.GetFullPath(objDM.CompanyLogoPath);
                }

                if (base64String == null)
                {
                    // If base64String is null, update CompanyLogoPath to null
                    objDM.CompanyLogoPath = null;
                }
                else
                {
                    // Convert base64String to image and store it inside a folder
                    // Return the relative path of the image
                    var imageRelativePath = await SaveFromBase64(base64String);

                    if (imageRelativePath != null)
                    {
                        objDM.CompanyLogoPath = imageRelativePath;
                    }
                }
                objDM.LastModifiedBy = _loginUserDetail?.LoginId;
                objDM.LastModifiedOnUTC = DateTime.UtcNow;
            }

            try
            {
                // Save changes to the database
                await _apiDbContext.SaveChangesAsync();

                // Todo: Delete the previous image from the folder
                /* if (File.Exists(imageFullPath))
                     File.Delete(imageFullPath);
 */
                return "Company Logo Updated";
            }
            catch (SIMSException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Cannot Update Company Logo");
            }

        }


        #endregion Update Profile Picture

        #region Create New  Company
        /// <summary>
        /// Method Used for SignUp New Cliet User
        /// </summary>
        /// <param name="signUpSM">SignUp Object to register a new Client User</param>
        /// <returns>If successful, returns ClientUserSM Otherwise returns null</returns>
        /// <exception cref="SIMSException"></exception>

        public async Task<ClientCompanyDetailSM?> CreateNewCompany(int userId, ClientCompanyDetailSM objSM)
        {
            if (objSM == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Please provide details for Creating new Company");
            }

            var objDM = _mapper.Map<ClientCompanyDetailDM>(objSM);

            // Validate EmailId and LoginId
            var existingWithCompanyCode = await _apiDbContext.ClientCompany.FirstOrDefaultAsync(x => x.CompanyCode == objSM.CompanyCode);
            if (existingWithCompanyCode != null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Company with CompanyCode {objSM.CompanyCode} Already Exist...Try Another Company Code");
            }

            var existingWithCompanyEmail = await _apiDbContext.ClientCompany.FirstOrDefaultAsync(x => x.ContactEmail == objSM.ContactEmail);
            if (existingWithCompanyCode != null || string.IsNullOrEmpty(objSM.ContactEmail))
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Company with Provided Email Already Exist...Try Another Company Email");
            }

            // Set ClientCompanyDetailId to null if not provided or if it doesn't exist in the database
            if (objSM.ClientCompanyAddressId == null || objSM.ClientCompanyAddressId == default)
            {
                objDM.ClientCompanyAddressId = null;
            }
            objDM.CreatedBy = _loginUserDetail.LoginId;
            objDM.CreatedOnUTC = DateTime.UtcNow;

            // Add client user to the database
            await _apiDbContext.ClientCompany.AddAsync(objDM);

            // If save changes is successful, return the saved client user
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var createdUser = await GetCompanyById(objDM.Id);
                return createdUser;
            }
            return null;
        }


        #endregion 

        #region Delete

        /// <summary>
        /// Deletes Company by Id from the database 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<DeleteResponseRoot> DeleteClientUserById(int id)
        {
            var clientToDelete = await _apiDbContext.ClientCompany 
                .FirstOrDefaultAsync(f => f.Id == id);
            // Remove the client user
            _apiDbContext.ClientCompany.Remove(clientToDelete);

            // Save changes
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Company Deleted Successfully...");
            }

            return new DeleteResponseRoot(false, "Company Not found");
        }




        #endregion Delete

        #region Private Functions
        /// <summary>
        /// Saves uploaded image (base64)
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns>
        /// returns the relative path of the saved image
        /// </returns>
        static async Task<string?> SaveFromBase64(string base64String)
        {
            string? filePath = null;
            string? imageExtension = "jpg";
            try
            {
                //convert bas64string to bytes
                byte[] imageBytes = Convert.FromBase64String(base64String);

                // Check if the file size exceeds 1MB (2 * 1024 * 1024 bytes)
                if (imageBytes.Length > 2 * 1024 * 1024) //change 1 to desired size 2,3,4 etc
                {
                    throw new Exception("File size exceeds 2 Mb limit.");
                }

                string fileName = Guid.NewGuid().ToString() + "." + imageExtension;

                // Specify the folder path where resumes are stored
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\companies\logos");

                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Combine the folder path and file name to get the full file path
                filePath = Path.Combine(folderPath, fileName);

                // Write the bytes to the file asynchronously
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Return the relative file path
                return Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
            }
            catch
            {
                // If an error occurs, delete the file (if created) and return null
                if (File.Exists(filePath))
                    File.Delete(filePath);
                throw;
            }
        }


        /// <summary>
        /// Converts an image file to a base64 encoded string.
        /// </summary>
        /// <param name="filePath">The path to the image file.</param>
        /// <returns>
        /// If successful, returns the base64 encoded string; 
        /// otherwise, returns null.
        /// </returns>
        static async Task<string?> ConvertToBase64(string filePath)
        {
            try
            {
                // Read all bytes from the file asynchronously
                byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

                // Convert the bytes to a base64 string
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return null
                //return ex.Message;
                return null;
            }
        }

        #endregion Private Functions
    }
}
