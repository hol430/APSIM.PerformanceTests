using System.Collections.Generic;


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

        public Command()
        {
            parameters = new Dictionary<string, string>();
        }
    }
}
