using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayEx;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.PayExDirectDebit.Controllers
{
    public class PaymentPayExDirectDebitController : Nop.Plugin.Payments.PayEx.Controllers.PaymentPayExController
    {
        public PaymentPayExDirectDebitController(ISettingService settingService, IPaymentService paymentService, IOrderService orderService, IOrderProcessingService orderProcessingService, ILogger logger, IWebHelper webHelper, IWorkContext workContext, PaymentSettings paymentSettings, PayExPaymentSettings payExPaymentSettings, IPayExAgreementService payExAgreementService)
            : base(settingService, paymentService, orderService, orderProcessingService, logger, webHelper, workContext, paymentSettings, payExPaymentSettings, payExAgreementService)
        {
            
        }

        #region NonAction Methods

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        #endregion

        #region Action Methods

        [AdminAuthorize]
        [ChildActionOnly]
        public override ActionResult Configure()
        {
            // We use the same configuration as the PayEx plugin.
            return RedirectToAction("Configure", "PaymentPayEx");
        }

        [ChildActionOnly]
        public override ActionResult PaymentInfo()
        {
            return View("Nop.Plugin.Payments.PayExDirectDebit.Views.PaymentPayExDirectDebit.PaymentInfo");
        }

        #endregion
    }
}