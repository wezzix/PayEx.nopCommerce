using System.Data.Entity.ModelConfiguration;
using Nop.Plugin.Payments.PayEx.Domain;

namespace Nop.Plugin.Payments.PayEx.Data
{
    public class PayExAgreementMap : EntityTypeConfiguration<PayExAgreement>
    {
        public PayExAgreementMap()
        {
            ToTable("PayExAgreement");

            // Map the primary key
            HasKey(m => m.Id);

            // Map the additional properties
            Property(m => m.CustomerId);
            Property(m => m.PaymentMethodSystemName).HasMaxLength(50);
            Property(m => m.AgreementRef).HasMaxLength(50);
            Property(m => m.Name).HasMaxLength(50);
            Property(m => m.PaymentMethod).HasMaxLength(20);
            Property(m => m.MaxAmount);
            Property(m => m.CreatedDate);
        }
    }
}
