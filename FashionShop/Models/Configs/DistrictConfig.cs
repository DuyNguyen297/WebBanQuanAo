using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models
{
    public class DistrictConfig : EntityTypeConfiguration<District>
    {
        public DistrictConfig()
        {
			this.HasMany<Ward>(x => x.Wards);
			this.HasRequired(p => p.Province)
                       .WithMany(c => c.Districts)
                       .HasForeignKey(p => p.ProvinceId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}
