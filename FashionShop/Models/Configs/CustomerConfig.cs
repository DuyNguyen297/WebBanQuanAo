using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;

namespace FashionShop.Models.Configs
{
    public class CustomerConfig : EntityTypeConfiguration<Customer>
    {
        public CustomerConfig()
        {
            this.Property(x => x.Address).HasColumnType("ntext");
            this.Property(x => x.Password);
            this.HasMany<Order>(x => x.Orders);
            this.HasMany<Feedback>(x => x.Feedbacks);
            this.HasMany<VoucherCustomer>(x => x.VoucherCustomers);
        }
    }
}

