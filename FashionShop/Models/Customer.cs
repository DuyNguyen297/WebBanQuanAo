using FashionShop.Models;
using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Customer")]
    public class Customer : BaseEntity
    {
        [DisplayName("Tên khách hàng")]
        public string Name { get; set; }
        [DisplayName("Email")]
        public string Email { get; set;}
        [DisplayName("Số điện thoại")]
        public string Phone { get; set; }
		[DisplayName("Hình đại diện")]
		public string Avatar { get; set; }
		[DisplayName("Địa chỉ")]
        public string Address { get; set; }
        [DisplayName("Mật khẩu")]
        public string Password { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<VoucherCustomer> VoucherCustomers { get; set; }
    }
}
