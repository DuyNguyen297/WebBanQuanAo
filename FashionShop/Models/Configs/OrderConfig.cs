using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;

namespace FashionShop.Models.Configs
{
    public class OrderConfig : EntityTypeConfiguration<Order>
    {
        public OrderConfig()
        {
            this.Property(x => x.Address).HasColumnType("ntext");
            this.Property(x => x.Reason).HasColumnType("ntext");
            this.HasMany<OrderDetail>(x => x.OrderDetails);
            this.HasRequired(p => p.Customer)
                       .WithMany(c => c.Orders)
                       .HasForeignKey(p => p.CustomerId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
			
		}
    }
}

