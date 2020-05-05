using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using DSharpPlus;
using System.Threading.Tasks;

namespace ShikimoriDiscordBot.Config {
    static class BotConfig {
        public static string Token { get; set; }
        public static string Prefix { get; set; }
        public static string AuthURL { get; set; }
        public static string ShikimoriClientSecret { get; set; }
        public static string ShikimoriClientId { get; set; }

        public static string GreetMessage = $"Привіт!\n" +
            $"Я допоможу тобі пройти авторизацію, щоб ти зміг користуватись ботом.\n\n" +
            $"Просто пройди за вказаним посиланням і надішли отриманий код мені.\n" +
            $"Але будь дуже уважним, адже цей код секретний, тому нікому його не показуй!\n" +
            $"Я чекатиму всього 5 хвилин, тому поспіши!";

        private class _ConfigModel {
            public string token { get; set; }
            public string prefix { get; set; }
            public string ShikimoriClientId { get; set; }
            public string ShikimoriClientSecret { get; set; }
        }

        public static DiscordConfiguration GetDiscordConfiguration(string configFile) {
            string _json;

            using (var sr = new StreamReader(File.OpenRead(configFile), new UTF8Encoding(false)))
                _json = sr.ReadToEnd();

            _ConfigModel cfg = JsonConvert.DeserializeObject<_ConfigModel>(_json);

            Token = cfg.token;
            Prefix = cfg.prefix;
            ShikimoriClientSecret = cfg.ShikimoriClientSecret;
            ShikimoriClientId = cfg.ShikimoriClientId;
            AuthURL = $"https://shikimori.org/oauth/authorize?client_id={cfg.ShikimoriClientId}&redirect_uri=urn%3Aietf%3Awg%3Aoauth%3A2.0%3Aoob&response_type=code&scope=user_rates";

            return new DiscordConfiguration
            {
                Token = cfg.token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
        }
        
    }
}
