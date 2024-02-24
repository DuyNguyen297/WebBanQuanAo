using FashionShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models
{
    public class ProvinceConfig : EntityTypeConfiguration<Province>
    {
        public ProvinceConfig()
        {
			this.HasMany<District>(x => x.Districts);
		}
    }
}
