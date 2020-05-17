using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ShikimoriDiscordBot.Database;
using ShikimoriDiscordBot.Database.Models;
using ShikimoriDiscordBot.Config;
using ShikimoriDiscordBot.Authorization;
using ShikimoriDiscordBot.Helpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;
using ShikimoriDiscordBot;
using DSharpPlus.EventArgs;
using ShikimoriDiscordBot.Json;

namespace ShikimoriDiscordBot.Commands {
    public class CommandsContainer {
        private readonly DatabaseManager db;
        private readonly ApiClient api;

        public CommandsContainer() {
            db = new DatabaseManager();
            db.Init().GetAwaiter().GetResult();

            api = new ApiClient();
        }

        [Command("search")]
        public async Task Hi(CommandContext ctx, string type, string title) {
            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user == null) {
                await ctx.RespondAsync($"{ctx.Message.Author.Mention}\nДля для цього потрібно авторизуватись!\n\nНадішли команду `!shiki auth` і виконай надіслані інструкції.");
                return;
            }

            var response = await api.SearchTitle(type, title, user.AccessToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                await CommandsHelper.UpdateTokens(user.ClientId, user.RefreshToken);

                user = await db.GetUser(ctx.User.Id.ToString());
                response = await api.SearchTitle(type, title, user.AccessToken);
            }

            var titleInfo = response.Content;
            var embed = CommandsHelper.BuildEmbed(titleInfo);
    
            await ctx.RespondAsync(embed: embed);
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
                shikimoriUserId: response.userId,
                accessToken: response.accessToken,
                refreshToken: response.refreshToken
            );

            await dm.SendMessageAsync("Авторизація пройшла успішно!");
        }
    }
}
