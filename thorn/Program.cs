﻿using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using thorn.Services;

namespace thorn
{
    internal static class Program
    {
        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .WriteTo.Console()
                .CreateLogger();

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    x.AddJsonFile("Config/config.json");
                })
                .UseSerilog()
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        AlwaysDownloadUsers = true,
                        DefaultRetryMode = RetryMode.RetryRatelimit,
                        ExclusiveBulkDelete = true,
                        MessageCacheSize = 50,
                        LogLevel = LogSeverity.Info
                    };

                    config.Token = context.Configuration["token"];
                    config.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
                })
                .UseCommandService((context, config) =>
                {
                    config.LogLevel = LogSeverity.Info;
                })
                .ConfigureServices((context, collection) =>
                {
                    collection.AddHostedService<CommandHandler>();
                    collection.AddHostedService<ReactionHandler>();

                    collection.AddSingleton<PairsService>();
                    collection.AddSingleton<DataStorageService>();
                    collection.AddSingleton<UserAccountsService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}