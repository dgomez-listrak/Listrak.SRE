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
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{
    public class WebhookConsumer : IWebhookConsumer
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;
        private readonly ITeamsStartNewThreadInTeam TeamsThing;

        public WebhookConsumer(IBotFrameworkHttpAdapter adapter, IBot bot, ITeamsStartNewThreadInTeam teamsThing)
        {
            Adapter = adapter;
            Bot = bot;
            TeamsThing = teamsThing;
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
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var cr = consumer.Consume();
                            Console.WriteLine($"Consumed message '{cr.Value}' from topic '{cr.Topic}, partition {cr.Partition}, at offset {cr.Offset}'");

                            //TeamsThing.SendMessageAsync("","19:24d638f4c79941298611e751c92277c4@thread.tacv2",cr.Message.Value);


                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error consuming from topic '{e.ConsumerRecord.Topic}': {e.Error.Reason}");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occured: {e.Error.Reason}");
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

    public class TeamsStartNewThreadInTeam:ITeamsStartNewThreadInTeam
    {
        private readonly string _appId;
        private readonly string _appPassword;
        private readonly string _tenantId;
        public IBotFrameworkHttpAdapter Adapter { get; }

        public TeamsStartNewThreadInTeam(IConfiguration configuration, IBotFrameworkHttpAdapter adapter)
        {
            Adapter = adapter;
            _appId = configuration["MicrosoftAppId"];
            _appPassword = configuration["MicrosoftAppPassword"];
            _tenantId = configuration["MicrosoftAppTenantId"];
        }
        public async Task SendMessageAsync(string serviceUrl, string channelId, string message)
        {
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = message,
                ServiceUrl = serviceUrl,
                ChannelId = channelId,
                Conversation = new ConversationAccount(id: channelId)
            };

            await connectorClient.Conversations.SendToConversationAsync(activity);
        }

    }

    public interface ITeamsStartNewThreadInTeam
    {
        Task SendMessageAsync(string serviceUrl, string channelId, string message);
    }
}
