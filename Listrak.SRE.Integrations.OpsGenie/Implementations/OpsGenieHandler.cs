////
///
/// So, on data coming from OG, they all should have the alert {} json object beneath "action":"Action" within {}.
/// API calls to get data, which we'll need to get the full alert data will be different, it will just be {data{}}
/// so we can then have a model for incoming OG alerts, even tohugh those after CREATE will lack some data
/// and those for API gets
///
/// 
using AdaptiveCards.Templating;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Confluent.Kafka;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations;

public class OpsGenieHandler : IOpsGenieHandler
{
    private readonly string _card = Path.Combine(".", "Resources", "AlertCard.json");
    private readonly ILogger<OpsGenieHandler> _logger;

    private readonly string _appPassword;
    private readonly string _appId;
    private readonly string _serviceUrl;
    private readonly string _channelId;
    private readonly IMySqlAdapter _mySqlAdapter;

    public OpsGenieHandler(ILogger<OpsGenieHandler> logger, IConfiguration configuration, IMySqlAdapter mySqlAdapter)
    {

        _logger = logger;
        _appId = configuration["MicrosoftAppId"];
        _appPassword = configuration["MicrosoftAppPassword"];
        _serviceUrl = configuration["ServiceUrl"];
        _channelId = configuration["ChannelId"];
        _mySqlAdapter = mySqlAdapter;
    }

    public async Task<string> SendMessageAsync(string serviceUrl, string channelId, AlertData message)
    {
        ResourceResponse result = new ResourceResponse();
        _logger.LogInformation("[TeamsNotifier]  SendMessageAsync to Teams Begin");
        System.Diagnostics.Trace.WriteLine("SendMessageAsync to Teams");
        try
        {
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);

            var cardAttachment = BuildNotificationCard(_card, message);

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                ServiceUrl = serviceUrl,
                ChannelId = channelId,
                Conversation = new ConversationAccount(id: channelId)
            };


            activity.Attachments = new List<Attachment>() { cardAttachment };

            result = await connectorClient.Conversations.SendToConversationAsync(activity);

            activity.Id = result.Id;
            ////activity.Attachments[0].Content = new JObject { ["text"] = "This is a test" };
            ////await connectorClient.Conversations.UpdateActivityAsync(activity);
            Console.WriteLine(result);
            _logger.LogInformation("[SendMessageAsync] Message sent...hopefully");

        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message} - {ex.InnerException} -{ex.StackTrace}");
        }

        return result.Id.ToString();
    }

    public async Task UpdateMessageAsync(string serviceUrl, string channelId, AlertData message, string conversationId)
    {
        ResourceResponse result = new ResourceResponse();
        _logger.LogInformation("[TeamsNotifier]  UpdateMessageAsync to Teams Begin");
        System.Diagnostics.Trace.WriteLine("UpdateMessageAsync to Teams");
        try
        {
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            var cardAttachment = BuildNotificationCard(_card, message);

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                ServiceUrl = serviceUrl,
                ChannelId = channelId,
                Conversation = new ConversationAccount(id: channelId)
            };
            activity.Attachments = new List<Attachment>() { cardAttachment };
            activity.Id = conversationId;
            await connectorClient.Conversations.UpdateActivityAsync(activity);
            Console.WriteLine(result);
            _logger.LogInformation("[UpdateMessageAsync] Message sent...hopefully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message} - {ex.InnerException} -{ex.StackTrace}");
        }
    }

    private Attachment BuildNotificationCard(string filePath, object myData)
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

    public async Task ProcessNotification(string jsonPayload)
    {
        try
        {
            var notification = JsonConvert.DeserializeObject<OpsGenieNotification>(jsonPayload);
            var payloadToSend = notification.Action != null ? notification.Alert : notification.Data;

            Console.WriteLine("Create payload from OG");

            // If action is 'create', we expect the payload in 'Alert'
            if (notification.Alert != null)
            {
                // Process using notification.Alert
                notification.Alert.Status = notification.Action?.ToLower() switch
                {
                    "create" => "New",
                    "close" => "Closed",
                    "addnote" => "Updated",
                    "acknowledge" => "Acknowledged",
                    _ => "Unknown"  // default case
                };

                payloadToSend = notification.Alert;

                var existingConversationId = _mySqlAdapter.GetConverationId(notification.Alert.UnifiedAlertId);
                if (existingConversationId != null)
                {
                    // Call UpdateMessageAsync if conversationId exists
                    await UpdateMessageAsync(_serviceUrl, _channelId, payloadToSend, existingConversationId);
                }
                else
                {
                    // Call SendMessageAsync if conversationId does not exist
                    var result = await SendMessageAsync(_serviceUrl, _channelId, payloadToSend);
                    payloadToSend.ConversationId = result;
                }

                await _mySqlAdapter.LogToMysql(notification);
            }
            else
            {
                _logger.LogError($"[WebhookConsumer] Expected 'Alert' payload for 'create' action. Payload: {jsonPayload}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[WebhookConsumer] Error occurred: {e.Message}");
            _logger.LogError($"[WebhookConsumer] Error occurred: {e.Message}");
            throw;
        }
    }

}