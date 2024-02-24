using FashionShop.Models.Base;
using FashionShop.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionShop.AreaModels
{
    public class ImportProductModel : BaseEntity
    {
        public string Name { get; set; }

        public string Code { get; set; }
		public Account Account { get; set; }
        public decimal? Total { get; set; }
        public List<Import> ColorSizes { get; set; }
    }
}
