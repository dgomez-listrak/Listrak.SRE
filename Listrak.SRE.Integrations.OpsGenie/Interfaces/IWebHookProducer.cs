namespace Listrak.SRE.Integrations.OpsGenie.Interfaces
{
    public interface IWebHookProducer
    {
        Task Produce(string msg);
    }
}