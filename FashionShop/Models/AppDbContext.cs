using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Linq;
using FashionShop.Models;
using FashionShop.Models.Configs;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace FashionShop.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<ImportDetail> ImportDetails { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<VoucherShip> VoucherShips { get; set; }
        public DbSet<VoucherProduct> VoucherProducts { get; set; }
        public DbSet<VoucherCustomer> VoucherCustomers { get; set; }
        public DbSet<VoucherCategory> VoucherCategories { get; set; }

        public AppDbContext() : base("ConnectionString")
        {
            Database.SetInitializer<AppDbContext>(new CreateDatabaseIfNotExists<AppDbContext>());
           // Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, FashionShop.Migrations.Configuration>());
        }
        public class AppDbInitializer : CreateDatabaseIfNotExists<AppDbContext>
        {
            protected override void Seed(AppDbContext context)
            {
                base.Seed(context);
            }
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AccountConfig());
            modelBuilder.Configurations.Add(new CustomerConfig());
            modelBuilder.Configurations.Add(new ProductConfig());
            modelBuilder.Configurations.Add(new CategoryConfig());
            modelBuilder.Configurations.Add(new BranchConfig());
            modelBuilder.Configurations.Add(new OrderDetailConfig()); 
            modelBuilder.Configurations.Add(new ImportDetailConfig());
            modelBuilder.Configurations.Add(new OrderConfig());
            modelBuilder.Configurations.Add(new ContactConfig());
            modelBuilder.Configurations.Add(new FeedbackConfig());
            modelBuilder.Configurations.Add(new ColorConfig());
            modelBuilder.Configurations.Add(new SizeConfig());
            modelBuilder.Configurations.Add(new CartConfig());
            modelBuilder.Configurations.Add(new SlideConfig());
            modelBuilder.Configurations.Add(new BannerConfig());
            modelBuilder.Configurations.Add(new VoucherProductConfig());
            modelBuilder.Configurations.Add(new VoucherCustomerConfig());
            modelBuilder.Configurations.Add(new VoucherCategoryConfig());
            modelBuilder.Configurations.Add(new VoucherShipConfig());
        }

  
    }
}