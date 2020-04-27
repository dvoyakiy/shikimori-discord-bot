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

        private class _ConfigModel {
            public string token { get; set; }
            public string prefix { get; set; }
        }

        public static DiscordConfiguration GetDiscordConfiguration(string configFile) {
            string _json;

            using (var sr = new StreamReader(File.OpenRead(configFile), new UTF8Encoding(false)))
                _json = sr.ReadToEnd();

            _ConfigModel cfg = JsonConvert.DeserializeObject<_ConfigModel>(_json);

            Token = cfg.token;
            Prefix = cfg.prefix;

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
