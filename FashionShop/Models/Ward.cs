using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Ward")]
    public class Ward : BaseEntity
    {
		[DisplayName("Mã")]
		public string Code { get; set; }
		[DisplayName("Tên phường,xã")]
        public string Name { get; set; }
		public string DistrictId { get; set; }
		[ForeignKey("DistrictId")]
		public District District { get; set; }
	}
}
