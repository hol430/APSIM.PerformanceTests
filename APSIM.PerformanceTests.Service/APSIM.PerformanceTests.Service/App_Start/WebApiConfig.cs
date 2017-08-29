using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace APSIM.PerformanceTests.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();


            config.Routes.MapHttpRoute(
                name: "UpdateAcceptStats",
                routeTemplate: "api/{controller}/{id}/{updateStatus}"
            );

            config.Routes.MapHttpRoute(
                name: "UpdatePullRequestMergeStatus",
                routeTemplate: "api/{controller}/{id}/{mergeStatus}"
            );

            config.Routes.MapHttpRoute(
                name: "Action",
                routeTemplate: "api/{controller}/{action}/{id}"
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
