using Duha.SIMS.ServiceModels.Enums;
using Newtonsoft.Json;

namespace Duha.SIMS.ServiceModels.Base
{
    public class ErrorData
    {
        [JsonProperty("errorType")]
        public ApiErrorTypeSM ApiErrorType { get; set; }

        [JsonProperty("displayMessage")]
        public string DisplayMessage { get; set; }

        [JsonProperty("additionalProps")]
        public Dictionary<string, object>? AdditionalProps { get; set; }

        public ErrorData()
        {
            AdditionalProps = new Dictionary<string, object>();
        }
    }
}