using System.Text.RegularExpressions;
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
using SD.Payex2.Entities;

namespace Nop.Plugin.Payments.PayExSwish
{
    /// <summary>
    /// PayEx Swish payment processor
    /// </summary>
    public class PayExSwishPaymentProcessor : PayExPaymentProcessor
    {
        public PayExSwishPaymentProcessor(
            PayExPaymentSettings payExPaymentSettings, PayExAgreementObjectContext payExAgreementObjectContext,
            IPayExAgreementService payExAgreementService, ISettingService settingService,
            ICurrencyService currencyService, CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
            IHttpContextAccessor httpContextAccessor, ILogger logger, IOrderService orderService,
            ILocalizationService localizationService, IStoreContext storeContext, IWorkContext workContext)
            : base(
                payExPaymentSettings, payExAgreementObjectContext, payExAgreementService, settingService,
                currencyService, currencySettings, webHelper, checkoutAttributeParser, taxService, httpContextAccessor,
                logger, orderService, localizationService, storeContext, workContext)
        {
        }

        /// <summary>
        /// This determines the default payment method shown in the PayEx payment gateway.
        /// It can be overridden to show a different payment method.
        /// </summary>
        protected override string PaymentView => "SWISH";

        #region IPaymentMethod Methods

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        public override void GetPublicViewComponent(out string viewComponentName) =>
            viewComponentName = "PaymentPayExSwish";

        public override bool SkipPaymentInfo => true;

        protected override void BeforeInitialize(
            PostProcessPaymentRequest postProcessPaymentRequest, InitializeRequest request)
        {
            // In m-commerce flow, the customer may want to use a different number other than that on the order.
            // Since we don't know which flow, it's safer to not specify however less convenient for desktop users.
            // If the number is not Swish enabled, we immediately get ThirdPartyError=Payer not Enrolled.
            // Additionally, the number must contain country code or else we get "Invalid parameter:MSISDN" in Initialize.
            //request.MobilePhoneNumber = postProcessPaymentRequest.Order.BillingAddress?.FaxNumber

            // Description Can be 160 characters long, except when the payment method is iDEAL or Swish, then the limit is 35 characters (iDEAL) 
            // and 50 characters (Swish). For Swish, allowed characters are restricted to [a-öA-Ö0-9:;.,?!()”].
            request.Description = Regex.Replace(request.Description, @"[^a-öA-Ö0-9:;\.,\?\!\(\)"" ]", "");
            if (request.Description.Length > 50)
                request.Description = request.Description.PadLeft(50);
        }

        #endregion

        #region BasePlugin Methods

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Plugins.FriendlyName.Payments.PayExSwish", "Swish");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayExSwish.RedirectionTip",
                "You will be redirected to the PayEx site to complete the payment, once you click Confirm.");

            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugins.FriendlyName.Payments.PayExSwish");
            this.DeletePluginLocaleResource("Plugins.Payments.PayExSwish.RedirectionTip");

            base.Uninstall();
        }

        #endregion
    }
}