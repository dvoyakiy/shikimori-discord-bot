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
        public static int SearchLimit { get; set; }

        public static string GreetMessage = $"Привет!\n" +
            $"Я помогу тебе пройти авторизацию, чтобы ты смог пользоваться ботом.\n\n" +
            $"Просто пройди по указанной ссылке и пришли мне полученый код.\n" +
            $"Но будь очень внимателен, ведь этот код секретный, так что никому его не показывай!\n" +
            $"Я буду ждать всего 5 минут, поэтому поспеши!";

        private class _ConfigModel {
            public string token { get; set; }
            public string prefix { get; set; }
            public string ShikimoriClientId { get; set; }
            public string ShikimoriClientSecret { get; set; }
            public int search_limit { get; set; }
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
            SearchLimit = cfg.search_limit;
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
