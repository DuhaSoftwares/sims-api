﻿using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.DomainModels.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.DAL.Seeds
{
    public class DatabaseSeeder<T> where T : EfCoreContextRoot
    {
        public void SetupDatabaseWithSeedData(ModelBuilder modelBuilder)
        {
            var defaultCreatedBy = "SeedAdmin";
            //SeedDummyData(modelBuilder, defaultCreatedBy);
            SeedDummyCompanyData(modelBuilder, defaultCreatedBy);
        }
        public bool SetupDatabaseWithTestData(T context, Func<string, string> encryptorFunc)
        {
            var defaultCreatedBy = "SeedAdmin";
            var defaultUpdatedBy = "UpdateAdmin";
            var apiDb = context as ApiDbContext;
            if (apiDb != null && apiDb.ApplicationUsers.Count() == 0)
            {
                SeedDummySuperAdminUsers(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedDummySystemAdminUsers(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedDummyClientAdminUsers(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedDummyClientAdminAddress(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedBrands(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedUnits(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedWarehouses(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedProductCategories(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedVariants(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                SeedCategoryVariants(apiDb, defaultCreatedBy, defaultUpdatedBy, encryptorFunc);
                return true;
            }
            return false;
        }



        #region Data To Entities

        #region Dummy Data

        /* private void SeedDummyData(ModelBuilder modelBuilder, string defaultCreatedBy)
         {
             modelBuilder.Entity<DummyTeacherDM>().HasData(
                 new DummyTeacherDM() { Id = 1, FirstName = "Teacher A", LastName = "Khan", CreatedBy = defaultCreatedBy },
                 new DummyTeacherDM() { Id = 2, FirstName = "Teacher B", LastName = "Kumar", CreatedBy = defaultCreatedBy },
                 new DummyTeacherDM() { Id = 3, FirstName = "Teacher C", LastName = "Johar", CreatedBy = defaultCreatedBy }
                 );

             modelBuilder.Entity<DummySubjectDM>().HasData(
                 new DummySubjectDM() { Id = 1, SubjectName = "Physics", SubjectCode = "phy", DummyTeacherID = 1, CreatedBy = defaultCreatedBy },
                 new DummySubjectDM() { Id = 2, SubjectName = "Chemistry", SubjectCode = "chem", DummyTeacherID = 2, CreatedBy = defaultCreatedBy },
                 new DummySubjectDM() { Id = 3, SubjectName = "Biology", SubjectCode = "bio", DummyTeacherID = 1, CreatedBy = defaultCreatedBy }
             );
         }*/

        #endregion Dummy Data

        #region Companies
        private void SeedDummyCompanyData(ModelBuilder modelBuilder, string defaultCreatedBy)
        {
            var renoComp = new ClientCompanyDetailDM()
            {
                Id = 1,
                Name = "Duha-Softwares",
                CompanyCode = "123",
                Description = "Software Development Company",
                ContactEmail = "DuhaSoftwares@outlook.com",
                CompanyMobileNumber = "9876542341",
                CompanyWebsite = "www.duhasoftwares.com",
                CompanyLogoPath = "wwwroot/content/companies/logos/company.jpg",
                CompanyDateOfEstablishment = new DateTime(1990, 1, 1),
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow,
                //ClientCompanyAddressId = 2
            };
            var clustComp = new ClientCompanyDetailDM()
            {
                Id = 2,
                Name = "Wedding-Mart",
                CompanyCode = "124",
                Description = "Software Development Company",
                ContactEmail = "weddingmart@gmail.com",
                CompanyMobileNumber = "1234567890",
                CompanyWebsite = "www.wmart.com",
                CompanyLogoPath = "wwwroot/content/companies/logos/company.jpg",
                CompanyDateOfEstablishment = new DateTime(2009, 1, 1),
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow,
                //ClientCompanyAddressId = 1
            };
            modelBuilder.Entity<ClientCompanyDetailDM>().HasData(renoComp, clustComp);
        }

        #endregion Companies

        #region Users

        private void SeedDummySuperAdminUsers(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var superUser1 = new ApplicationUserDM()
            {
                RoleType = RoleTypeDM.SuperAdmin,
                FirstName = "Super",
                MiddleName = "Admin",
                EmailId = "saone@email.com",
                LastName = "One",
                LoginId = "super1",
                IsEmailConfirmed = true,
                LoginStatus = LoginStatusDM.Enabled,
                PhoneNumber = "1234567890",
                IsPhoneNumberConfirmed = true,
                PasswordHash = encryptorFunc("pass123"),
                ProfilePicturePath = "wwwroot/content/loginusers/profile/profile.jpg",
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
            var superUser2 = new ApplicationUserDM()
            {
                RoleType = RoleTypeDM.SuperAdmin,
                FirstName = "Super",
                MiddleName = "Admin",
                LastName = "Two",
                EmailId = "satwo@email.com",
                LoginId = "super2",
                PhoneNumber = "1234567890",
                ProfilePicturePath = "wwwroot/content/loginusers/profile/profile.jpg",
                IsEmailConfirmed = true,
                LoginStatus = LoginStatusDM.Enabled,
                IsPhoneNumberConfirmed = true,
                PasswordHash = encryptorFunc("pass123"),
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
            apiDb.ApplicationUsers.Add(superUser1);
            apiDb.SaveChanges();
            apiDb.ApplicationUsers.Add(superUser2);
            apiDb.SaveChanges();
        }
        private void SeedDummySystemAdminUsers(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var sysUser1 = new ApplicationUserDM()
            {
                RoleType = RoleTypeDM.SystemAdmin,
                FirstName = "System",
                MiddleName = "Admin",
                EmailId = "sysone@email.com",
                LastName = "One",
                LoginId = "system1",
                PhoneNumber = "1234567890",
                ProfilePicturePath = "wwwroot/content/loginusers/profile/profile.jpg",
                IsEmailConfirmed = true,
                LoginStatus = LoginStatusDM.Enabled,
                IsPhoneNumberConfirmed = true,
                PasswordHash = encryptorFunc("pass123"),
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
            var sysUser2 = new ApplicationUserDM()
            {
                RoleType = RoleTypeDM.SystemAdmin,
                FirstName = "System",
                MiddleName = "Admin",
                LastName = "Two",
                EmailId = "systwo@email.com",
                LoginId = "system2",
                PhoneNumber = "1234567890",
                ProfilePicturePath = "wwwroot/content/loginusers/profile/profile.jpg",
                IsEmailConfirmed = true,
                LoginStatus = LoginStatusDM.Enabled,
                IsPhoneNumberConfirmed = true,
                PasswordHash = encryptorFunc("pass123"),
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
            apiDb.ApplicationUsers.Add(sysUser1);
            apiDb.SaveChanges();
            apiDb.ApplicationUsers.Add(sysUser2);
            apiDb.SaveChanges();
        }
        private void SeedDummyClientAdminUsers(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var cAdmin1 = new ClientUserDM()
            {
                ClientCompanyDetailId = 1,
                RoleType = RoleTypeDM.CompanyAdmin,
                FirstName = "Company",
                MiddleName = "Admin",
                EmailId = "companyadmin1@email.com",
                PersonalEmailId = "companyadmin1@email.com",
                LastName = "One",
                LoginId = "companyadmin1",
                IsEmailConfirmed = true,
                PhoneNumber = "1234567890",
                LoginStatus = LoginStatusDM.Enabled,
                IsPhoneNumberConfirmed = true,
                PasswordHash = encryptorFunc("pass123"),
                ProfilePicturePath = "wwwroot/content/loginusers/profile/profile.jpg",
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
            var cAdmin2 = new ClientUserDM()
            {
                RoleType = RoleTypeDM.CompanyAdmin,
                ClientCompanyDetailId = 2,
                FirstName = "Company",
                MiddleName = "Admin",
                LastName = "Two",
                EmailId = "companyAdmin2@email.com",
                PersonalEmailId = "companyAdmin2@email.com",
                LoginId = "clientadmin2",
                PhoneNumber = "1234567890",
                ProfilePicturePath = "wwwroot/content/loginusers/profile/profile.jpg",
                IsEmailConfirmed = true,
                LoginStatus = LoginStatusDM.Enabled,
                IsPhoneNumberConfirmed = true,
                PasswordHash = encryptorFunc("pass123"),
                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
            apiDb.ClientUsers.Add(cAdmin1);
            apiDb.SaveChanges();
            apiDb.ClientUsers.Add(cAdmin2);
            apiDb.SaveChanges();
        }


        #endregion Users

        #region Client User Address

        private void SeedDummyClientAdminAddress(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var add1 = new ClientUserAddressDM()
            {
                City = "Srinagar",
                Country = "India",
                PinCode = "1234567890",
                State = "jammu and kashmir",
                Address1 = "Address1",
                Address2 = "Address2",

                CreatedBy = defaultCreatedBy,
                CreatedOnUTC = DateTime.UtcNow
            };
           
            apiDb.ClientUserAddress.Add(add1);
            apiDb.SaveChanges();
            
        }

        #endregion Client User Address

        #region Application Specific Tables

        #region seed Brands

        private void SeedBrands(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var brands = new List<BrandDM>()
            {
                new() {  Name = "Nike", ImagePath = "wwwroot/content/brands/nike.png", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Puma", ImagePath = "wwwroot/content/brands/puma.png", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow}
            };           

            apiDb.Brands.AddRange(brands);
            apiDb.SaveChanges();
        }

        #endregion seed Brands

        #region Seed Units

        private void SeedUnits(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var units = new List<UnitsDM>()
            {
                new() {  Name = "Kilogram", Symbol = "Kg", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Gram", Symbol = "g", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Litre", Symbol = "l", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Milli Litre", Symbol = "ml", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},

            };

            apiDb.Units.AddRange(units);
            apiDb.SaveChanges();
        }

        #endregion Seed Units

        #region Seed Warehouses

        private void SeedWarehouses(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var warehouses = new List<WarehouseDM>()
            {
                new() {  Name = "Warehouse 1", Description = "Demo Description", Capacity = 1000, ClientCompanyDetailId = 1, IsActive = true, Location = "Kashmir", ContactNumber = "1234567890", StorageType = StorageTypeDM.AUTOMATED, EmailId = "warehouse1@email.com", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Warehouse 2", Description = "Demo Description", Capacity = 2000, ClientCompanyDetailId = 2, IsActive = true, Location = "Jammu", ContactNumber = "1234567890", StorageType = StorageTypeDM.AUTOMATED, EmailId = "warehouse2@email.com", CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                
            };

            apiDb.Warehouses.AddRange(warehouses);
            apiDb.SaveChanges();
        }

        #endregion Seed Warehouses

        #region Product Categories

        private void SeedProductCategories(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var categories = new List<ProductCategoryDM>()
            {
                new() {  Name = "Electronics", Level = CategoryLevelDM.Level1, LevelId = null, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Clothing", Level = CategoryLevelDM.Level1, LevelId = null, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Furniture", Level = CategoryLevelDM.Level1, LevelId = null, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},

            };

            apiDb.ProductCategories.AddRange(categories);
            apiDb.SaveChanges();
        }

        #endregion Product Categories

        #region Seed Variants and category variants

        private void SeedVariants(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var variants = new List<VariantDM>()
            {
                new() {  Name = "RAM", VariantLevel =  VariantLevelDM.Level1, VariantId = null, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "ROM", VariantLevel =  VariantLevelDM.Level1, VariantId = null, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  Name = "Camera Pixels", VariantLevel =  VariantLevelDM.Level1, VariantId = null, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},

            };

            apiDb.Variants.AddRange(variants);
            apiDb.SaveChanges();
        }

        private void SeedCategoryVariants(ApiDbContext apiDb, string defaultCreatedBy, string defaultUpdatedBy, Func<string, string> encryptorFunc)
        {
            var catVariants = new List<CategoryVariantDM>()
            {
                new() {  ProductCategoryId = 1, VariantId = 1, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  ProductCategoryId = 1, VariantId = 2, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},
                new() {  ProductCategoryId = 1, VariantId = 3, CreatedBy = defaultCreatedBy, CreatedOnUTC = DateTime.UtcNow},

            };

            apiDb.CategoryVariants.AddRange(catVariants);
            apiDb.SaveChanges();
        }

        #endregion Seed Variants

        #endregion Application Specific Tables

        #endregion Data To Entities

    }
}
