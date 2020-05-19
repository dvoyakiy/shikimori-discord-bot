using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ShikimoriDiscordBot.Json {
    [JsonObject(MemberSerialization.OptIn)]
    public class UserImage {

        [JsonProperty("x160")]
        public string x160 { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TitleStatus {

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("grouped_id")]
        public string grouped_id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("size")]
        public int size { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Statuses {

        [JsonProperty("anime")]
        public IList<TitleStatus> anime { get; set; }

        [JsonProperty("manga")]
        public IList<TitleStatus> manga { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Stats {

        [JsonProperty("statuses")]
        public Statuses statuses { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class UserInfo {

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("nickname")]
        public string nickname { get; set; }

        [JsonProperty("image")]
        public UserImage image { get; set; }

        [JsonProperty("last_online")]
        public string last_online { get; set; }

        [JsonProperty("full_years")]
        public object full_years { get; set; }

        [JsonProperty("name")]
        private string _name {
            get; set;
        }

        [JsonProperty("sex")]
        private string _sex {
            get; set;
        }

        [JsonProperty("website")]
        private string _website {
            get; set;
        }

        [JsonProperty("location")]
        private string _location {
            get; set;
        }

        public string name {
            get {
                if (_name == string.Empty)
                    return null;
                else
                    return _name;
            }
        }

        public string sex {
            get {
                if (_sex == string.Empty)
                    return null;
                else
                    return _sex;
            }
        }

        public string website {
            get {
                if (_website == string.Empty)
                    return null;
                else
                    return _website;
            }
        }

        public string location {
            get {
                if (_location == string.Empty)
                    return null;
                else
                    return _location;
            }
        }

        [JsonProperty("stats")]
        public Stats stats { get; set; }
    }
}
