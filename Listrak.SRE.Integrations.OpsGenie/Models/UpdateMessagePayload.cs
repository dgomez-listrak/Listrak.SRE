using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Newtonsoft.Json.Linq;

namespace Listrak.SRE.Integrations.OpsGenie.Models
{
    public class DataMessagePayload : IMessagePayload
    {
        public object CreateMessagePayload(JObject jsonObject)
        {
            return new
            {
                Title = $"{jsonObject["data"]["message"]}",
                Description = $"{jsonObject["data"]["description"]}",
                AlertId = $"{jsonObject["data"]["id"]}",
                Priority = $"{jsonObject["data"]["priority"]}",
                Status = $"{jsonObject["data"]["status"]}",
                Source = $"{jsonObject["data"]["source"]}"
            };
        }
    }
}
