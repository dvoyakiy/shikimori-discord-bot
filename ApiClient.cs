using ShikimoriDiscordBot.Config;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ShikimoriDiscordBot.Json;
using Newtonsoft.Json;

namespace ShikimoriDiscordBot {
    class ApiClient {
        public async Task<Tokens> RefreshCurrentToken(string refreshToken) {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "ShikimoriDiscordBot");

            var response = await Post("https://shikimori.org/oauth/token", new Dictionary<string, string> {
                    { "grant_type", "refresh_token" },
                    { "client_id", BotConfig.ShikimoriClientId },
                    { "client_secret", BotConfig.ShikimoriClientSecret },
                    { "refresh_token", refreshToken }
                 }, http);
            var responseString = await response.Content.ReadAsStringAsync();

            var jsonResponse = JsonConvert.DeserializeObject<Tokens>(responseString);

            return jsonResponse;
        }

        public async Task<HttpResponseMessage> Post(string url, Dictionary<string, string> body, HttpClient http) {
            var content = new FormUrlEncodedContent(body);
            var response = await http.PostAsync(url, content);

            return response;
        }

        public async Task<HttpResponseMessage> Get(string url, Dictionary<string, string> queryParams, HttpClient http) {
            var builder = new UriBuilder(url);
            builder.Port = -1;

            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (KeyValuePair<string, string> entry in queryParams)
                query[entry.Key] = entry.Value;

            builder.Query = query.ToString();
            string requestUrl = builder.ToString();
            
            var response = await http.GetAsync(requestUrl);

            return response;
        }
    }
}
