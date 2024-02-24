using FashionShop.Models.Base;
using FashionShop.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace FashionShop.AreaModels
{
    public class CreateColorModel:BaseEntity
    {
        public int Index { get; set; }
        // colorId
        public string Color { get; set; }
        // list sizeId
        public string[] Size { get; set; }
    }
}
