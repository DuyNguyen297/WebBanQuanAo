using FashionShop.Models.Base;
using Irony.Parsing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
	[Table("District")]
	public class District : BaseCodeEntity
	{
		[DisplayName("Tên quận,huyện")]
		public string Name { get; set; }
		public string ProvinceId { get; set; }
		[ForeignKey("ProvinceId")]
		public Province Province { get; set; }
		public ICollection<Ward> Wards { get; set; }

	}
}
