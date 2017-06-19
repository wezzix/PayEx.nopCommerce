using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PayEx
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PayEx",
                 "Plugins/PaymentPayEx/{action}",
                 new { controller = "PaymentPayEx" },
                 new[] { "Nop.Plugin.Payments.PayEx.Controllers" }
            );
        }
        public int Priority => 0;
    }
}
