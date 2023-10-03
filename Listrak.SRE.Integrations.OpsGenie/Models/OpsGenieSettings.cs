using Microsoft.Extensions.Configuration;

namespace Listrak.SRE.Integrations.OpsGenie.Models
{
    public class OpsGenieSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } 
    }

}
