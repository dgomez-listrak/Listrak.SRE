using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IOpsGenieAPI
    {
        Task AcknowledgeAlert(string alertId);
        Task UnacknowledgeAlert(string alertId);
        Task CloseAlert(string alertId);
    }
}
