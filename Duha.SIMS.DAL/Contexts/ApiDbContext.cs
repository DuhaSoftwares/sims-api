/*using Duha.SIMS.DAL.Seeds;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Client;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.DAL.Contexts
{
    public class ApiDbContext : EfCoreContextRoot
    {
        public ApiDbContext(DbContextOptions options)
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Data Source=SQL8004.site4now.net;Initial Catalog=db_aabe7d_sims;User Id=db_aabe7d_sims_admin;Password=sims1234");
            optionsBuilder.UseSqlServer("Data Source=host.ukserverhosting.org;Initial Catalog=duhasoft_sims;User Id=duhasoft_soft;Password=l7!s542Oq");
            //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SimsDB;Trusted_Connection=True;MultipleActiveResultSets=true");
            base.OnConfiguring(optionsBuilder);
        }
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
            DatabaseSeeder<ApiDbContext> seeder = new DatabaseSeeder<ApiDbContext>();
            seeder.SetupDatabaseWithSeedData(modelBuilder);

        }
    }
}
*/

using Duha.SIMS.DAL.Seeds;
using Duha.SIMS.DomainModels.AppUsers;
using Duha.SIMS.DomainModels.Client;
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
