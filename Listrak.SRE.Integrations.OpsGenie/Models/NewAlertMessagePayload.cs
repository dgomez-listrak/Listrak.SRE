using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Newtonsoft.Json.Linq;

namespace Listrak.SRE.Integrations.OpsGenie.Models
{
    public class NewAlertMessagePayload : IMessagePayload
    {
        public object CreateMessagePayload(JObject jsonObject)
        {
            return new
            {
                Title = $"{jsonObject["alert"]["message"]}",
                Description = $"{jsonObject["alert"]["description"]}",
                AlertId = $"{jsonObject["alert"]["alertId"]}",
                Priority = $"{jsonObject["alert"]["priority"]}",
                Status = string.Empty,
                Source = $"{jsonObject["alert"]["source"]}"
            };
        }
    }
}
