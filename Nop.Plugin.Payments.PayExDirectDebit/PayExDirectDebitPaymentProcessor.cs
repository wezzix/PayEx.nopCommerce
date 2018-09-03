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
        private readonly ILocalizationService _localizationService;

        public PayExDirectDebitPaymentProcessor(
            CurrencySettings currencySettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IPayExAgreementService payExAgreementService,
            IPaymentService paymentService,
            ISettingService settingService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWebHelper webHelper,
            IWorkContext workContext,
            PayExAgreementObjectContext payExAgreementObjectContext,
            PayExPaymentSettings payExPaymentSettings)
            : base(
                currencySettings,
                checkoutAttributeParser,
                currencyService,
                httpContextAccessor,
                localizationService,
                logger,
                orderService,
                payExAgreementService,
                paymentService,
                settingService,
                storeContext,
                taxService,
                webHelper,
                workContext,
                payExAgreementObjectContext,
                payExPaymentSettings)
        {
            _localizationService = localizationService;
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
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public override string GetPublicViewComponentName() => "PaymentPayExDirectDebit";

        public override bool SkipPaymentInfo => !RopcEnabled;

        #endregion

        #region BasePlugin Methods

        public override void Install()
        {
            _localizationService.AddOrUpdatePluginLocaleResource(
                "Plugins.FriendlyName.Payments.PayExDirectDebit", "Direct Debit");
            _localizationService.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayExDirectDebit.RedirectionTip",
                "You will be redirected to the PayEx site to complete the payment, once you click Confirm.");

            base.Install();
        }

        public override void Uninstall()
        {
            _localizationService.DeletePluginLocaleResource("Plugins.FriendlyName.Payments.PayExDirectDebit");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayExDirectDebit.RedirectionTip");

            base.Uninstall();
        }

        #endregion
    }
}