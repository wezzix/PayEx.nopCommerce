using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payments.PayEx;
using Nop.Plugin.Payments.PayEx.Data;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.PayExDirectDebit
{
    /// <summary>
    /// PayEx Direct Debit payment processor
    /// </summary>
    public class PayExDirectDebitPaymentProcessor : PayExPaymentProcessor
    {
        public PayExDirectDebitPaymentProcessor(
            PayExPaymentSettings payExPaymentSettings, PayExAgreementObjectContext payExAgreementObjectContext,
            IPayExAgreementService payExAgreementService, ISettingService settingService,
            ICurrencyService currencyService, CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger, IOrderService orderService, ILocalizationService localizationService,
            IStoreContext storeContext, IWorkContext workContext)
            : base(
                payExPaymentSettings, payExAgreementObjectContext, payExAgreementService, settingService,
                currencyService, currencySettings, webHelper, checkoutAttributeParser, taxService, httpContextAccessor,
                logger,
                orderService, localizationService, storeContext, workContext)
        {
        }

        /// <summary>
        /// This determines the default payment method shown in the PayEx payment gateway.
        /// It can be overridden to show a different payment method.
        /// </summary>
        protected override string PaymentView => "DIRECTDEBIT";

        #region IPaymentMethod Methods

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public override IList<string> ValidatePaymentForm(IFormCollection form) =>
            new List<string>();

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public override ProcessPaymentRequest GetPaymentInfo(IFormCollection form) =>
            new ProcessPaymentRequest();

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        public override void GetPublicViewComponent(out string viewComponentName) =>
            viewComponentName = "PaymentPayExDirectDebit";

        public override bool SkipPaymentInfo => !RopcEnabled;

        #endregion

        #region BasePlugin Methods

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Plugins.FriendlyName.Payments.PayExDirectDebit", "Direct Debit");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayExDirectDebit.RedirectionTip",
                "You will be redirected to the PayEx site to complete the payment, once you click Confirm.");

            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugins.FriendlyName.Payments.PayExDirectDebit");
            this.DeletePluginLocaleResource("Plugins.Payments.PayExDirectDebit.RedirectionTip");

            base.Uninstall();
        }

        #endregion
    }
}