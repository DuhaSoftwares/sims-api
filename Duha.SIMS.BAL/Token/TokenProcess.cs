using AutoMapper;
using Duha.SIMS.BAL.Interface;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.ServiceModels.AppUsers.Login;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.ServiceModels.v1.General.Token;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Token
{
    public partial class TokenProcess : SIMSBalBase
    {
        #region Properties

        private readonly IPasswordEncryptHelper _passwordEncryptHelper;

        #endregion Properties

        #region Constructor
        public TokenProcess(IMapper mapper, ApiDbContext context, IPasswordEncryptHelper passwordEncryptHelper) : base(mapper, context)
        {
            _passwordEncryptHelper = passwordEncryptHelper;
        }

        #endregion Constructor

        #region Token
        public async Task<(LoginUserSM, int)> ValidateLoginAndGenerateToken(TokenRequestSM tokenReq)
        {
            LoginUserSM? loginUserSM = null;
            int compId = default;
            // add hash
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(tokenReq.Password);
            var passwordHash2 = await _passwordEncryptHelper.ProtectAsync<string>(tokenReq.Password);
            var decryptedPassword = await _passwordEncryptHelper.UnprotectAsync<string>(passwordHash);
            var decryptedPassword2 = await _passwordEncryptHelper.UnprotectAsync<string>(passwordHash2);
            switch (tokenReq.RoleType)
            {
                case RoleTypeSM.SuperAdmin:
                case RoleTypeSM.SystemAdmin:
                    var appUser = await _apiDbContext.ApplicationUsers
                        //.FirstOrDefaultAsync(x => x.LoginId == tokenReq.LoginId
                        .FirstOrDefaultAsync(x => (x.LoginId == tokenReq.LoginId || x.EmailId == tokenReq.LoginId)
                        && x.PasswordHash == passwordHash && x.RoleType == (RoleTypeDM)tokenReq.RoleType);
                    if (appUser != null)
                    { loginUserSM = _mapper.Map<ApplicationUserSM>(appUser); }

                    break;
                /*case RoleTypeSM.ClientAdmin:
                case RoleTypeSM.ClientEmployee:
                    {
                        var data = await (from comp in _apiDbContext.ClientCompanyDetails
                                          join user in _apiDbContext.ClientUsers
                                          on comp.Id equals user.ClientCompanyDetailId
                                          where user.LoginId == tokenReq.LoginId && user.PasswordHash == passwordHash
                                          && comp.CompanyCode == tokenReq.CompanyCode && user.RoleType == (RoleTypeDM)tokenReq.RoleType
                                          select new { User = user, CompId = comp.Id }).FirstOrDefaultAsync();
                        if (data != null && data.User != null)
                        {
                            loginUserSM = _mapper.Map<ClientUserSM>(data.User);
                            compId = data.CompId;
                        }
                    }
                    break;*/
                case RoleTypeSM.CompanyAdmin:
                    {
                        if (!string.IsNullOrEmpty(tokenReq.CompanyCode))
                        {
                            var data = await (from comp in _apiDbContext.ClientCompany
                                              join user in _apiDbContext.ClientUsers
                                              on comp.Id equals user.ClientCompanyDetailId
                                              //where user.LoginId == tokenReq.LoginId
                                              where (user.LoginId == tokenReq.LoginId || user.EmailId == tokenReq.LoginId)
                                              && user.PasswordHash == passwordHash
                                              && comp.CompanyCode == tokenReq.CompanyCode && user.RoleType == (RoleTypeDM)tokenReq.RoleType
                                              select new { User = user, CompId = comp.Id }).FirstOrDefaultAsync();
                            if (data != null && data.User != null)
                            {
                                loginUserSM = _mapper.Map<LoginUserSM>(data.User);
                                compId = data.CompId;
                            }
                            break;
                        }
                        else
                        {
                            var data = await _apiDbContext.ClientUsers
                                .FirstOrDefaultAsync(x => (x.LoginId == tokenReq.LoginId || x.EmailId == tokenReq.LoginId) && x.PasswordHash == passwordHash && x.RoleType == (RoleTypeDM)tokenReq.RoleType);
                            //Todo: Should be used dataWithoutCompanyCode as a new user is not necessary to have company code 
                            /*var dataWithoutCompanyCode = await _apiDbContext.Farmers
                                .FirstOrDefaultAsync(x => x.LoginId == tokenReq.LoginId && x.PasswordHash == passwordHash 
                                && x.RoleType == (RoleTypeDM)tokenReq.RoleType && x.UserCompanyDetailId == null);*/
                            if (data != null)
                            { loginUserSM = _mapper.Map<LoginUserSM>(data); }
                            break;
                        }

                    }
            }
            return (loginUserSM, compId);
        }
        #endregion Token
    }
}
