using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Interfaces;

public interface ITeamsSendNotification
{
    Task SendMessageAsync(string serviceUrl, string channelId, object message);
}