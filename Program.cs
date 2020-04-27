namespace ShikimoriDiscordBot {
    class Program {
        static void Main(string[] args) {
            Bot.RunAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();  
        }
    }
}
