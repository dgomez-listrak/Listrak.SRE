using Newtonsoft.Json.Linq;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IMessagePayload
    {
        object CreateMessagePayload(JObject jsonObject);
    }

}

