using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System;
using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Listrak.SRE.Integrations.OpsGenie.Implementations
{

    /*{
        private readonly string _apiKey = "348d8279-6ef8-4883-b0b5-a0853db14458";
        private readonly string _baseUrl = "https://api.opsgenie.com/v2/alerts";
        */

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

        public async Task AcknowledgeAlert(string alertId)
        {
            var payload = new {isBulk = "false", alertId = alertId};
            await SendRequestAsync($"{_settings.BaseUrl}/{alertId}/acknowledge?identifierType=id", HttpMethod.Post,
                payload);
        }

        public async Task UnacknowledgeAlert(string alertId)
        {
            // Similar payload and endpoint structure can go here.
        }

        public async Task CloseAlert(string alertId)
        {
            // Similar payload and endpoint structure can go here.
        }

        public async Task CommentOnAlert(string alertId, string comment)
        {
            // Similar payload and endpoint structure can go here.
        }

        private async Task SendRequestAsync(string url, HttpMethod method, object payload)
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
