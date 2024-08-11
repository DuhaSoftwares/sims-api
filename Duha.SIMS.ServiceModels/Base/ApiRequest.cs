using Newtonsoft.Json;

namespace Duha.SIMS.ServiceModels.Base
{
    public class ApiRequest<T> where T : class
    {
        [JsonProperty("reqData")]
        public T ReqData { get; set; }

        [JsonProperty("queryFilter")]
        public QueryFilter? QueryFilter { get; set; }

        public ApiRequest()
        {
            QueryFilter = null;
        }
    }
}