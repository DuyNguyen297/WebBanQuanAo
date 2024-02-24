using FashionShop.Models;
using FashionShop.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace FashionShop.Models
{
    [Table("Banner")]
    public class Banner : BaseEntity
    {
		public string Name { get; set; }
		public string Image { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public int? SeqNum { get; set; }
		public bool IsActive { get; set; } = true;

		[ForeignKey("CreateUserId")]
		public Account Account { set; get; }
	}
}