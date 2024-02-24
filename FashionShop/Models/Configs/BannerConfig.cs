using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class BannerConfig : EntityTypeConfiguration<Banner>
    {
        public BannerConfig()
        {
            this.Property(x => x.Title).HasColumnType("ntext");
            this.Property(x => x.Content).HasColumnType("ntext");
        }
    }
}