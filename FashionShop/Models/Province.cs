using FashionShop.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Province")]
    public class Province : BaseCodeEntity
    {
        [DisplayName("Tên tỉnh")]
        public string Name { get; set; }
        public ICollection<District> Districts { get; set; }
    }
}
