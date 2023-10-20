using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations;

public class OpsGenieHandler : IOpsGenieHandler
{
    private readonly ILogger<OpsGenieHandler> _logger;

    private readonly string _appPassword;
    private readonly string _appId;
    private readonly string _serviceUrl;
    private readonly string _channelId;
    private readonly IMySqlAdapter _mySqlAdapter;
    private readonly ICardBuilder _cardBuilder;
    private readonly IOpsGenieAPI _api;

    public OpsGenieHandler(ILogger<OpsGenieHandler> logger, IConfiguration configuration, IMySqlAdapter mySqlAdapter, ICardBuilder cardBuilder, IOpsGenieAPI api)
    {

        _logger = logger;
        _appId = configuration["MicrosoftAppId"];
        _appPassword = configuration["MicrosoftAppPassword"];
        _serviceUrl = configuration["ServiceUrl"];
        _channelId = configuration["ChannelId"];
        _mySqlAdapter = mySqlAdapter;
        _cardBuilder = cardBuilder;
        _api = api;
    }

    public string SendMessageAsync(string serviceUrl, string channelId, AlertData message)
    {
        ResourceResponse result = new ResourceResponse();
        _logger.LogInformation("[TeamsNotifier]  SendMessageAsync to Teams Begin");
        System.Diagnostics.Trace.WriteLine("SendMessageAsync to Teams");
        try
        {
            if (_mySqlAdapter.CheckExistingAlert(message.UnifiedAlertId))
                return string.Empty; // exists in the db already
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            var newCard = _cardBuilder.BuildCard(message);
            newCard = _cardBuilder.AddAckButton(newCard, message);
            newCard = _cardBuilder.AddCloseButton(newCard, message);
            newCard = _cardBuilder.AddNoteButton(newCard, message);
            newCard = _cardBuilder.AddIncidentButton(newCard, message);

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                ServiceUrl = serviceUrl,
                ChannelId = channelId,
                Conversation = new ConversationAccount(id: channelId)
            };

            activity.Attachments = new List<Attachment>() { newCard };

            result = connectorClient.Conversations.SendToConversationAsync(activity).Result;

            activity.Id = result.Id;
            Console.WriteLine(result);
            _logger.LogInformation("[SendMessageAsync] Message sent...hopefully");

        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message} - {ex.InnerException} -{ex.StackTrace}");
        }

        return result.Id.ToString();
    }

    public void UpdateMessageAsync(string serviceUrl, string channelId, AlertData message, string conversationId)
    {
        ResourceResponse result = new ResourceResponse();
        _logger.LogInformation("[TeamsNotifier]  UpdateMessageAsync to Teams Begin");
        System.Diagnostics.Trace.WriteLine("UpdateMessageAsync to Teams");
        try
        {
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            var card = _cardBuilder.BuildCard(message);
            _logger.LogError($"Update Request Status - {message.Status}");

            // So for add note and probably closed status comes through as updated, we'll need
            // to see if it's been acked or not. Unsure if the API call will provide that
            // we'll need to make sure to update mysql as well, thinking of some kind of cron job to regularly check
            // ticket statuses and update accordingly if they're not

            var statusDetails = _api.GetAlertStatus(message.UnifiedAlertId);
            if (statusDetails.Data.Status.ToLower() != "closed")
            {
                card = statusDetails.Data.Acknowledged
                    ? _cardBuilder.AddUnAckButton(card, message)
                    : _cardBuilder.AddAckButton(card, message);

                card = _cardBuilder.AddNoteButton(card, message);
                card = _cardBuilder.AddCloseButton(card, message);
                card = _cardBuilder.AddIncidentButton(card, message);
            }
            else
            {
                card = _cardBuilder.AddNoteButton(card, message);
                card = _cardBuilder.AddIncidentButton(card, message);
            }

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                ServiceUrl = serviceUrl,
                ChannelId = channelId,
                Conversation = new ConversationAccount(id: channelId)
            };
            activity.Attachments = new List<Attachment>() { card };
            activity.Id = conversationId;
            connectorClient.Conversations.UpdateActivityAsync(activity);
            Console.WriteLine(result);
            _logger.LogInformation("[UpdateMessageAsync] Message sent...hopefully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message} - {ex.InnerException} -{ex.StackTrace}");
        }
    }


    public void ProcessNotification(string jsonPayload)
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
                    _ => "New"  // default case
                };

                payloadToSend = notification.Alert;

                var existingConversationId = _mySqlAdapter.GetConverationId(notification.Alert.UnifiedAlertId);
                if (existingConversationId != null)
                {
                    // Call UpdateMessageAsync if conversationId exists
                    UpdateMessageAsync(_serviceUrl, _channelId, payloadToSend, existingConversationId);
                }
                else
                {
                    // Call SendMessageAsync if conversationId does not exist
                    var result = SendMessageAsync(_serviceUrl, _channelId, payloadToSend);
                    if (!string.IsNullOrEmpty(result))
                    {
                        payloadToSend.ConversationId = result;
                    }
                }
                _mySqlAdapter.LogToMysql(notification);
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