// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Listrak.SRE.Integrations.OpsGenie.Implementations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Listrak.SRE.Integrations.OpsGenie
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((logging) =>
                    {
                        logging.AddDebug();
                        logging.AddConsole();
                        logging.AddAzureWebAppDiagnostics();
                    });
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices((context, collection) =>
                {
                    collection.AddHostedService<WebhookConsumer>();
                });
    }
}