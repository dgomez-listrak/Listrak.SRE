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

    public class NotificationCardHandler : TeamsActivityHandler
    {
        private readonly ILogger<WebhookConsumer> _logger;
        public NotificationCardHandler(ILogger<WebhookConsumer> logger)
        {
            _logger = logger;
        }
        private readonly string[] _cards =
        {
            Path.Combine(".", "Resources", "AlertCard.json")

        };

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                if (turnContext.Activity.Value != null)
                {
                    // if value json has type of Acknowledge, thne do this
                    //  https://lstrk.app.opsgenie.com/webapi/alert/acknowledge?_=1695825793886
                    // ?isBulk=false&alertId=aadacde3-2076-453d-9682-22e0f8317f18-1695825328305

                    var value = JsonConvert.DeserializeObject<NotificationActionModel>(turnContext.Activity.Value.ToString());
                    switch (value.type.ToLower())
                    {
                        case "ack":
                            // do http post to  https://lstrk.app.opsgenie.com/webapi/alert/acknowledge
                            //HttpWebRequest request = new HttpWebRequest($"https://lstrk.app.opsgenie.com/webapi/alert/acknowledge?isBulk=false&alertId={value.alertId}");
                            AcknowledgeAlert(value.alertId);
                            // Do some card update

                            var message = new
                            {
                                Title = $"Title Variable UPDATED",
                                Description = $"Description Variable",
                                AlertId = $"AlertID Variable",
                                Priority = $"Priority Variable",
                                Status = $"{string.Empty}",
                                Source = $"Source Varialbe",
                            };
                            var cardAttachment = CreateAdaptiveCardAttachment(_cards[0], message);

                            /*var activity = new Activity
                            {
                                Type = ActivityTypes.Message,
                                ServiceUrl = string.Empty,
                                ChannelId = string.Empty
                                
                            };
                            */
                            var activity = turnContext.Activity;

                            //var activity = MessageFactory.Attachment(cardAttachment);
                            activity.Attachments = new List<Attachment>() { cardAttachment };




                            // HeroCard card = new AdaptiveCards();
                            //  card.Title = "I've been updated";

                            var data = turnContext.Activity.Value as JObject;
                            data = JObject.FromObject(data);
                            // data["count"] = data["count"].Value<int>() + 1;
                            // card.Text = $"Update count - {data["count"].Value<int>()}";
                            /*card.Buttons = new List<CardAction>();
                            card.Buttons.Add(new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Update Card",
                                Text = "UpdateCardAction",
                                Value = data
                            });
                            */

                            //var activity = MessageFactory.Attachment(card.ToAttachment());

                            activity.Id = turnContext.Activity.Id;

                            activity.Conversation.Id = turnContext.Activity.Conversation.Id;


                            var x = await turnContext.UpdateActivityAsync(activity, cancellationToken);
                            Console.WriteLine(x.ToString());

                            break;

                        case "close":
                            CloseAlert(value.alertId);
                            break;
                    }
                }
                else
                {
                    //return;
                    string message = turnContext.Activity.Text;
                    var cardAttachment = CreateAdaptiveCardAttachment(_cards[0], string.Empty);
                    var serviceUrl = turnContext.Activity.ServiceUrl;

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Please enter any text to see another card."),
                        cancellationToken);
                    if (!string.IsNullOrEmpty(message))
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text($"Dynamic response: {message} - URL: {serviceUrl}"), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
            }
        }


        public static void AcknowledgeAlert(string alertId)
        {
            string apiKey = "348d8279-6ef8-4883-b0b5-a0853db14458";
            string baseUrl = "https://api.opsgenie.com/v2/alerts";
            string fullUrl = $"{baseUrl}/{alertId}/acknowledge?identifierType=id";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);

            // Set method to POST
            request.Method = "POST";

            // Add API key to header
            request.Headers.Add("Authorization", $"GenieKey {apiKey}");

            // Set content type
            request.ContentType = "application/json";

            // Create JSON payload
            var payload = new { isBulk = "false", alertId = alertId };
            string payloadString = JsonConvert.SerializeObject(payload);

            // Write JSON payload to request stream
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(payloadString);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static void CloseAlert(string alertId)
        {
            string apiKey = "348d8279-6ef8-4883-b0b5-a0853db14458";
            string baseUrl = "https://api.opsgenie.com/v2/alerts";
            string fullUrl = $"{baseUrl}/{alertId}/close?identifierType=id";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);

            // Set method to POST
            request.Method = "POST";

            // Add API key to header
            request.Headers.Add("Authorization", $"GenieKey {apiKey}");

            // Set content type
            request.ContentType = "application/json";

            // Create JSON payload
            var payload = new { isBulk = "false", alertId = alertId };
            string payloadString = JsonConvert.SerializeObject(payload);

            // Write JSON payload to request stream
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(payloadString);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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
}
