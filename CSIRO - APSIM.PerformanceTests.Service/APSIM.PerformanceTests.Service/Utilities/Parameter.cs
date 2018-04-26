using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace APSIM.PerformanceTests.Service
{
    public class Parameter
    {
        public string type;
        public string param;
        public object value;

        public Parameter(string type, string param, object value)
        {
            this.type = type;
            this.param = param;
            this.value = value;
        }
    }
}
