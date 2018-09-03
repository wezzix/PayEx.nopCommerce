using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayEx;
using Nop.Plugin.Payments.PayEx.Controllers;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.PayExDirectDebit.Controllers
{
    public class PaymentPayExDirectDebitController : PaymentPayExController
    {
        public PaymentPayExDirectDebitController(
            ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            IPermissionService permissionService,
            IWebHelper webHelper,
            IWorkContext workContext,
            PaymentSettings paymentSettings,
            PayExPaymentSettings payExPaymentSettings,
            IPayExAgreementService payExAgreementService)
            : base(
                settingService, paymentService, orderService, orderProcessingService, logger, permissionService,
                webHelper, workContext, paymentSettings, payExPaymentSettings, payExAgreementService)
        {
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public override IActionResult Configure()
        {
            // We use the same configuration as the PayEx plugin.
            return RedirectToAction("Configure", "PaymentPayEx");
        }
    }
}