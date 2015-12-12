using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ELibrary
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			//routes.RouteExistingFiles = true;
			routes.MapMvcAttributeRoutes();

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

			routes.MapRoute("Home", "Home", new { controller = "Home", action = "Index" });

			routes.MapRoute("Short", "{action}", new { controller = "Home" });

			////routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute("Admin", "Admin", new { controller = "Admin", action = "Index" });
		}
	}
}
