using Listrak.SRE.Integrations.OpsGenie.Models;
using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IOpsGenieAPI
    {
        void AcknowledgeAlert(string alertId);
        void UnacknowledgeAlert(string alertId);
        void CloseAlert(string alertId);
        OpsGenieStatus GetAlertStatus(string alertId);
    }
}
