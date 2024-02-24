using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Models.Configs
{
    public class VoucherCategoryConfig : EntityTypeConfiguration<VoucherCategory>
    {
        public VoucherCategoryConfig()
        {
			this.HasRequired(p => p.Category)
		   .WithMany(c => c.VoucherCategories)
		   .HasForeignKey(p => p.CategoryId)
		   .WillCascadeOnDelete(true); // or true for "on delete cascade"
		}
    }
}
