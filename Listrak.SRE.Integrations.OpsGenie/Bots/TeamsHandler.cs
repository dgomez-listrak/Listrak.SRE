using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Confluent.Kafka;
using Listrak.SRE.Integrations.OpsGenie.Implementations;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Listrak.SRE.Integrations.OpsGenie.Bots
{

    public class TeamsHandler : TeamsActivityHandler
    {
        private readonly ILogger<WebhookConsumer> _logger;
        private readonly IOpsGenieAPI _api;

        public TeamsHandler(ILogger<WebhookConsumer> logger, IOpsGenieAPI api)
        {
            _logger = logger;
            _api = api;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                if (turnContext.Activity.Value != null)
                {
                    var value = JsonConvert.DeserializeObject<NotificationActionModel>(turnContext.Activity.Value.ToString());
                    switch (value.type.ToLower())
                    {
                        case "unack":
                            _api.UnacknowledgeAlert(value.alertId);
                            break;

                        case "ack":
                            _api.AcknowledgeAlert(value.alertId);
                            break;

                        case "close":
                            _api.CloseAlert(value.alertId);
                            break;
                    }
                }
                else
                {
                    _logger.LogInformation("[TeamsHandler] Not sure what this is");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
            }
        }
    }
}
