using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayExSwish.Components
{
    [ViewComponent(Name = "PaymentPayExSwish")]
    public class PaymentPayExViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayExSwish/Views/PaymentInfo.cshtml");
        }
    }
}