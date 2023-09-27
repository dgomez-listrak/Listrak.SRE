using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Listrak.SRE.Integrations.OpsGenie.Bots
{
    public class NotificationCardHandler : ActivityHandler
    {
        private readonly string[] _cards =
        {
            Path.Combine(".", "Resources", "AlertCard.json")

        };

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                /// if value json has type of Acknowledge, thne do this
                ///    https://lstrk.app.opsgenie.com/webapi/alert/acknowledge?_=1695825793886
                /// ?isBulk=false&alertId=aadacde3-2076-453d-9682-22e0f8317f18-1695825328305

                var value = JsonConvert.DeserializeObject<NotificationActionModel>(turnContext.Activity.Value.ToString());
                switch (value.type.ToLower())
                {
                    case "ack":
                        /// do http post to  https://lstrk.app.opsgenie.com/webapi/alert/acknowledge
                        //HttpWebRequest request = new HttpWebRequest($"https://lstrk.app.opsgenie.com/webapi/alert/acknowledge?isBulk=false&alertId={value.alertId}");
                        AcknowledgeAlert(value.alertId);
                        break;
                    case "close":
                        CloseAlert(value.alertId);
                        break;
                }
            }
            else
            {
                return;
                string message = turnContext.Activity.Text;
                var cardAttachment = CreateAdaptiveCardAttachment(_cards[0]);
                var serviceUrl = turnContext.Activity.ServiceUrl;

                await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("Please enter any text to see another card."),
                    cancellationToken);
                if (!string.IsNullOrEmpty(message))
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text($"Dynamic response: {message} - URL: {serviceUrl}"), cancellationToken);
                }
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

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

    }
}
