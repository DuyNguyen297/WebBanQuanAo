using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;

namespace FashionShop.Models.Configs
{
    public class CartConfig : EntityTypeConfiguration<Cart>
    {
        public CartConfig()
        {
            this.HasRequired(p => p.Product)
                       .WithMany(c => c.Carts)
                       .HasForeignKey(p => p.ProductId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}

