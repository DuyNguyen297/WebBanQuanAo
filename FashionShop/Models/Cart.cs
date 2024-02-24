using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Web;
using FashionShop.Models.Base;
using Irony.Parsing;

namespace FashionShop.Models
{
    public class Cart:BaseEntity
    {
        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        // Sản phẩm
        [ForeignKey("ProductId")]
        public Product Product { set; get; }
        // Người đặt hàng
        public string SessionId { get; set; }
    }
}