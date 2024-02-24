using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class CategoryConfig : EntityTypeConfiguration<Category>
    {
        public CategoryConfig()
        {
            this.Property(x => x.Code);
            this.Property(x => x.Name);
            this.Property(x => x.Description).HasColumnType("ntext");
            this.HasMany<Product>(x => x.Products);
            this.HasMany<VoucherCategory>(x => x.VoucherCategories);
        }
    }
}