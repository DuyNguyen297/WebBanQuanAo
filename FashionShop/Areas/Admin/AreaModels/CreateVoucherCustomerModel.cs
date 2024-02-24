using FashionShop.Models.Base;
using FashionShop.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System;

namespace FashionShop.AreaModels
{
    public class CreateVoucherCustomerModel : BaseEntity
    {
		public string Name { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public decimal? TotalCondition { get; set; }
		public decimal? Discount { get; set; }
		public int? Quantity { get; set; }

		public string[] Customer { get; set; }

		public bool IsForAll { get; set; }
	}
}
