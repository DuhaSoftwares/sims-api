using Newtonsoft.Json;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace Duha.SIMS.ServiceModels.Base
{
    public class BoolResponseRoot
    {
        public bool BoolResponse { get; set; }

        public string ResponseMessage { get; set; }

        public BoolResponseRoot(bool boolValue, string responseMessage = "")
        {
            BoolResponse = boolValue;
            ResponseMessage = responseMessage;
        }
    }
}
