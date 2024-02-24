using System.Data.Entity.ModelConfiguration;
using FashionShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Models.Configs
{
    public class VoucherShipConfig : EntityTypeConfiguration<VoucherShip>
    {
        public VoucherShipConfig()
        {

		}
    }
}
