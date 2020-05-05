using System.IO;
using System.Collections.Generic;
using System.Text;
using ShikimoriDiscordBot.Database.Models;
using SQLite;
using SQLitePCL;
using System.Threading.Tasks;
using System.Linq;

namespace ShikimoriDiscordBot.Database {
    class DatabaseManager {
        public SQLiteAsyncConnection db { get; set; }

        public DatabaseManager(string databasePath = "../../../Database/users.db") {
            using (var fs = new FileStream(databasePath, FileMode.OpenOrCreate))
                db = new SQLiteAsyncConnection(databasePath);
        }

        public async Task Init() {
            await db.CreateTableAsync<User>();
        }

        public async Task InsertUser(string nickname, string clientId, string accessToken, string refreshToken) {
            await db.InsertAsync(new User() {
                Nickname = nickname,
                ClientId = clientId,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        public async Task<User> GetUser(string clientId) {
            var query = db.Table<User>().Where(user => user.ClientId == clientId);
            var result = await query.ToListAsync();

            return result.FirstOrDefault();
        }
    }
}
