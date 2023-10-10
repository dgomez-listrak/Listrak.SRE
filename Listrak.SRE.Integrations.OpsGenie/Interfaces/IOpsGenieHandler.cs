using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Implementations;
using Listrak.SRE.Integrations.OpsGenie.Models;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IOpsGenieHandler
    {
        Task<string> SendMessageAsync(string serviceUrl, string channelId, AlertData message);
        Task ProcessNotification(string payload);
    }
}
