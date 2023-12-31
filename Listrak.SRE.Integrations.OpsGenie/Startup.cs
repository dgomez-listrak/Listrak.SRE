﻿using Listrak.SRE.Integrations.OpsGenie.Bots;
using Listrak.SRE.Integrations.OpsGenie.Implementations;
using Listrak.SRE.Integrations.OpsGenie.Interfaces;
using Listrak.SRE.Integrations.OpsGenie.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Listrak.SRE.Integrations.OpsGenie
{
    public class Startup
    {

        public IConfiguration Configuration { get; }  // <-- Add this property

        public Startup(IConfiguration configuration)  // <-- Add this constructor
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<OpsGenieSettings>(Configuration.GetSection("OpsGenieSettings"));  // <-- Add this line

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



            // Register the HttpClient and OpsGenieApi
            services.AddHttpClient<IOpsGenieAPI, OpsGenieApi>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddSingleton<IOpsGenieAPI, OpsGenieApi>();
            services.AddSingleton<IWebHookProducer, WebhookProducer>();
            services.AddSingleton<IWebhookConsumer, WebhookConsumer>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<IOpsGenieHandler, OpsGenieHandler>();
            services.AddTransient<IBot, TeamsHandler>();
            services.AddSingleton<IMySqlAdapter, MySqlAdapter>();
            services.AddSingleton<ICardBuilder, CardBuilder>();
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
