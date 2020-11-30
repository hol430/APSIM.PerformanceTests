using System.Text.Json.Serialization;

namespace APSIM.POStats.Shared.Models
{
    public class VariableData
    {
        public int Id { get; set; }
        public string Label { get; set; }

        public double Predicted { get; set; }
        public double Observed { get; set; }
        
        [JsonIgnore]
        public int VariableId { get; set; }

        [JsonIgnore]
        public virtual Variable Variable { get; set; }
    }
}
