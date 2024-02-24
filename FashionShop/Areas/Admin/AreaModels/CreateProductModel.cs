using FashionShop.Models.Base;
using FashionShop.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionShop.AreaModels
{
    public class CreateProductModel:BaseEntity
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
        public string Image { get; set; }

        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }

        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public string CategoryId { get; set; }
        public Category Category { get; set; }

        public string BranchId { get; set; }
        public Branch Branch { get; set; }

        public bool Effective { get; set; } = true;
        public bool Outstanding { get; set; } = false;
        public List<CreateColorModel> ColorSizes { get; set; }
    }
}
