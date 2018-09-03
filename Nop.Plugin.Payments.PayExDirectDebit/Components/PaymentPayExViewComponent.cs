using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayExDirectDebit.Components
{
    [ViewComponent(Name = "PaymentPayExDirectDebit")]
    public class PaymentPayExViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayExDirectDebit/Views/PaymentInfo.cshtml");
        }
    }
}