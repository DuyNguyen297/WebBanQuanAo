using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
	[Table("VoucherCustomer")]
	public class VoucherCustomer : BaseEntity
	{
		[DisplayName("Tên khuyến mãi")]
		public string Name { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal? TotalCondition { get; set; }
		public decimal? Discount { get; set; }
		public int? Quantity { get; set; }
		public string CustomerId { get; set; }
		[ForeignKey("CustomerId")]
		public Customer Customer { get; set; }
		[ForeignKey("CreateUserId")]
		public Account Account { set; get; }

	}
}
