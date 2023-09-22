using Confluent.Kafka;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{
    public class WebhookConsumer : IWebhookConsumer
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;
        private readonly ITeamsSendNotification TeamsThing;
        private readonly ILogger<WebhookConsumer> _logger;

        public WebhookConsumer(IBotFrameworkHttpAdapter adapter, IBot bot, ITeamsSendNotification teamsThing, ILogger<WebhookConsumer> logger)
        {
            Adapter = adapter;
            Bot = bot;
            TeamsThing = teamsThing;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "srekafka.servicebus.windows.net:9093",
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = "Endpoint=sb://srekafka.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Bg14XNWKIJJnqYflp8UogsOwVH7qy2eMl+AEhG1OsFU=",
                GroupId = "webhookConsumerGroup",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<string, string>(config).Build())
            {

                consumer.Subscribe("opsgeniewebhook");
                Console.WriteLine("Listening...");
                _logger.LogInformation("Listening...");
                System.Diagnostics.Trace.WriteLine("Listening to Kafka");
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var cr = consumer.Consume();
                            _logger.LogInformation($"Consumed message '{cr.Value}' from topic '{cr.Topic}, partition {cr.Partition}, at offset {cr.Offset}'");
                            System.Diagnostics.Trace.WriteLine($"[WebhookConsumer] Consumed message '{cr.Value}' from topic '{cr.Topic}, partition {cr.Partition}, at offset {cr.Offset}'");
                            _logger.LogInformation("[WebhookConsumer] Sending to teams...");
                            System.Diagnostics.Trace.WriteLine("[WebhookConsumer] Calling SendMessageAsync...");
                            TeamsThing.SendMessageAsync("https://smba.trafficmanager.net/amer/", "19:24d638f4c79941298611e751c92277c4@thread.tacv2", cr.Message.Value);
                            System.Diagnostics.Trace.WriteLine("Sent to teams");
                            _logger.LogInformation("[WebhookConsumer] SendMessageAsync Called");

                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"[WebhookConsumer] Error consuming from topic '{e.ConsumerRecord.Topic}': {e.Error.Reason}");
                            _logger.LogError($"[WebhookConsumer] Error consuming from topic '{e.ConsumerRecord.Topic}': {e.Error.Reason}");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"[WebhookConsumer] Error occured: {e.Error.Reason}");
                    System.Diagnostics.Trace.WriteLine($"[WebhookConsumer] Error occured: {e.Error.Reason}");
                    _logger.LogError($"[WebhookConsumer] Error occured: {e.Error.Reason}");
                    consumer.Close();
                }
                return Task.CompletedTask;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
