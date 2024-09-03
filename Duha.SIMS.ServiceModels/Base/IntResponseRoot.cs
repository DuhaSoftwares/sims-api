using Newtonsoft.Json;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace Duha.SIMS.ServiceModels.Base
{
    public class IntResponseRoot
    {
        public int IntResponse { get; set; }
        public string? Message { get; set; }
        public IntResponseRoot(int intResponse, string message = "" ) 
        { 
            IntResponse = intResponse;
            Message = message;
        }
    }
}
