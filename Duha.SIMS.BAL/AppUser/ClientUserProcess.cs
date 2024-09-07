using AutoMapper;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.BAL.Interface;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.AppUser
{
    public partial class ClientUserProcess : LoginUserProcess<ClientUserSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;

        #endregion Properties

        #region Constructor
        public ClientUserProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext, IPasswordEncryptHelper passwordEncryptHelper)
            : base(mapper, apiDbContext)
        {
            _apiDbContext = apiDbContext;
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
        }

        #endregion Constructor

        #region Odata
        public override async Task<IQueryable<ClientUserSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.ClientUsers;
            IQueryable<ClientUserSM> retSM = await MapEntityAsToQuerable<ClientUserDM, ClientUserSM>(_mapper, entitySet);
            return retSM;
        }

        #endregion Odata


        #region Get
        /// <summary>
        /// Fetches All Client Users
        /// </summary>
        /// <returns>
        /// If Successful, returns List of ClientUserSM
        /// </returns>
        public async Task<List<ClientUserSM>> GetAllClientUsers()
        {
            var dm = await _apiDbContext.ClientUsers.AsNoTracking().ToListAsync();
            var sm = _mapper.Map<List<ClientUserSM>>(dm);
            return sm;
        }

        /// <summary>
        /// Fetches Client User By Id
        /// </summary>
        /// <returns>
        /// If Successful, returns ClientUserSM with Base64 image , Otherwise returns null
        /// </returns>

        public async Task<ClientUserSM> GetClientUserById(int id)
        {
            ClientUserDM clientUserDM = await _apiDbContext.ClientUsers.FindAsync(id);
            string profilePicture = null;
            if (clientUserDM != null)
            {
                var sm = _mapper.Map<ClientUserSM>(clientUserDM);
                if(!string.IsNullOrEmpty(sm.ProfilePicturePath))
                {
                    profilePicture = await ConvertToBase64(sm.ProfilePicturePath);
                    sm.ProfilePicturePath = profilePicture;
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
        /// Updates Client User Details using Id and ClientUserSM object
        /// </summary>
        /// <param name="Id">Id of Client User to Update</param>
        /// <param name="objSM">Object of type ClienetUserSM to Update</param>
        /// <returns>
        /// If Successful, returns Updated ClientUserSM Otherwise return null
        /// </returns>
        public async Task<ClientUserSM> UpdateClientUserDetails(int userId, ClientUserSM objSM)
        {
            if (userId == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Please Provide Value to Id");
            }

            if (objSM == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Nothing to Update");

            }

            var objDM = await _apiDbContext.ClientUsers
            .Where(x => x.Id == userId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(objSM.LoginId))
            {
                if (objSM.LoginId != objDM.LoginId && objSM.LoginId.Length < 6)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.NotFoundException, "Please provide LoginId with minimum 5 characters");
                }
            }

            var existingUser = await _apiDbContext.ClientUsers
                   .Where(l => l.LoginId == objSM.LoginId)
                   .FirstOrDefaultAsync();
            string imageFullPath = null;
            if (objDM != null)
            {
                objSM.Id = objDM.Id;
                objSM.PasswordHash = objDM.PasswordHash;
                objSM.ClientCompanyDetailId = (int)objDM.ClientCompanyDetailId; 

                if (!string.IsNullOrEmpty(objSM.ProfilePicturePath))
                {
                    imageFullPath = Path.GetFullPath(objDM.ProfilePicturePath);
                    var IsCompanyLogoUpdated = await UpdateProfilePicture(objDM.RoleType.ToString(), userId, objSM.ProfilePicturePath);
                    if (IsCompanyLogoUpdated != null)
                    {
                        objSM.ProfilePicturePath = null;
                    }
                }
                else
                {
                    objSM.ProfilePicturePath = objDM.ProfilePicturePath;
                }

                if (existingUser != null && objSM.LoginId != objDM.LoginId)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.AccessDeniedLog, $"User With Login Id: {objSM.LoginId} Already Existed...Choose Another LoginId");
                }
                objSM.LoginStatus = (LoginStatusSM)objDM.LoginStatus;
                objSM.IsEmailConfirmed = objDM.IsEmailConfirmed;
                objSM.IsPhoneNumberConfirmed = objDM.IsPhoneNumberConfirmed;
                objSM.RoleType = (RoleTypeSM)objDM.RoleType;

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
                    var updatedClientUser = await GetClientUserById(userId);
                    return updatedClientUser;
                }
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Something went wrong while Updating User Details");

            }
            else
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Data to update not found, add as new instead.");

            }
        }

        #endregion Update Client User (Mine)

        #region Update Profile Picture
        /// <summary>
        /// Updates Profile Picture of a ClientUser
        /// </summary>
        /// <param name="userRole">Defines role of a user (Here it is CompanyAdmin) </param>
        /// <param name="base64String">Base64 string which represents the profile picture of a Client User</param>
        /// <returns>
        /// Returns message (string) response
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        private async Task<string> UpdateProfilePicture(string userRole, int userId, string base64String)
        {
            var imageFullPath = "";
            if (userRole == RoleTypeDM.CompanyAdmin.ToString())
            {

                var objDM = await _apiDbContext.ClientUsers.FirstOrDefaultAsync(s => s.Id == userId);

                if (objDM == null)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"User Profile Picture Cannot be updated...Please check Again");
                }

                if (!string.IsNullOrEmpty(objDM.ProfilePicturePath))
                {
                    imageFullPath = Path.GetFullPath(objDM.ProfilePicturePath);
                }

                if (base64String == null)
                {
                    // If base64String is null, update CompanyLogoPath to null
                    objDM.ProfilePicturePath = null;
                }
                else
                {
                    // Convert base64String to image and store it inside a folder
                    // Return the relative path of the image
                    var imageRelativePath = await SaveFromBase64(base64String);

                    if (imageRelativePath != null)
                    {
                        objDM.ProfilePicturePath = imageRelativePath;
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
                return "Profile Picture Updated";
            }
            catch (SIMSException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Cannot Update Profile Picture");
            }

        }


        #endregion Update Profile Picture

        #region SignUp Client User
        /// <summary>
        /// Method Used for SignUp New Cliet User
        /// </summary>
        /// <param name="signUpSM">SignUp Object to register a new Client User</param>
        /// <returns>If successful, returns ClientUserSM Otherwise returns null</returns>
        /// <exception cref="SIMSException"></exception>

        public async Task<ClientUserSM?> SignUpClientUser(ClientUserSM signUpSM)
        {
            if (signUpSM == null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Please provide details for Sign Up");
            }
            if (signUpSM.LoginId.Length < 5)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Please provide LoginId with minimum 5 characters");
            }

            var objDM = _mapper.Map<ClientUserDM>(signUpSM);

            // Validate EmailId and LoginId
            var existingWithEmail = await _apiDbContext.ClientUsers.FirstOrDefaultAsync(x => x.EmailId == objDM.EmailId);
            if (existingWithEmail != null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Client User with EmailId Already Exist...Try Another EmailId");
            }

            var existingWithLoginId = await _apiDbContext.ClientUsers.FirstOrDefaultAsync(x => x.LoginId == objDM.LoginId);
            if (existingWithLoginId != null)
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Client User with LoginId Already Exist...Try Another LoginId");
            }

            // Set ClientCompanyDetailId to null if not provided or if it doesn't exist in the database
            if (!signUpSM.ClientCompanyDetailId.HasValue || !await _apiDbContext.ClientCompany.AnyAsync(x => x.Id == signUpSM.ClientCompanyDetailId))
            {
                objDM.ClientCompanyDetailId = null;
            }


            if (string.IsNullOrEmpty(signUpSM.PasswordHash))
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, $"Password Is Mandatory");
            }

            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(signUpSM.PasswordHash);
            objDM.RoleType = RoleTypeDM.CompanyAdmin;
            objDM.PasswordHash = passwordHash;
            objDM.CreatedBy = _loginUserDetail.LoginId;
            objDM.CreatedOnUTC = DateTime.UtcNow;
            objDM.IsEmailConfirmed = true;
            objDM.IsPhoneNumberConfirmed = true;
            objDM.LoginStatus = LoginStatusDM.Enabled;

            // Add client user to the database
            await _apiDbContext.ClientUsers.AddAsync(objDM);

            // If save changes is successful, return the saved client user
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var createdUser = await GetClientUserById(objDM.Id);
                return createdUser;
            }
            return null;
        }


        #endregion SignUp Client User

        #region Delete

        /// <summary>
        /// Deletes ClientUser by Id from the database 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<DeleteResponseRoot> DeleteClientUserById(int id)
        {
            var clientToDelete = await _apiDbContext.ClientUsers // Load related `ClientCompanyDetail`
                .FirstOrDefaultAsync(f => f.Id == id);
                // Remove the client user
                _apiDbContext.ClientUsers.Remove(clientToDelete);

                // Save changes
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, "Client User Deleted Successfully...");
                }

            return new DeleteResponseRoot(false, "User Not found");
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
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\loginusers\profile");

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

