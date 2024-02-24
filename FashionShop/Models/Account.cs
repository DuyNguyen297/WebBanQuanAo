using FashionShop.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Account")]
    public class Account : BaseEntity
    {
        [DisplayName("Tên người dùng")]
        public string Name { get; set; }
        [DisplayName("Tên đăng nhập")]
        public string UserName { get; set; }
		[DisplayName("Hình đại diện")]
		public string Avatar { get; set; }
		[DisplayName("Email")]
        public string Email { get; set; }
        [DisplayName("Số điện thoại")]
        public string Phone { get; set; }

        [DisplayName("Mật khẩu")]
        public string Password { get; set; }

        [DisplayName("Vai trò")]
        public string Role { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Import> Imports { get; set; }
        public ICollection<VoucherShip> VoucherShips { get; set; }
        public ICollection<VoucherProduct> VoucherProducts { get; set; }
        public ICollection<VoucherCategory> VoucherCategories { get; set; }
        public ICollection<VoucherCustomer> VoucherCustomer { get; set; }
    }
}
