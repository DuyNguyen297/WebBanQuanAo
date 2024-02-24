using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class SlideConfig : EntityTypeConfiguration<Slide>
    {
        public SlideConfig()
        {
            this.Property(x => x.Title).HasColumnType("ntext");
            this.Property(x => x.Content).HasColumnType("ntext");
        }
    }
}