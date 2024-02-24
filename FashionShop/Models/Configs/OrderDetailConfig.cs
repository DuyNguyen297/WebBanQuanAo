using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models
{
    public class OrderDetailConfig : EntityTypeConfiguration<OrderDetail>
    {
        public OrderDetailConfig()
        {
            this.HasRequired(p => p.Product)
                       .WithMany(c => c.OrderDetails)
                       .HasForeignKey(p => p.ProductId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
            this.HasRequired(p => p.Order)
                       .WithMany(c => c.OrderDetails)
                       .HasForeignKey(p => p.OrderId)
                       .WillCascadeOnDelete(true); // or true for "on delete cascade"
        }
    }
}
