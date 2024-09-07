using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.AppUsers.Login;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.AppUsers.Login;


namespace Duha.SIMS.BAL.AppUser
{
    public abstract class LoginUserProcess<T> : SIMSBalOdataBase<T>
    {
        //protected readonly ILoginUserDetail _loginUserDetail;

        public LoginUserProcess(IMapper mapper, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
        }

        #region CRUD 

        #region Add Update

        protected async Task<string> AddOrUpdateProfilePictureInDb(LoginUserDM targetLoginUser, string webRootPath, IFormFile postedFile)
        {
            /*if (targetLoginUser != null)
            {
               
                var currLogoPath = targetLoginUser.ProfilePicturePath;
                var targetRelativePath = Path.Combine("content\\loginusers\\profile", $"{targetLoginUser.Id}_{Guid.NewGuid()}_original{Path.GetExtension(postedFile.FileName)}");
                var targetPath = Path.Combine(webRootPath, targetRelativePath);
                if (await base.SavePostedFileAtPath(postedFile, targetPath))
                {
                    //Entry Method//
                    //var comp = new ClientCompanyDetailDM() { Id = companyId, CompanyLogoPath = targetRelativePath };
                    //_apiDbContext.ClientCompanyDetails.Attach(comp);
                    //_apiDbContext.Entry(comp).Property(e => e.CompanyLogoPath).IsModified = true;
                    targetLoginUser.ProfilePicturePath = WebExtensions.ConvertFromFilePathToUrl(targetRelativePath);
                    targetLoginUser.LastModifiedBy = _loginUserDetail.LoginId;
                    targetLoginUser.LastModifiedOnUTC = DateTime.UtcNow;
                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(currLogoPath))
                        { File.Delete(Path.Combine(webRootPath, currLogoPath)); }
                        return WebExtensions.ConvertFromFilePathToUrl(targetRelativePath);
                    }
                }
            }*/
            return "";
        }

        #endregion Add Update

        #region Delete

        protected async Task<DeleteResponseRoot> DeleteProfilePictureById(LoginUserSM targetLoginUser, string webRootPath)
        {
            if (targetLoginUser != null)
            {
                var currLogoPath = targetLoginUser.ProfilePicturePath;
                targetLoginUser.ProfilePicturePath = "";
                //targetLoginUser.LastModifiedBy = _loginUserDetail.LoginId;
                targetLoginUser.LastModifiedOnUTC = DateTime.UtcNow;

                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    if (!string.IsNullOrWhiteSpace(currLogoPath))
                    {
                        File.Delete(Path.Combine(webRootPath, currLogoPath));
                        return new DeleteResponseRoot(true);
                    }
                }
            }
            return new DeleteResponseRoot(false, "User or Picture Not found");
        }

        #endregion Delete

        #endregion CRUD

        #region Private Functions
        #endregion Private Functions

    }
}
