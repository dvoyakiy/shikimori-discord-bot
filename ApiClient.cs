using ShikimoriDiscordBot.Config;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ShikimoriDiscordBot.Json;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Linq;

namespace ShikimoriDiscordBot {
    class SearchResponse<T> {
        public T Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    class ApiClient {
        private readonly HttpClient httpClient;

        private struct SearchTypes {
            public static string Ranobe = "ranobe";
            public static string User = "user";
        }

        public ApiClient() {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ShikimoriDiscordBot");
        }

        private string GetApiUrl(string type, string nickname = "") {
            if (type == SearchTypes.User)
                return $"https://shikimori.org/api/users";

            if (type == SearchTypes.Ranobe)
                return $"https://shikimori.org/api/{type}";

            return $"https://shikimori.org/api/{type}s";
        }

        public async Task<SearchResponse<List<TitleInfo>>> SearchTitleByQuery(string type, string title, string accessToken) {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var url = GetApiUrl(type);

            var response = await Get(url, new Dictionary<string, string> {
                {"search", title },
                {"limit", BotConfig.SearchLimit.ToString() }
            });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new SearchResponse<List<TitleInfo>>() { 
                    StatusCode = response.StatusCode
                };

            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<TitleInfo[]>(responseString);

            return new SearchResponse<List<TitleInfo>>() { 
                Content = jsonResponse.ToList(),
                StatusCode = response.StatusCode
            };
        }

        public async Task<SearchResponse<TitleInfo>> SearchTitleById(string type, int id) {
            var url = GetApiUrl(type);
            var response = await Get($"{url}/{id}");

            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<TitleInfo>(responseString);

            return new SearchResponse<TitleInfo>() {
                Content = jsonResponse,
                StatusCode = response.StatusCode
            };
        }

        public async Task<SearchResponse<UserInfo>> SearchUser(string nickname, string accessToken) {
            var url = GetApiUrl(SearchTypes.User, nickname);
            var response = await Get($"{url}/{nickname}?is_nickname=1");

            if (response.StatusCode != HttpStatusCode.OK) {
                return new SearchResponse<UserInfo>() {
                    StatusCode = response.StatusCode
                };
            }

            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<UserInfo>(responseString);
            
            return new SearchResponse<UserInfo>() {
                Content = jsonResponse,
                StatusCode = response.StatusCode
            };
        }

        public async Task<Tokens> RefreshCurrentToken(string refreshToken) {
            var response = await Post("https://shikimori.org/oauth/token", new Dictionary<string, string> {
                    { "grant_type", "refresh_token" },
                    { "client_id", BotConfig.ShikimoriClientId },
                    { "client_secret", BotConfig.ShikimoriClientSecret },
                    { "refresh_token", refreshToken }
                 });

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Tokens>(responseString);

            return jsonResponse;
        }

        public async Task<HttpResponseMessage> Post(string url, Dictionary<string, string> body) {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "ShikimoriDiscordBot");

            var content = new FormUrlEncodedContent(body);
            var response = await http.PostAsync(url, content);

            return response;
        }

        public async Task<HttpResponseMessage> Get(string url) {
            var response = await httpClient.GetAsync(url);

            return response;
        }

        public async Task<HttpResponseMessage> Get(string url, Dictionary<string, string> queryParams) {
            var builder = new UriBuilder(url);
            builder.Port = -1;

            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (KeyValuePair<string, string> entry in queryParams)
                query[entry.Key] = entry.Value;

            builder.Query = query.ToString();
            string requestUrl = builder.ToString();
            
            var response = await httpClient.GetAsync(requestUrl);

            return response;
        }
    }
}
