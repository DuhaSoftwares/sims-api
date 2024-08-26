using Duha.SIMS.DAL.Seeds;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Warehouse;
using Duha.SIMS.DomainModels.Product;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.DAL.Contexts
{
    public class ApiDbContext : EfCoreContextRoot
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUserDM> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserAddressDM> ApplicationUserAddress { get; set; }
        public DbSet<ClientUserDM> ClientUsers { get; set; }
        public DbSet<ClientUserAddressDM> ClientUserAddress { get; set; }
        public DbSet<ClientCompanyDetailDM> ClientCompany { get; set; }
        public DbSet<ClientCompanyAddressDM> ClientCompanyAddress { get; set; }
        public DbSet<WarehouseDM> Warehouses { get; set; }
        public DbSet<BrandDM> Brands { get; set; }
        public DbSet<ProductCategoryDM> ProductCategories { get; set; }
        public DbSet<ProductDM> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure unique constraint on Email and UserName
            modelBuilder.Entity<ClientUserDM>()
                .HasIndex(u => u.EmailId)
                .IsUnique();

            modelBuilder.Entity<ClientUserDM>()
                .HasIndex(u => u.LoginId)
                .IsUnique();

            // Seed database with initial data
            DatabaseSeeder<ApiDbContext> seeder = new DatabaseSeeder<ApiDbContext>();
            seeder.SetupDatabaseWithSeedData(modelBuilder);
        }
    }
}
