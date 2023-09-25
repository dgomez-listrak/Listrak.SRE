using AdaptiveCards.Templating;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations;

public class TeamsSendNotification : ITeamsSendNotification
{
    private readonly string _appId;
    private readonly string _appPassword;
    private readonly string _tenantId;
    private readonly ILogger<TeamsSendNotification> _logger;
    public IBotFrameworkHttpAdapter Adapter { get; }

    private readonly string _card = Path.Combine(".", "Resources", "AlertCard.json");

    public TeamsSendNotification(IConfiguration configuration, IBotFrameworkHttpAdapter adapter, ILogger<TeamsSendNotification> logger)
    {
        Adapter = adapter;
        _appId = configuration["MicrosoftAppId"];
        _appPassword = configuration["MicrosoftAppPassword"];
        _tenantId = configuration["MicrosoftAppTenantId"];
        _logger = logger;
    }

    public async Task SendMessageAsync(string serviceUrl, string channelId, object message)
    {
        _logger.LogInformation("[TeamsSendNotification]  SendMessageAsync to Teams Begin");
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
                Conversation = new ConversationAccount(id: channelId)
            };
            activity.Attachments = new List<Attachment>() { cardAttachment };


            await connectorClient.Conversations.SendToConversationAsync(activity);

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

}