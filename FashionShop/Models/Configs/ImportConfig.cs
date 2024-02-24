using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models
{
    public class ImportConfig : EntityTypeConfiguration<Import>
    {
        public ImportConfig()
        {
            this.HasMany<ImportDetail>(x => x.ImportDetails);
            this.HasRequired(p => p.Account)
                   .WithMany(c => c.Imports)
                   .HasForeignKey(p => p.CreateUserId)
                   .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}
