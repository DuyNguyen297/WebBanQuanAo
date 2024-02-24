using FashionShop.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FashionShop.Models
{
    [Table("OrderDetail")]
    public class OrderDetail : BaseEntity
    {

        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }


        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; } = 0;


        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [DisplayName("Đơn hàng")]
        public string OrderId { get; set; }

        // Đơn hàng
        [ForeignKey("OrderId")]
        public Order Order { set; get; }
    }
}
