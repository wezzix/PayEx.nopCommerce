using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PayExDirectDebit
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PayExDirectDebit",
                 "Plugins/PaymentPayExDirectDebit/{action}",
                 new { controller = "PaymentPayExDirectDebit" },
                 new[] { "Nop.Plugin.Payments.PayExDirectDebit.Controllers" }
            );
        }
        public int Priority { get { return 0; } }
    }
}
