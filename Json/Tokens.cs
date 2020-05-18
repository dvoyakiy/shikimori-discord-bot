using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShikimoriDiscordBot.Json {
    [JsonObject(MemberSerialization.OptIn)]
    class Tokens {
        [JsonProperty]
        public string access_token { get; set; }

        [JsonProperty]
        public string refresh_token { get; set; }
    }
}
