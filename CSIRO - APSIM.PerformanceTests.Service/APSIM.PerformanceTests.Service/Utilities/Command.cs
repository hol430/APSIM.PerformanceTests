using System.Collections.Generic;
using System.Data;


namespace APSIM.PerformanceTests.Service
{
    /// <summary>
    /// A serialisable version of SqlCommand
    /// </summary>
    public class Command
    {
        public string command;
        public string type;
        public Dictionary<string, string> parameters;
        public string spName;
        public string paramName;
        public string spData;

        public Command()
        {
            parameters = new Dictionary<string, string>();
        }
    }
}
