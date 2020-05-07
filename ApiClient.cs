﻿using ShikimoriDiscordBot.Config;
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

namespace ShikimoriDiscordBot {
    class SearchResponse {
        public TitleInfo Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    class ApiClient {
        private readonly HttpClient httpClient;

        public ApiClient() {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ShikimoriDiscordBot");
        }

        private string GetApiUrl(string type) {
            //if ((type == "characters") || (type == "people"))
            //    return $"https://shikimori.org/api/{type}/search";

            if (type == "ranobe")
                return $"https://shikimori.org/api/{type}";

            return $"https://shikimori.org/api/{type}s";
        }

        private async Task<SearchResponse> SearchByQuery(string url, string query) {
            var response = await Get(url, new Dictionary<string, string> {
                {"search", query }
            });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new SearchResponse() { 
                    StatusCode = response.StatusCode
                };

            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<TitleInfo[]>(responseString);

            return new SearchResponse() { 
                Content = jsonResponse[0],
                StatusCode = response.StatusCode
            };
        }

        private async Task<SearchResponse> SearchById(string url, string id) {
            var response = await Get($"{url}/{id}");
            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<TitleInfo>(responseString);

            return new SearchResponse() {
                Content = jsonResponse,
                StatusCode = response.StatusCode
            };
        }

        public async Task<SearchResponse> SearchTitle(string type, string title, string accessToken) {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = GetApiUrl(type);
            SearchResponse searchResponse = await SearchByQuery(url, title);

            if (searchResponse.StatusCode == HttpStatusCode.Unauthorized) {
                return searchResponse;
            }

            searchResponse = await SearchById(url, searchResponse.Content.id.ToString());

            return searchResponse;
        }

        public async Task<Tokens> RefreshCurrentToken(string refreshToken) {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ShikimoriDiscordBot");

            var response = await Post("https://shikimori.org/oauth/token", new Dictionary<string, string> {
                    { "grant_type", "refresh_token" },
                    { "client_id", BotConfig.ShikimoriClientId },
                    { "client_secret", BotConfig.ShikimoriClientSecret },
                    { "refresh_token", refreshToken }
                 });
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);

            var jsonResponse = JsonConvert.DeserializeObject<Tokens>(responseString);

            return jsonResponse;
        }

        public async Task<HttpResponseMessage> Post(string url, Dictionary<string, string> body) {
            var content = new FormUrlEncodedContent(body);
            var response = await httpClient.PostAsync(url, content);

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
