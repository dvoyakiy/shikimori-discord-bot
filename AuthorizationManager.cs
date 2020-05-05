using System;
using System.Collections.Generic;
using System.Text;
using ShikimoriDiscordBot.Database;
using System.Net;
using System.Net.Http;
using ShikimoriDiscordBot.Config;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ShikimoriDiscordBot.Authorization {
    class ShikimoriResponse {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public HttpStatusCode status { get; set; }
    }

    class AuthorizationManager {
        private HttpClient http = new HttpClient();

        public async Task<ShikimoriResponse> AuthorizeUser(string authCode) {
            http.DefaultRequestHeaders.Add("User-Agent", "ShikimoriDiscordBot");

            var values = new Dictionary<string, string> {
                    { "grant_type", "authorization_code" },
                    { "client_id", BotConfig.ShikimoriClientId },
                    { "client_secret", BotConfig.ShikimoriClientSecret },
                    { "code", authCode },
                    { "redirect_uri", "urn:ietf:wg:oauth:2.0:oob" }
                 };

            var content = new FormUrlEncodedContent(values);
            var response = await http.PostAsync("https://shikimori.org/oauth/token", content);
            var status = response.StatusCode;
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            string accessToken;
            string refreshToken;

            responseDict.TryGetValue("access_token", out accessToken);
            responseDict.TryGetValue("refresh_token", out refreshToken);

            return new ShikimoriResponse() {
                accessToken = accessToken,
                refreshToken = refreshToken,
                status = status
            };
        }

    }
}
