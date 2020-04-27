using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using ShikimoriDiscordBot.Config;

namespace ShikimoriDiscordBot {
    static class Bot {
        static DiscordClient discord;
        static CommandsNextModule commands;
        static InteractivityModule interactivity;

        static public async Task RunAsync(string[] args) {
            discord = new DiscordClient(BotConfig.GetDiscordConfiguration("config.json"));
            interactivity = discord.UseInteractivity(new InteractivityConfiguration());
            commands = discord.UseCommandsNext(new CommandsNextConfiguration {
                StringPrefix = BotConfig.Prefix
            });

            commands.RegisterCommands<ShikimoriDiscordBot.Commands.Commands>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
