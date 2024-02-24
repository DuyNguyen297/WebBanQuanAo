using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Product")]
    public class Product : BaseEntity
    {
        [DisplayName("Tên sản phẩm")]
        public string Name { get; set; }
 
        [DisplayName("Mã")]
        public string Code { get; set; }

        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }
        [DisplayName("Mô tả")]
        public string Description { get; set; }
        [DisplayName("Ảnh")]
        public string Image { get; set; }
        [DisplayName("Giá bán")]
        public decimal? Price { get; set; }
        [DisplayName("Giảm giá")]
        public decimal? Discount { get; set; }

		[DisplayName("Hiệu lực")]
		public bool Effective { get; set; } = true;

		[DisplayName("Nổi bật")]
        public bool Outstanding { get; set; } = true;

		[DisplayName("Màu")]
		public string ColorId { get; set; }
		[ForeignKey("ColorId")]
		public Color Color { get; set; }

		[DisplayName("Kích Thước")]
		public string SizeId { get; set; }
		[ForeignKey("SizeId")]
		public Size Size { get; set; }

		[DisplayName("Danh mục")]
        public string CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [DisplayName("Thương hiệu")]
        public string BranchId { get; set; }
        [ForeignKey("BranchId")]
        public Branch Branch { get; set; }


        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<ImportDetail> ImportDetails { get; set; }
        public ICollection<VoucherProduct> VoucherProducts { get; set; }
    }
}
