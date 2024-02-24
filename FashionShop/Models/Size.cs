using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Size")]
    public class Size : BaseCodeEntity
    {
        [DisplayName("Kích thước")]
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
