using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface INotificationProcessor
    {
        Task SendMessageAsync(string serviceUrl, string channelId, object message);
        Task ProcessNotification(string payload);
    }
}
