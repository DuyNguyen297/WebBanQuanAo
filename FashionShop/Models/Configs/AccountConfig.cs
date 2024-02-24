using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class AccountConfig : EntityTypeConfiguration<Account>
    {
        public AccountConfig()
        {
            this.Property(x => x.Name);
            this.Property(x => x.UserName);
            this.Property(x => x.Email);
            this.Property(x => x.Phone);
            this.Property(x => x.Password);
            this.Property(x => x.Role);
            this.HasMany<Order>(x => x.Orders);
            this.HasMany<Import>(x => x.Imports);
            this.HasMany<VoucherShip>(x => x.VoucherShips);
            this.HasMany<VoucherProduct>(x => x.VoucherProducts);
            this.HasMany<VoucherCategory>(x => x.VoucherCategories);
            this.HasMany<VoucherCustomer>(x => x.VoucherCustomer);
        }
    }
}
