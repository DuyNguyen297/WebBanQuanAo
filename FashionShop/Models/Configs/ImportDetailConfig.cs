using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models
{
    public class ImportDetailConfig : EntityTypeConfiguration<ImportDetail>
    {
        public ImportDetailConfig()
        {
            this.HasRequired(p => p.Product)
                       .WithMany(c => c.ImportDetails)
                       .HasForeignKey(p => p.ProductId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
            this.HasRequired(p => p.Import)
                       .WithMany(c => c.ImportDetails)
                       .HasForeignKey(p => p.ImportId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}
