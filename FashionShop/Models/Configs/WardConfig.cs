using FashionShop.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models
{
    public class WardConfig : EntityTypeConfiguration<Ward>
    {
        public WardConfig()
        {
            this.HasRequired(p => p.District)
                       .WithMany(c => c.Wards)
                       .HasForeignKey(p => p.DistrictId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}
