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

            //config.Routes.MapHttpRoute(
            //    name: "UpdateReleaseStatus",
            //    routeTemplate: "api/{controller}/{id:isReleased}"
            //    //defaults: new { id = RouteParameter.Optional }
            //);
            //config.Routes.MapHttpRoute(
            //    name: "PassedTestsStatus",
            //    routeTemplate: "{controller}/{id}"
            //);

            //config.Routes.MapHttpRoute(
            //    name: "IsReleased",
            //    routeTemplate: "{controller}/{action}/{id}"
            //);


            config.Routes.MapHttpRoute(
                name: "UpdatePullRequest",
                routeTemplate: "api/{controller}/{id}/{releaseStatus}"
            );

            config.Routes.MapHttpRoute(
                name: "Action",
                routeTemplate: "api/{controller}/{action}/{id}"
            );

            //config.Routes.MapHttpRoute(
            //    name: "Action",
            //    routeTemplate: "api/{controller}/{action}/{pullRequestId}"
            //);

            //config.Routes.MapHttpRoute(
            //    name: "Action",
            //    routeTemplate: "api/{controller}/{action}/{pullRequestId}/{releaseStatus}"
            //);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
