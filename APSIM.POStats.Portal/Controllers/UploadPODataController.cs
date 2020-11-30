using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APSIM.POStats.Portal.Data;
using APSIM.POStats.Shared;
using APSIM.POStats.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace APSIM.POStats.Portal.Controllers
{
    [Route("api/uploadpodata")]
    [ApiController]
    public class UploadPODataController : ControllerBase
    {
        /// <summary>The database context.</summary>
        private readonly StatsDbContext statsDb;

        /// <summary>Constructor.</summary>
        /// <param name="stats">The database context.</param>
        public UploadPODataController(StatsDbContext stats)
        {
            statsDb = stats;
        }

        /// <summary>Invoked by collector to upload a pull request.</summary>
        /// <param name="pullRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public string Post([FromBody]PullRequest pullRequest)
        {
            try
            {
                statsDb.PullRequests.Add(pullRequest);
                statsDb.SaveChanges();
            }
            catch (Exception err)
            {
                return $"Error from POStats web api: {err.ToString()}";
            }
            return null;
        }
    }
}