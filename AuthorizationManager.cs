using System;
using System.Collections.Generic;
using System.Text;
using ShikimoriDiscordBot.Database;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using ShikimoriDiscordBot.Config;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ShikimoriDiscordBot.Database.Models;

namespace ShikimoriDiscordBot.Authorization {
    class ShikimoriResponse {
        public long userId { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }

        public HttpStatusCode status { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    class JsonResponse {
        [JsonProperty]
        public long id { get; set; }

        [JsonProperty]
        public string access_token { get; set; }

        [JsonProperty]
        public string refresh_token { get; set; }
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
            var jsonResponse = JsonConvert.DeserializeObject<JsonResponse>(responseString);
            ShikimoriResponse shikimoriResponse = new ShikimoriResponse() {
                accessToken = jsonResponse.access_token,
                refreshToken = jsonResponse.refresh_token,
                status = status
            };

            if (jsonResponse.access_token != null) {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jsonResponse.access_token);
                responseString = await http.GetStringAsync("https://shikimori.org/api/users/whoami");
                jsonResponse = JsonConvert.DeserializeObject<JsonResponse>(responseString);

                shikimoriResponse.userId = jsonResponse.id;
            }

            return shikimoriResponse;
        }

    }
}
