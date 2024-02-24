using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models.Base
{
	public class BaseEntity
    {
		[Key]
        [Column(Order = 1)]
        public string Id { get; set; }
		public DateTime? CreatedAt { get; set; } = DateTime.Now;
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public string UpdateUserId { get; set; }
		public string CreateUserId { get; set; }
	}
}
