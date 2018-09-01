using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PayExSwish
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PayExSwish",
                 "Plugins/PaymentPayExSwish/{action}",
                 new { controller = "PaymentPayExSwish" },
                 new[] { "Nop.Plugin.Payments.PayExSwish.Controllers" }
            );
        }
        public int Priority => 0;
    }
}
