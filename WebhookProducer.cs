using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;

namespace Listrak.SRE.Integrations.OpsGenie
{
    public class WebhookProducer : IWebHookProducer
    {
        public async Task Produce(string msg)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "srekafka.servicebus.windows.net:9093",
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = "Endpoint=sb://srekafka.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Bg14XNWKIJJnqYflp8UogsOwVH7qy2eMl+AEhG1OsFU="
            };
            var jsonmsg = Newtonsoft.Json.JsonConvert.DeserializeObject<WebhookPayload>(msg);

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                var message = new Message<string, string>
                {
                    Key = jsonmsg.Alert.AlertId,
                    Value = msg
                };

                var result = producer.ProduceAsync("opsgeniewebhook", message).Result;
                producer.Flush(TimeSpan.FromSeconds(10));
                Console.WriteLine($"Message is sent to Partition: {result.Partition}, Offset: {result.Offset}");

            }

        }
    }
}