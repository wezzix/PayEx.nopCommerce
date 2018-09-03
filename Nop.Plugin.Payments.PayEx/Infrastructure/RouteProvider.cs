using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.PayEx.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute(
                "Plugin.Payments.PayEx",
                "Plugins/PaymentPayEx/{action}/",
                new { controller = "PaymentPayEx" }
            );
        }

        public int Priority => -1;
    }
}