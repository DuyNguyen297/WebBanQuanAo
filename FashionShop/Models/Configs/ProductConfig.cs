using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Models.Configs
{
    public class ProductConfig : EntityTypeConfiguration<Product>
    {
        public ProductConfig()
        {
            this.Property(x => x.Description).HasColumnType("ntext");
            this.HasMany<Feedback>(x => x.Feedbacks);
            this.HasMany<Cart>(x => x.Carts);
            this.HasMany<OrderDetail>(x => x.OrderDetails);
            this.HasMany<ImportDetail>(x => x.ImportDetails);
            this.HasMany<VoucherProduct>(x => x.VoucherProducts);
            // Cấu hình quy tắc xóa cho quan hệ với Category
            this.HasRequired(p => p.Category)
                       .WithMany(c => c.Products)
                       .HasForeignKey(p => p.CategoryId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
            this.HasRequired(p => p.Branch)
                       .WithMany(c => c.Products)
                       .HasForeignKey(p => p.BranchId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"

            this.HasRequired(p => p.Color)
                       .WithMany(c => c.Products)
                       .HasForeignKey(p => p.ColorId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
            this.HasRequired(p => p.Size)
                       .WithMany(c => c.Products)
                       .HasForeignKey(p => p.SizeId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}
