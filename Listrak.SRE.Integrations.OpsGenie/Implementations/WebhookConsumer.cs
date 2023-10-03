﻿using Confluent.Kafka;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{
    public class WebhookConsumer : IWebhookConsumer
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;
        private readonly ITeamsSendNotification TeamsThing;
        private readonly ILogger<WebhookConsumer> _logger;
        private readonly INotificationProcessor _notificationProcessor;

        public WebhookConsumer(IBotFrameworkHttpAdapter adapter, IBot bot, ITeamsSendNotification teamsThing, ILogger<WebhookConsumer> logger, INotificationProcessor notificationProcessor)
        {
            Adapter = adapter;
            Bot = bot;
            TeamsThing = teamsThing;
            _logger = logger;
            _notificationProcessor = notificationProcessor;
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
                            _logger.LogInformation("Sending to Notification Processor");
                            _notificationProcessor.ProcessNotification(cr.Message.Value);
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