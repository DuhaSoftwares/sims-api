﻿using AutoMapper;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.AppUsers.Login;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Warehouse;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.ServiceModels.AppUsers;
using Duha.SIMS.ServiceModels.AppUsers.Login;
using Duha.SIMS.ServiceModels.Client;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.Product;
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
            CreateMap<BrandDM, BrandSM>().ReverseMap();
            CreateMap<ProductDM, ProductSM>().ReverseMap();
            CreateMap<ProductCategoryDM, ProductCategorySM>().ReverseMap();
            CreateMap<UnitsDM, UnitsSM>().ReverseMap();
            CreateMap<VariantDM, VariantSM>().ReverseMap();
            CreateMap<CategoryVariantDM, CategoryVariantSM>().ReverseMap();
            CreateMap<GenderDM, GenderSM>().ReverseMap();
            CreateMap<CategoryLevelDM, CategoryLevelSM>().ReverseMap();
            CreateMap<StorageTypeDM, StorageTypeSM>().ReverseMap();
            CreateMap<RoleTypeDM, RoleTypeSM>().ReverseMap();
            CreateMap<VariantLevelDM, VariantLevelSM>().ReverseMap();
        }
    }
}
