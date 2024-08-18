using AutoMapper;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.AppUsers.Login;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Warehouse;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.AppUsers.Login;
using Duha.SIMS.ServiceModels.Client;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.Warehouse;

namespace Duha.SIMS.API.AutoMapperBindings
{
    public class AutoMapperDefaultProfile : Profile
    {
        public AutoMapperDefaultProfile()
        {
            CreateMap<LoginUserDM, LoginUserSM>().ReverseMap();
            CreateMap<ClientUserDM, ClientUserSM>().ReverseMap();
            CreateMap<ApplicationUserDM, ApplicationUserSM>().ReverseMap();
            CreateMap<ClientCompanyDetailDM, ClientCompanyDetailSM>().ReverseMap();
            CreateMap<WarehouseDM, WarehouseSM>().ReverseMap();
            CreateMap<GenderDM, GenderSM>().ReverseMap();
            CreateMap<StorageTypeDM, StorageTypeSM>().ReverseMap();
            CreateMap<RoleTypeDM, RoleTypeSM>().ReverseMap();
        }
    }
}
