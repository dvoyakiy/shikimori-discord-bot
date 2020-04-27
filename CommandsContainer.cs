using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShikimoriDiscordBot.Commands {
    public class CommandsContainer {
        [Command("hi")]
        public async Task Hi(CommandContext ctx) {
            await ctx.RespondAsync($"Hi, {ctx.User.Mention}");

            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(m => {
                return m.Author.Id == ctx.User.Id && m.Content.ToLower() == "how are you?";
            }, TimeSpan.FromSeconds(15));

            if (msg != null)
                await ctx.RespondAsync("fine, thanks");
        }
    }
}
