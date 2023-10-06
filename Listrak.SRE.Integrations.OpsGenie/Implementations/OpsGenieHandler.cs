using AdaptiveCards.Templating;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
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

namespace Listrak.SRE.Integrations.OpsGenie.Implementations;

public class OpsGenieHandler : IOpsGenieHandler
{
    private readonly string _card = Path.Combine(".", "Resources", "AlertCard.json");
    private readonly ILogger<OpsGenieHandler> _logger;

    private readonly string _appPassword;
    private readonly string _appId;
    private readonly string _serviceUrl;
    private readonly string _channelId;

    public OpsGenieHandler(ILogger<OpsGenieHandler> logger, IConfiguration configuration)
    {

        _logger = logger;
        _appId = configuration["MicrosoftAppId"];
        _appPassword = configuration["MicrosoftAppPassword"];
        _serviceUrl = configuration["ServiceUrl"];
        _channelId = configuration["ChannelId"];
    }

    public async Task SendMessageAsync(string serviceUrl, string channelId, object message)
    {
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

            var result = await connectorClient.Conversations.SendToConversationAsync(activity);

            activity.Id = result.Id;
            ////activity.Attachments[0].Content = new JObject { ["text"] = "This is a test" };
            ////await connectorClient.Conversations.UpdateActivityAsync(activity);
            Console.WriteLine(result);
            _logger.LogInformation("[SendMessageAsync] Message sent...hopefully");

            string connectionString = "MYSQLCONNSTR_Listrk_SRE";
            using MySqlConnection connection = new MySqlConnection(connectionString);

            connection.Open();

            int alertIdValue = 1; // This can be any value you're checking/inserting
            string alertMessageValue = "New Alert Message"; // Message you want to insert or update

            string upsertSQL = @"
                                INSERT INTO alerts (alertId, conversationId) 
                                VALUES (@alertId, @conversationId) 
                                ON DUPLICATE KEY UPDATE conversationId = @conversationId;
";

            using MySqlCommand cmd = new MySqlCommand(upsertSQL, connection);
            cmd.Parameters.AddWithValue("@alertId", alertIdValue);
            cmd.Parameters.AddWithValue("@conversationId", result.Id);

            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows > 0)
            {
                if (affectedRows == 1)
                    Console.WriteLine("Insert operation performed.");
                else if (affectedRows == 2)
                    Console.WriteLine("Update operation performed.");
            }
            else
            {
                Console.WriteLine("No operation was performed.");
            }

            connection.Close();
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

    public Task ProcessNotification(string jsonPayload)
    {
        try
        {
            JObject jsonObject = JObject.Parse(jsonPayload);
            string actionValue = jsonObject["action"]?.ToString();

            IMessagePayload messagePayload;
            if (actionValue?.ToLower() == "create")
            {
                messagePayload = new NewAlertMessagePayload();
            }
            else
            {
                // Log the jsonPayload if the actionValue is not "create"
                _logger.LogError($"[WebhookConsumer] Payload: {jsonPayload}");
                _logger.LogError($"[WebhookConsumer] ActionType was: {actionValue}");
                if (jsonObject["data"] != null)
                {
                    messagePayload = new DataMessagePayload();
                }
                else
                {

                    return Task.CompletedTask;
                }
            }

            var message = messagePayload.CreateMessagePayload(jsonObject);
            var result = SendMessageAsync(_serviceUrl, _channelId, message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[WebhookConsumer] Error occurred: {e.Message}");
            _logger.LogError($"[WebhookConsumer] Error occurred: {e.Message}");
            return Task.FromException(e);
        }
        return Task.CompletedTask;
    }


}