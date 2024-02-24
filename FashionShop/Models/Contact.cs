using FashionShop.Models.Base;
using FashionShop.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace FashionShop.Models
{
    [Table("Contact")]
    public class Contact : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Topic { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
    }
}
