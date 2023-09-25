using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface INotificationProcessor
    {
        Task ProcessNotification(string payload);
    }
}
