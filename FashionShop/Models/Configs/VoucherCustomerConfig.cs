using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Models.Configs
{
    public class VoucherCustomerConfig : EntityTypeConfiguration<VoucherCustomer>
    {
        public VoucherCustomerConfig()
        {
			this.HasRequired(p => p.Customer)
		   .WithMany(c => c.VoucherCustomers)
		   .HasForeignKey(p => p.CustomerId)
		   .WillCascadeOnDelete(true); // or true for "on delete cascade"
		}
    }
}
