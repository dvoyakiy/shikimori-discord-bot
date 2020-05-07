﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShikimoriDiscordBot.Json {
    [JsonObject(MemberSerialization.OptIn)]
    public class Image {
        //public string original { get; set; }
        [JsonProperty]
        public string preview { get; set; }
        //public string x96 { get; set; }
        //public string x48 { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Genre {
        //public int id { get; set; }
        [JsonProperty]
        public string name { get; set; }
        //public string russian { get; set; }
        //public string kind { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TitleInfo {
        [JsonProperty]
        public int id { get; set; }
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public string russian { get; set; }
        [JsonProperty]
        public IList<string> english { get; set; }
        [JsonProperty]
        public IList<string> japanese { get; set; }
        [JsonProperty]
        public Image image { get; set; }
        [JsonProperty]
        public string url { get; set; }
        [JsonProperty]
        public string kind { get; set; }
        [JsonProperty]
        public string score { get; set; }
        [JsonProperty]
        public string status { get; set; }
        [JsonProperty]
        public int episodes { get; set; }
        [JsonProperty]
        public int duration { get; set; }
        [JsonProperty]
        public string description { get; set; }
        [JsonProperty]
        public IList<Genre> genres { get; set; }
    }
}
