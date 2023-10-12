using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Implementations;
using Listrak.SRE.Integrations.OpsGenie.Models;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IOpsGenieHandler
    {
        string SendMessageAsync(string serviceUrl, string channelId, AlertData message);
        void ProcessNotification(string payload);
    }
}
