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
	[Table("VoucherCategory")]
	public class VoucherCategory : BaseEntity
	{
		[DisplayName("Tên khuyến mãi")]
		public string Name { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal? TotalCondition { get; set; }
		public decimal? Discount { get; set; }
		public int? Quantity { get; set; }
		public string CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		public Category Category { get; set; }
		[ForeignKey("CreateUserId")]
		public Account Account { set; get; }
	}
}
