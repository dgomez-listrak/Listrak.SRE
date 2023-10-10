using Listrak.SRE.Integrations.OpsGenie.Models;
using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IMySqlAdapter
    {
        Task LogToMysql(OpsGenieNotification message);
        string GetConverationId(string alertUnifiedAlertId);
    }
}
