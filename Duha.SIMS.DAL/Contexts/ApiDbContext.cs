using Duha.SIMS.DAL.Seeds;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Warehouse;
using Duha.SIMS.DomainModels.Product;
using Microsoft.EntityFrameworkCore;
using Duha.SIMS.DomainModels.Customer;

namespace Duha.SIMS.DAL.Contexts
{
    public class ApiDbContext : EfCoreContextRoot
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<ApplicationUserDM> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserAddressDM> ApplicationUserAddress { get; set; }
        public DbSet<ClientUserDM> ClientUsers { get; set; }
        public DbSet<ClientUserAddressDM> ClientUserAddress { get; set; }
        public DbSet<ClientCompanyDetailDM> ClientCompany { get; set; }
        public DbSet<ClientCompanyAddressDM> ClientCompanyAddress { get; set; }
        public DbSet<WarehouseDM> Warehouses { get; set; }
        public DbSet<BrandDM> Brands { get; set; }
        public DbSet<SupplierDM> Suppliers { get; set; }
        public DbSet<ProductCategoryDM> ProductCategories { get; set; }
        public DbSet<UnitsDM> Units { get; set; }
        public DbSet<ProductDM> Products { get; set; }
        public DbSet<ProductDetailsDM> ProductDetails { get; set; }
        public DbSet<VariantDM> Variants { get; set; }
        public DbSet<CategoryVariantDM> CategoryVariants { get; set; }
        public DbSet<ProductVariantDM> ProductVariants { get; set; }
        public DbSet<CustomerDM> Customers { get; set; }

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

            // Configure relationship between ProductVariantDM and ProductDM
            modelBuilder.Entity<ProductVariantDM>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariants) // Assuming ProductDM has a collection of ProductVariants
                .HasForeignKey(pv => pv.ProductId);

            // Configure VariantLevel1 relationship
            modelBuilder.Entity<ProductVariantDM>()
                .HasOne(pv => pv.VariantLevel1)
                .WithMany() // Assuming VariantDM does not have a collection of ProductVariants for VariantLevel1
                .HasForeignKey(pv => pv.VariantLevel1Id)
                .HasPrincipalKey(v => v.Id) // Assuming Id is the primary key in VariantDM
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // Configure VariantLevel2 relationship
            modelBuilder.Entity<ProductVariantDM>()
                .HasOne(pv => pv.VariantLevel2)
                .WithMany() // Assuming VariantDM does not have a collection of ProductVariants for VariantLevel2
                .HasForeignKey(pv => pv.VariantLevel2Id)
                .HasPrincipalKey(v => v.Id) // Assuming Id is the primary key in VariantDM
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // Seed database with initial data
            DatabaseSeeder<ApiDbContext> seeder = new DatabaseSeeder<ApiDbContext>();
            seeder.SetupDatabaseWithSeedData(modelBuilder);
        }
    }
}
