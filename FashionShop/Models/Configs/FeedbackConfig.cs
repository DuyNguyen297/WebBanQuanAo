using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class FeedbackConfig : EntityTypeConfiguration<Feedback>
    {
        public FeedbackConfig()
        {
            this.Property(x => x.Message).HasColumnType("ntext");
            this.HasRequired(p => p.Product)
                       .WithMany(c => c.Feedbacks)
                       .HasForeignKey(p => p.ProductId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
            this.HasRequired(p => p.Customer)
                       .WithMany(c => c.Feedbacks)
                       .HasForeignKey(p => p.CreateUserId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}