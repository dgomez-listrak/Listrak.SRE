using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using AdaptiveCards.Templating;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations;

public class NotificationProcessor : INotificationProcessor
{
    private readonly IBotFrameworkHttpAdapter Adapter;
    private readonly IBot Bot;
    
    private readonly ILogger<NotificationProcessor> _logger;


    private readonly string _appId;
    private readonly string _appPassword;
    

    private readonly string _card = Path.Combine(".", "Resources", "AlertCard.json");

    public NotificationProcessor(IBotFrameworkHttpAdapter adapter, IBot bot, ILogger<NotificationProcessor> logger, IConfiguration configuration)
    {
        Adapter = adapter;
        Bot = bot;
        _logger = logger;
        _appId = configuration["MicrosoftAppId"];
        _appPassword = configuration["MicrosoftAppPassword"];
    }

    public async Task SendMessageAsync(string serviceUrl, string channelId, object message)
    {
        _logger.LogInformation("[TeamsNotifier]  SendMessageAsync to Teams Begin");
        System.Diagnostics.Trace.WriteLine("SendMessageAsync to Teams");
        try
        {
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);

            var cardAttachment = CreateAdaptiveCardAttachment(_card, message);

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                ServiceUrl = serviceUrl,
                ChannelId = channelId,
                Conversation = new ConversationAccount(id: channelId),
                ReplyToId = "thisIsAReplyToId"
            };
            activity.Attachments = new List<Attachment>() { cardAttachment };

            var result = await connectorClient.Conversations.SendToConversationAsync(activity);
            Console.WriteLine(result);
            _logger.LogInformation("[SendMessageAsync] Message sent...hopefully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message} - {ex.InnerException} -{ex.StackTrace}");
        }
    }

    private Attachment CreateAdaptiveCardAttachment(string filePath, object myData)
    {
        var adaptiveCardJson = File.ReadAllText(filePath);
        var template = new AdaptiveCardTemplate(adaptiveCardJson);
        string cardJson = template.Expand(myData);

        var adaptiveCardAttachment = new Attachment()
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(cardJson)
        };
        return adaptiveCardAttachment;
    }


    public Task ProcessNotification(string jsonPayload)
    {
        try
        {
            // Parse JSON into a JObject
            JObject jsonObject = JObject.Parse(jsonPayload);

            // Check the "action" value
            string actionValue = jsonObject["action"]?.ToString();

            // Filter based on the "action" value
            switch (actionValue)
            {
                case "Acknowledge":
                    Console.WriteLine("Handling Acknowledge action.");
                    // Handle Acknowledge action
                    break;

                case "Create":
                    Console.WriteLine("Handling Create.");

                    WebhookPayload newAlert = JsonConvert.DeserializeObject<WebhookPayload>(jsonPayload);

                    var message = new
                    {
                        Title = $"{newAlert.Alert.Message}",
                        Description = $"{newAlert.Alert.Description}",
                        AlertId = $"{newAlert.Alert.AlertId}",
                        Priority = $"{newAlert.Alert.Priority}",
                        Status = $"{string.Empty}",
                        Source = $"{newAlert.Alert.Source}",
                    };

                    _logger.LogInformation("[WebhookConsumer] Sending to teams...");
                    var result = SendMessageAsync("https://smba.trafficmanager.net/amer/","19:24d638f4c79941298611e751c92277c4@thread.tacv2", message);
                    Console.WriteLine(result);
                    _logger.LogInformation("[WebhookConsumer] SendMessageAsync Called");
                    // Handle AnotherAction
                    break;

                default:
                    Console.WriteLine("Action not recognized.");
                    // Handle unrecognized actions
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[WebhookConsumer] Error occured: {e.Message}");
            _logger.LogError($"[WebhookConsumer] Error occured: {e.Message}");
            return Task.FromException(e);
        }

        return Task.CompletedTask;
    }
}