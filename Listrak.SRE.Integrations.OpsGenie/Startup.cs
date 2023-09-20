using Listrak.SRE.Integrations.OpsGenie.Implementations;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Listrak.SRE.Integrations.OpsGenie
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddAzureWebAppDiagnostics();
            }).BuildServiceProvider();
            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, AdaptiveCardsBot>();
            services.AddSingleton<IWebHookProducer, WebhookProducer>();
            services.AddSingleton<IWebhookConsumer, WebhookConsumer>();
            services.AddSingleton<ITeamsStartNewThreadInTeam, TeamsStartNewThreadInTeam>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            
            //services.AddTransient<IBot, TeamsConversationBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
