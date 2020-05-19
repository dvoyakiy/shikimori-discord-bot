using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShikimoriDiscordBot.Database;
using ShikimoriDiscordBot.Config;
using ShikimoriDiscordBot.Authorization;
using ShikimoriDiscordBot.Helpers;
using System.Net;
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
            if (!CommandsHelper.TitleSearchTypes.Contains(type)) {
                await ctx.RespondAsync("Неизвестный тип контента.\nОтправьте `!shiki help` чтобы посмотреть список комманд.");
                return;
            }

            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user == null) {
                await ctx.RespondAsync($"{ctx.Message.Author.Mention}\nДля этого тебе нужно авторизоваться!\n\nОтправь команду `!shiki auth` и выполни полученные инструкции.");
                return;
            }

            var found = await api.SearchTitleByQuery(type, title, user.AccessToken);

            if (found.StatusCode == HttpStatusCode.Unauthorized) {
                await CommandsHelper.UpdateTokens(user.ClientId, user.RefreshToken, db);

                user = await db.GetUser(ctx.User.Id.ToString());
                found = await api.SearchTitleByQuery(type, title, user.AccessToken);
            }

            Dictionary<int, TitleInfo> mappedTitles = new Dictionary<int, TitleInfo>();

            for (int idx = 0; idx < found.Content.Count; idx++)
                mappedTitles.Add(idx + 1, found.Content[idx]);

            if (mappedTitles.Count == 0) {
                var notFoundEmbed = CommandsHelper.BuildNotFoundEmbed();
                await ctx.RespondAsync(embed: notFoundEmbed);
                return;
            }

            var resultsEmbed = CommandsHelper.BuildTitleListEmbed(mappedTitles);
            await ctx.RespondAsync(embed: resultsEmbed);

            var interactivity = ctx.Client.GetInteractivityModule();
            int titleIndex = 0;

            do {
                var choice = await interactivity.WaitForMessageAsync(m => m.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(1));

                if (choice == null) {
                    await ctx.RespondAsync("Я устал ждать... Увидимся позже.");
                    return;
                }

                try {
                    titleIndex = Convert.ToInt32(choice.Message.Content);
                } catch (FormatException) {
                    continue;
                }


                if (titleIndex < 1 || titleIndex > BotConfig.SearchLimit)
                    continue;

            } while (titleIndex < 1 || titleIndex > BotConfig.SearchLimit);


            var response = await api.SearchTitleById(type, mappedTitles[titleIndex].id);
            var embed = CommandsHelper.BuildTitleEmbed(response.Content, type);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("auth")]
        public async Task Auth(CommandContext ctx) {
            if (ctx.Message.Channel.IsPrivate)
                return;

            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user != null) {
                await ctx.RespondAsync($"{ctx.Message.Author.Mention}, ты уже авторизован :3");
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
                    await dm.SendMessageAsync("Я устал ждать... Увидимся позже.");
                    return;
                }

                response = await authManager.AuthorizeUser(msg.Message.Content);

                if (response.status == HttpStatusCode.BadRequest)
                    await dm.SendMessageAsync($"Кажется, ты прислал неверный код. Попробуй ещё раз!\n\n{BotConfig.AuthURL}");

            } while (response.status == HttpStatusCode.BadRequest);


            await db.InsertUser(
                nickname: ctx.Message.Author.Username,
                clientId: ctx.User.Id.ToString(),
                shikimoriUserId: response.userId,
                shikimoriNickname: response.nickname,
                accessToken: response.accessToken,
                refreshToken: response.refreshToken
            );

            await dm.SendMessageAsync("Авторизация прошла успешно!");
        }

        [Command("user")]
        public async Task User(CommandContext ctx, string nickname) {
            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user == null) {
                await ctx.RespondAsync($"{ctx.Message.Author.Mention}\nДля этого тебе нужно авторизоваться!\n\nОтправь команду `!shiki auth` и выполни полученные инструкции.");
                return;
            }

            var found = await api.SearchUser(nickname, user.AccessToken);

            if (found.StatusCode == HttpStatusCode.Unauthorized) {
                await CommandsHelper.UpdateTokens(user.ClientId, user.RefreshToken, db);

                user = await db.GetUser(ctx.User.Id.ToString());
                found = await api.SearchUser(nickname, user.AccessToken);
            }

            if (found.StatusCode == HttpStatusCode.NotFound) {
                var notFoundEmbed = CommandsHelper.BuildNotFoundEmbed();
                await ctx.RespondAsync(embed: notFoundEmbed);
                return;
            }

            var embed = CommandsHelper.BuildUserInfoEmbed(found.Content);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("me")]
        public async Task Me(CommandContext ctx) {
            var user = await db.GetUser(ctx.User.Id.ToString());

            if (user == null) {
                await ctx.RespondAsync($"{ctx.Message.Author.Mention}\nДля этого тебе нужно авторизоваться!\n\nОтправь команду `!shiki auth` и выполни полученные инструкции.");
                return;
            }

            var found = await api.SearchUser(user.ShikimoriNickname, user.AccessToken);

            if (found.StatusCode == HttpStatusCode.Unauthorized) {
                await CommandsHelper.UpdateTokens(user.ClientId, user.RefreshToken, db);

                user = await db.GetUser(ctx.User.Id.ToString());
                found = await api.SearchUser(user.ShikimoriNickname, user.AccessToken);
            }

            var embed = CommandsHelper.BuildUserInfoEmbed(found.Content);
            await ctx.RespondAsync(embed: embed);
        }
    }
}
