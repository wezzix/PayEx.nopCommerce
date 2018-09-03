using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.PayEx.Domain;

namespace Nop.Plugin.Payments.PayEx.Data
{
    public class PayExAgreementMap : NopEntityTypeConfiguration<PayExAgreement>
    {
        public override void Configure(EntityTypeBuilder<PayExAgreement> builder)
        {
            builder.ToTable(nameof(PayExAgreement));

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CustomerId);
            builder.Property(m => m.PaymentMethodSystemName).HasMaxLength(50);
            builder.Property(m => m.AgreementRef).HasMaxLength(50);
            builder.Property(m => m.Name).HasMaxLength(50);
            builder.Property(m => m.PaymentMethod).HasMaxLength(20);
            builder.Property(m => m.MaxAmount);
            builder.Property(m => m.CreatedDate);
        }
    }
}