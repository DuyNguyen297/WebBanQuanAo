using FashionShop.Models.Base;
using FashionShop.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace FashionShop.AreaModels
{
    public class ViewColorSizeModel:BaseEntity
    {
        // color
        public Color Color { get; set; }
        // list size
        public List<Size> Size { get; set; }
    }
}
