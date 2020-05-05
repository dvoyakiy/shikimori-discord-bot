using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ShikimoriDiscordBot.Database;
using ShikimoriDiscordBot.Config;
using ShikimoriDiscordBot.Authorization;
using System.Net.Http;
using Newtonsoft.Json;

namespace ShikimoriDiscordBot.Commands {
    public class CommandsContainer {
        private DatabaseManager db = new DatabaseManager();
        private readonly HttpClient http = new HttpClient();

        [Command("search")]
        public async Task Hi(CommandContext ctx, string title) {
            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user == null) {
                await ctx.RespondAsync("Please authorize!");
                var dm = await ctx.Member.CreateDmChannelAsync();
                await dm.SendMessageAsync(BotConfig.AuthURL);
            }
        }

        [Command("auth")]
        public async Task Auth(CommandContext ctx) {
            if (ctx.Message.Channel.IsPrivate)
                return;

            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user != null) {
                await ctx.RespondAsync($"Друже {ctx.Message.Author.Mention}, ти вже авторизований :3");
                return;
            }

            var dm = await ctx.Member.CreateDmChannelAsync();
            var interactivity = ctx.Client.GetInteractivityModule();
            var authManager = new AuthorizationManager();
            ShikimoriResponse response;

            await dm.SendMessageAsync($"{BotConfig.GreetMessage}\n\n{BotConfig.AuthURL}");

            do {
                var msg = await interactivity.WaitForMessageAsync(m => {
                    return m.Author.Id == ctx.User.Id && m.Channel.IsPrivate;
                }, TimeSpan.FromMinutes(5));

                if (msg == null) {
                    await dm.SendMessageAsync("Я втомився чекати... Побачимось пізніше.");
                    return;
                }

                response = await authManager.AuthorizeUser(msg.Message.Content);

                if (response.status == System.Net.HttpStatusCode.BadRequest)
                    await dm.SendMessageAsync($"Здається, ти надіслав неправильний код. Спробуй ще раз!\n\n{BotConfig.AuthURL}");

            } while (response.status == System.Net.HttpStatusCode.BadRequest);

            

            await db.InsertUser(
                nickname: ctx.Message.Author.Username,
                clientId: ctx.User.Id.ToString(),
                accessToken: response.accessToken,
                refreshToken: response.refreshToken
            );

            await dm.SendMessageAsync("Авторизація пройшла успішно!");
        }
    }
}
