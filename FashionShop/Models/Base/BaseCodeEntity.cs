using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models.Base
{
	public class BaseCodeEntity : BaseEntity
    {
        [Required]
        [DisplayName("Mã")]
        public string Code { get; set; }
	}
}
