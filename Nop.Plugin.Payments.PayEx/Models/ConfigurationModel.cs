using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.PayEx.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.AccountNumber")]
        public int AccountNumber { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.EncryptionKey")]
        public string EncryptionKey { get; set; }

        public int TransactionModeId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.TransactionModeValues")]
        public SelectList TransactionModeValues { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.UseTestEnvironment")]
        public bool UseTestEnvironment { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.ValidateOrderTotal")]
        public bool ValidateOrderTotal { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.PassProductNamesAndTotals")]
        public bool PassProductNamesAndTotals { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.AllowCreateAgreement")]
        public bool AllowCreateAgreement { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEx.Fields.AgreementMaxAmount")]
        public decimal AgreementMaxAmount { get; set; }

        public string TransactionCallbackUrl { get; set; }

        public string Message { get; set; }
    }
}