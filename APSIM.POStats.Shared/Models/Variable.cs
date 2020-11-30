using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace APSIM.POStats.Shared.Models
{
    public class Variable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int N { get; set; }
        public double RMSE { get; set; }
        public double NSE { get; set; }
        public double RSR { get; set; }

        [JsonIgnore]
        public int TableId { get; set; }
        [JsonIgnore]
        public virtual Table Table { get; set; }

        public virtual List<VariableData> Data { get; set; }
    }
}
