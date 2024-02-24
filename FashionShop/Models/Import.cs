
using FashionShop.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FashionShop.Models
{
    [Table("Import")]
    public class Import : BaseCodeEntity
    {
        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; }

        // người xử lí nhập hàng
        [ForeignKey("CreateUserId")]
        public Account Account { set; get; }

        public ICollection<ImportDetail> ImportDetails { get; set; }
    }
}
