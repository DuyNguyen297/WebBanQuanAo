using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class ColorConfig : EntityTypeConfiguration<Color>
    {
        public ColorConfig()
        {
            this.Property(x => x.Code);
            this.Property(x => x.Name);
            this.HasMany<Product>(x => x.Products);
        }
    }
}