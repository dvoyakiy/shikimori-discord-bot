using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace ShikimoriDiscordBot.Database.Models {
    public class User {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ClientId { get; set; }
        public long ShikimoriUserId { get; set; }
        public string ShikimoriNickname{ get; set; }
        public string Nickname { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
