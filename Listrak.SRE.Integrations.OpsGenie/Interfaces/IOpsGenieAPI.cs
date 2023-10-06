using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IOpsGenieAPI
    {
        Task AcknowledgeAlert(string alertId, string replyToId = null);
    }
}
