using ShikimoriDiscordBot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using ShikimoriDiscordBot.Json;
using System.Linq;
using ShikimoriDiscordBot.Database.Models;

namespace ShikimoriDiscordBot.Helpers {
    static class CommandsHelper {
        public static readonly List<string> TitleSearchTypes = new List<string>() { "anime", "manga", "ranobe" };

        public static string BuildTitleName(string russian, string romaji, IList<string> japanese) {
            if (japanese == null || japanese[0] == null)
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

        public static string GetUserStatus(string status) {
            switch(status) {
                case "planned":
                    return "Запланировано";
                case "watching":
                    return "Смотрю";
                case "rewatching":
                    return "Пересматриваю";
                case "completed":
                    return "Просмотрено";
                case "on_hold":
                    return "Отложено";
                case "dropped":
                    return "Брошено";
                default:
                    return "N/a";
            }
        }

        public static string GetStudios(IList<Studio> studiosList) {
            var studios = from studio in studiosList select studio.name;
            Console.WriteLine(studios.Count());
            return string.Join(", ", studios);
        }

        public static string GetUrlTo(string part) {
            return "https://shikimori.org" + part;
        }

        public static DiscordEmbedBuilder BuildTitleListEmbed(Dictionary<int, TitleInfo> mappedTitles) {
            if (mappedTitles.Count == 0) {
                return new DiscordEmbedBuilder {
                    Title = "Ничего не найдено."
                };
            }

            var embed = new DiscordEmbedBuilder {
                Title = "Результаты поиска:"
            };

            foreach (KeyValuePair<int, TitleInfo> entry in mappedTitles)
                embed.AddField("\u200b", $"`{entry.Key}.` {BuildTitleName(entry.Value.russian, entry.Value.name, null)}");

            return embed;
        }

        public static DiscordEmbedBuilder BuildTitleEmbed(TitleInfo titleInfo, string type) {
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

            embed.AddField("В списке:", titleInfo.user_rate == null ? "N/a" : GetUserStatus(titleInfo.user_rate.status));

            return embed;
        }

        private static string GetGenderString(string gender) {
            return gender == "male" ? "мужчина" : "женщина";
        }

        private static string GetStatusesField(IList<TitleStatus> statuses) {
            var sb = new StringBuilder();

            foreach(var status in statuses) {
                sb.AppendLine($"{GetUserStatus(status.name)}: {status.size}");
            }

            return sb.ToString();
        }

        public static DiscordEmbedBuilder BuildUserInfoEmbed(UserInfo userInfo) {
            var embed = new DiscordEmbedBuilder {
                Title = userInfo.name != null ? $"{userInfo.nickname} ({userInfo.name})" : userInfo.nickname,
                ThumbnailUrl = userInfo.image.x160,
                Url = GetUrlTo(userInfo.nickname),
                Description = userInfo.last_online,
            };

            if (userInfo.sex != null)
                embed.AddField("Пол:", GetGenderString(userInfo.sex));

            if (userInfo.full_years != null)
                embed.AddField("Возраст:", $"{userInfo.full_years} лет");

            if (userInfo.location != null)
                embed.AddField("Откуда:", userInfo.location);

            if (userInfo.website != null)
                embed.AddField("Вебсайт:", userInfo.website);

            embed.AddField("Статистика аниме: ", GetStatusesField(userInfo.stats.statuses.anime));
            embed.AddField("Статистика манги и ранобэ: ", GetStatusesField(userInfo.stats.statuses.manga));


            return embed;
        }
    }
}
