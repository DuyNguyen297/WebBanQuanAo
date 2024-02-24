using FashionShop.Models;
using FashionShop.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace FashionShop.Models
{
    [Table("Feedback")]
    public class Feedback : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? Rate { get; set; }
        public string Message { get; set; }
        public string ProductId { get; set; }
        // Sản phẩm
        [ForeignKey("ProductId")]
        public Product Product { set; get; }
        [ForeignKey("CreateUserId")]
        public Customer Customer { set; get; }
    }
}