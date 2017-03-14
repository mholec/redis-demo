using System.Web.Mvc;
using System.Web.Routing;

namespace DemoRedisCache
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

	        routes.MapRoute(
		        name: "Connections",
		        url: "connections",
		        defaults: new {controller = "Test", action = "Connections", id = UrlParameter.Optional}
	        );

	        routes.MapRoute(
		        name: "Db",
		        url: "databases",
		        defaults: new {controller = "Test", action = "ConnectionsDb", id = UrlParameter.Optional}
	        );

			routes.MapRoute(
                name: "Default",
                url: "{action}/{id}",
                defaults: new { controller = "Demo", action = "Prehled", id = UrlParameter.Optional }
            );
        }
    }
}
