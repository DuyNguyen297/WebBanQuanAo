using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;
namespace FashionShop.Models.Configs
{
    public class BranchConfig : EntityTypeConfiguration<Branch>
    {
        public BranchConfig()
        {
            this.Property(x => x.Code);
            this.Property(x => x.Name);
            this.Property(x => x.Description).HasColumnType("ntext");
            this.HasMany<Product>(x => x.Products);
        }
    }
}
