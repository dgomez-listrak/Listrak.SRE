using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations;

public class NotificationProcessor : INotificationProcessor
{
    private readonly IBotFrameworkHttpAdapter Adapter;
    private readonly IBot Bot;
    private readonly ITeamsSendNotification TeamsThing;
    private readonly ILogger<WebhookConsumer> _logger;

    public NotificationProcessor(IBotFrameworkHttpAdapter adapter, IBot bot, ITeamsSendNotification teamsThing, ILogger<WebhookConsumer> logger)
    {
        Adapter = adapter;
        Bot = bot;
        TeamsThing = teamsThing;
        _logger = logger;
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
                    var result = TeamsThing.SendMessageAsync("https://smba.trafficmanager.net/amer/","19:24d638f4c79941298611e751c92277c4@thread.tacv2", message);
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