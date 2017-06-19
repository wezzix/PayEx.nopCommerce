using System.Web;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payments.PayEx;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Plugin.Payments.PayEx.Data;
using System.Web.Routing;

namespace Nop.Plugin.Payments.PayExDirectDebit
{
    /// <summary>
    /// PayEx Direct Debit payment processor
    /// </summary>
    public class PayExDirectDebitPaymentProcessor : Nop.Plugin.Payments.PayEx.PayExPaymentProcessor
    {
        public PayExDirectDebitPaymentProcessor(PayExPaymentSettings payExPaymentSettings, PayExAgreementObjectContext payExAgreementObjectContext, ISettingService settingService, ICurrencyService currencyService, CurrencySettings currencySettings, IWebHelper webHelper, ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService, HttpContextBase httpContext, ILogger logger, IOrderService orderService, ILocalizationService localizationService, IStoreContext storeContext)
            : base(payExPaymentSettings, payExAgreementObjectContext, settingService, currencyService, currencySettings, webHelper, checkoutAttributeParser, taxService, httpContext, logger, orderService, localizationService, storeContext)
        {
            
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPayExDirectDebit";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayExDirectDebit.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// This determines the default payment method shown in the PayEx payment gateway.
        /// It can be overridden to show a different payment method.
        /// </summary>
        protected override string PaymentView { get { return "DIRECTDEBIT"; } }

        #region BasePlugin Methods

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Plugins.FriendlyName.Payments.PayExDirectDebit", "Direct Debit");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayExDirectDebit.RedirectionTip", "You will be redirected to the PayEx site to complete the payment, once you click Confirm.");

            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugins.FriendlyName.Payments.PayExDirectDebit");
            this.DeletePluginLocaleResource("Plugins.Payments.PayExDirectDebit.RedirectionTip");

            base.Uninstall();
        }

        #endregion

        public override bool SkipPaymentInfo
        {
            get { return true; }
        }
    }
}
