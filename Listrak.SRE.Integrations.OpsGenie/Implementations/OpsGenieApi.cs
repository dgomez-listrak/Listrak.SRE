using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{
    public class OpsGenieApi : IOpsGenieAPI
    {
        private readonly HttpClient _httpClient;
        private readonly OpsGenieSettings _settings;
        private readonly IConfiguration _configuration;

        public OpsGenieApi(HttpClient httpClient, IOptions<OpsGenieSettings> settings, IConfiguration config)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public void AcknowledgeAlert(string alertId)
        {
            var payload = new { isBulk = "false", alertId = alertId };
            SendUpdateRequestAsync($"{_settings.BaseUrl}/v2/alerts/{alertId}/acknowledge?identifierType=id", HttpMethod.Post,
                payload);
        }

        public void UnacknowledgeAlert(string alertId)
        {
            var payload = new { isBulk = "false", alertId = alertId };
            SendUpdateRequestAsync($"{_settings.BaseUrl}/v2/alerts/{alertId}/unacknowledge?identifierType=id", HttpMethod.Post,
               payload);
        }

        public void CloseAlert(string alertId)
        {
            var payload = new { isBulk = "false", alertId = alertId };
            SendUpdateRequestAsync($"{_settings.BaseUrl}/v2/alerts/{alertId}/close?identifierType=id", HttpMethod.Post, payload);
        }


        public void CommentOnAlert(string alertId, string comment)
        {
            // Similar payload and endpoint structure can go here.
        }

        public OpsGenieStatus GetAlertStatus(string alertId)
        {
            var response = SendStatusRequestAsync($"{_settings.BaseUrl}/v2/alerts/{alertId}?identifierType=id", HttpMethod.Get);
            var result = JsonConvert.DeserializeObject<OpsGenieStatus>(response.Result);
            return result;
        }

        private async Task<string> SendStatusRequestAsync(string url, HttpMethod method)
        {
            var requestMessage = new HttpRequestMessage(method, url);

            requestMessage.Headers.Add("Authorization", $"GenieKey {_settings.ApiKey}");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed: {response.StatusCode}");
            }
            return response.Content.ReadAsStringAsync().Result;
        }

        private async Task SendUpdateRequestAsync(string url, HttpMethod method, object payload = null)
        {
            var requestMessage = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("Authorization", $"GenieKey {_settings.ApiKey}");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed: {response.StatusCode}");
            }
        }
    }
}
