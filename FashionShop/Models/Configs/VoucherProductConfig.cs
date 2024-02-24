using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Models.Configs
{
    public class VoucherProductConfig : EntityTypeConfiguration<VoucherProduct>
    {
        public VoucherProductConfig()
        {
			this.HasRequired(p => p.Product)
		   .WithMany(c => c.VoucherProducts)
		   .HasForeignKey(p => p.ProductId)
		   .WillCascadeOnDelete(true); // or true for "on delete cascade"
		}
    }
}
