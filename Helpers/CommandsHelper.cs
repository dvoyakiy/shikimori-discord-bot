using ShikimoriDiscordBot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using ShikimoriDiscordBot.Json;
using System.Linq;

namespace ShikimoriDiscordBot.Helpers {
    static class CommandsHelper {
        public static string BuildTitleName(string russian, string romaji, IList<string> japanese) {
            if (japanese[0] == null)
                return $"{russian} ({romaji})";

            return $"{russian} ({romaji} / { string.Join(" / ", japanese) })";
        }

        public static string GetStatus(string rawStatus) {
            switch (rawStatus) {
                case "anons":
                    return "Анонс";
                case "released":
                    return "Заверешен";
                case "ongoing":
                    return "Онгоинг";
                default:
                    return "Неизвестно";
            }
        }

        public static async Task UpdateTokens(string clientId, string refreshToken, DatabaseManager db) {
            var api = new ApiClient();
            var res = await api.RefreshCurrentToken(refreshToken);
            await db.Execute($"update User set AccessToken=\"{res.access_token}\", RefreshToken=\"{res.refresh_token}\" where ClientId={clientId}");
        }

        public static string GetGenres(IList<Genre> genresList) {
            var genres = from genre in genresList select genre.russian;
            return string.Join(", ", genres);
        }

        public static string GetStudios(IList<Studio> studiosList) {
            var studios = from studio in studiosList select studio.name;
            Console.WriteLine(studios.Count());
            return string.Join(", ", studios);
        }

        public static string GetUrlTo(string part) {
            return "https://shikimori.org" + part;
        }

        public static DiscordEmbedBuilder BuildEmbed(TitleInfo titleInfo, string type) {
            var embed = new DiscordEmbedBuilder {
                Title = BuildTitleName(titleInfo.russian, titleInfo.name, titleInfo.japanese),
                ImageUrl = GetUrlTo(titleInfo.image.original),
                Description = titleInfo.description,
                Url = GetUrlTo(titleInfo.url),
            };

            if (titleInfo.ongoing) {
                embed.AddField("Статус:", $"{GetStatus(titleInfo.status)}, с {titleInfo.aired_on}", true);

                if (type == "anime") {
                    string nextEpisodeDate = DateTime.Parse(titleInfo.next_episode_at).ToString();
                    embed.AddField("Следующая серия:", nextEpisodeDate.Substring(0, nextEpisodeDate.Length - 3), true);
                }
            } else if (titleInfo.released_on != null) {
                embed.AddField("Статус:", $"{GetStatus(titleInfo.status)}, с {titleInfo.aired_on} по {titleInfo.released_on}");
            } else {
                embed.AddField("Статус:", $"{GetStatus(titleInfo.status)}, {titleInfo.aired_on}");
            }

            embed.AddField("Тип:", titleInfo.kind.ToUpper());


            if (type == "anime") {
                embed.AddField("Эпизоды (вышло / всего):", $"{titleInfo.episodes_aired}/{titleInfo.episodes}", true);
                embed.AddField("Длительность эпизода:", titleInfo.duration.ToString(), true);
            }

            embed.AddField("Оценка:", titleInfo.score);
            embed.AddField("Жанры:", GetGenres(titleInfo.genres));

            if (titleInfo.studioOrPublisher != null)
                embed.AddField(type == "anime" ? "Студия:" : "Издатель:", GetStudios(titleInfo.studioOrPublisher));

            return embed;
        }
    }
}
