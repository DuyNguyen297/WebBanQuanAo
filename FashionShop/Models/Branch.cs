using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Branch")]
    public class Branch : BaseCodeEntity
    {
        [DisplayName("Thương hiệu")]
        public string Name { get; set; }
		[DisplayName("Nổi bật")]
		public bool Outstanding { get; set; } = true;
		[DisplayName("Hình ảnh")]
		public string Image { get; set; }

		[DisplayName("Mô tả")]
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
