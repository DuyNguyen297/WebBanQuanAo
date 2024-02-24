using System.Data.Entity.ModelConfiguration;

namespace FashionShop.Models.Configs
{
    public class ContactConfig : EntityTypeConfiguration<Contact>
    {
        public ContactConfig()
        {
            this.Property(x => x.Name);
            this.Property(x => x.Message).HasColumnType("ntext");
            this.Property(x => x.Email);
            this.Property(x => x.Phone);
        }
    }
}