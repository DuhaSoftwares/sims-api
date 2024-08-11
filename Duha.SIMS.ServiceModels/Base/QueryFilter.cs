using Newtonsoft.Json;

namespace Duha.SIMS.ServiceModels.Base
{
    public class QueryFilter
    {
        [JsonProperty("$skip")]
        public int? Skip { get; set; }

        [JsonProperty("$top")]
        public int? Top { get; set; }

        public QueryFilter()
        {
            Skip = -1;
            Top = -1;
        }
    }
}