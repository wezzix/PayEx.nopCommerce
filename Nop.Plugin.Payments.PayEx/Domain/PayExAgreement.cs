using System;
using Nop.Core;

namespace Nop.Plugin.Payments.PayEx.Domain
{
    public class PayExAgreement : BaseEntity
    {
        public virtual int CustomerId { get; set; }
        public virtual string PaymentMethodSystemName { get; set; }
        public virtual string AgreementRef { get; set; }
        public virtual string Name { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual DateTime? PaymentMethodExpireDate { get; set; }
        public virtual decimal MaxAmount { get; set; }
        public virtual int NumberOfUsages { get; set; }
        public virtual DateTime? LastUsedDate { get; set; }
        public virtual DateTime CreatedDate { get; set; }
    }
}