using System;
using System.IO;
using System.Threading.Tasks;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using MySqlConnector;

namespace Listrak.SRE.Integrations.OpsGenie
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebHookController : ControllerBase
    {

        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly IWebHookProducer _webHookProducer;
        public WebHookController(IBotFrameworkHttpAdapter adapter, IBot bot, IWebHookProducer webHookProducer)
        {
            _adapter = adapter;
            _bot = bot;
            _webHookProducer = webHookProducer;
        }


        // POST api/webhooks/receive
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveWebhook()
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    string data = await reader.ReadToEndAsync();

                    if (string.IsNullOrEmpty(data))
                    {
                        return BadRequest();
                    }

                    //await _adapter.ProcessAsync(Request, Response, _bot);
                    // Now, you have the raw JSON in 'data' variable. Process it as needed.
                    // Send to EvenTHub
                    await _webHookProducer.Produce(data);
                    return Ok("thanks");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

    }
}
