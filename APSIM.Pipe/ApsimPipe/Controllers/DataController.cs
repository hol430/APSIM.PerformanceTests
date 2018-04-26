using ApsimPipe.Models;
using System.Web.Http;

namespace ApsimPipe.Controllers
{
    public class DataController : ApiController
    {
         // POST api/data
        [HttpPost]
        public string Post([FromBody] Command command)
        {
            string retStr = string.Empty;
            try
            {
                Utilities.WriteToLogFile("Testing.");
                retStr = SQL.RelayCommand(command);

            }
            catch (System.Exception ex)
            {
                Utilities.WriteToLogFile("ERROR: " + ex.Message.ToString());
            }
            return retStr;
        }
    }
}
