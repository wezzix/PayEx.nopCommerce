using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayEx
{
    public class PayExPaymentSettings : ISettings
    {
        public int AccountNumber { get; set; }
        public string EncryptionKey { get; set; }
        public TransactionMode TransactionMode { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool UseTestEnvironment { get; set; }
        public bool ValidateOrderTotal { get; set; }
        public bool PassProductNamesAndTotals { get; set; }
        public bool AllowCreateAgreement { get; set; }
        public decimal AgreementMaxAmount { get; set; }
        public string Email { get; set; }
        public string RegistrationKey { get; set; }
    }
}
