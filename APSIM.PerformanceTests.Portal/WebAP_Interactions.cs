using APSIM.PerformanceTests.Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;


namespace APSIM.PerformanceTests.Portal
{
    public class WebAP_Interactions
    {
        //#if DEBUG
        //            httpClient.BaseAddress = new Uri("http://localhost:53187/");
        //#endif

        public static void RenamePredictedObservedTable(PORename objRename)
        {
            HttpClient httpClient = new HttpClient();

            string serviceUrl = ConfigurationManager.AppSettings["serviceAddress"].ToString() + "APSIM.PerformanceTests.Service/";
            httpClient.BaseAddress = new Uri(serviceUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = new HttpResponseMessage();
            response = httpClient.PostAsJsonAsync("api/PORename", objRename).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
            }
        }


        public static void UpdatePullRequestStats(string updateType, AcceptStatsLog apsimLog)
        {
            HttpClient httpClient = new HttpClient();

            string serviceUrl = ConfigurationManager.AppSettings["serviceAddress"].ToString() + "APSIM.PerformanceTests.Service/";
            httpClient.BaseAddress = new Uri(serviceUrl);

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = new HttpResponseMessage();
            if (updateType == "Accept")
            {
                response = httpClient.PostAsJsonAsync("api/acceptStats", apsimLog).Result;
            }
            else if (updateType == "Update")
            {
                response = httpClient.PostAsJsonAsync("api/updateStats", apsimLog).Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                }

                //This will check the status of the updates above, and notify Git
                response = httpClient.GetAsync("api/acceptstats/" + apsimLog.PullRequestId.ToString()).Result;
            }

            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
            }

        }

    }
}