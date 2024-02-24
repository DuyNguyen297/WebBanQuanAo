using FashionShop.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("ImportDetail")]
    public class ImportDetail : BaseEntity
    {
        [DisplayName("Giá nhập")]
        public decimal? PriceIn { get; set; }


        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }


        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; }


        [DisplayName("Đơn nhập")]
        public string ImportId { get; set; }
        [ForeignKey("ImportId")]
        public Import Import { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
