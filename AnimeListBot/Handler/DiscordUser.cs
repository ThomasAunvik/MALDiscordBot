﻿using Discord;
using Discord.WebSocket;
using JikanDotNet;
using AnimeListBot.Handler.Anilist;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace AnimeListBot.Handler
{
    public class DiscordUser
    {
        public enum AnimeList
        {
            MAL,
            Anilist
        }

        public ulong userID;
        
        public UserProfile malProfile;
        public IAniUser anilistProfile;
        
        public AnimeList animeList;

        public decimal animeDays;
        public decimal mangaDays;

        public DiscordUser() { }

        // Most of the reasons you do this part is to create a new user and upload it to the db automaticly
        public DiscordUser(IUser user) {
            userID = user.Id;
        }
        public IUser GetUser() { return Program._client.GetUser(userID); }

        public async Task CreateUserDatabase()
        {
            if (!await DatabaseRequest.DoesUserIdExist(userID))
                await DatabaseRequest.CreateUser(this);
            else await UpdateDatabase();
        }

        public async Task UpdateDatabase()
        {
            if (await DatabaseRequest.DoesUserIdExist(userID))
                await DatabaseRequest.UpdateUser(this);
        }

        public string GetAnimelistUsername()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.Username == null ? "" : malProfile.Username;
                case AnimeList.Anilist:
                    return anilistProfile.name == null ? "" : anilistProfile.name;
                default:
                    return "";
            }
        }

        public string GetAnimelistThumbnail()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.ImageURL;
                case AnimeList.Anilist:
                    return anilistProfile.Avatar?.large;
                default:
                    return "";
            }
        }

        public string GetAnimelistLink()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile?.URL;
                case AnimeList.Anilist:
                    return anilistProfile?.siteUrl;
                default:
                    return "";
            }
        }

        #region AnimeStats

        public (ulong, double) GetAnimeServerRank(DiscordServer server)
        {
            if (server.animeRoleIds.Count < 1) return (0, 0);

            double animeDays = (double)GetAnimeWatchDays();
            for(int roleIndex = 0; roleIndex < server.animeRoleDays.Count; roleIndex++)
            {
                if (animeDays < server.animeRoleDays[roleIndex]) {
                    if (roleIndex < 1) return (0,0);
                    roleIndex--;
                    return (server.animeRoleIds[roleIndex], server.animeRoleDays[roleIndex]);
                }
            }
            return (server.animeRoleIds[server.animeRoleIds.Count-1], server.animeRoleDays[server.animeRoleIds.Count-1]);
        }

        public decimal GetAnimeWatchDays()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    decimal mal_days = malProfile.AnimeStatistics.DaysWatched.GetValueOrDefault();
                    return decimal.Round(mal_days, 1);
                case AnimeList.Anilist:
                    decimal minutesWatched = anilistProfile.statistics.anime.minutesWatched;
                    decimal ani_days = minutesWatched / (decimal)60.0 / (decimal)24.0;
                    return decimal.Round(ani_days, 1);
                default:
                    return 0;
            }
        }

        public float GetAnimeMeanScore()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return (float)malProfile.AnimeStatistics.MeanScore.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.statistics.anime?.meanScore).GetValueOrDefault() / 10.0f;
                default:
                    return 0;
            }
        }

        public int GetAnimeTotalEntries()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.TotalEntries.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.anime.count;
                default:
                    return 0;
            }
        }

        public int GetAnimeEpisodesWatched()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.EpisodesWatched.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.anime.episodesWatched;
                default:
                    return 0;
            }
        }

        public int GetAnimeRewatched()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Rewatched.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.REPEATING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeWatching()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Watching.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.CURRENT)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeCompleted()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Completed.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.COMPLETED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeOnHold()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.OnHold.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.PAUSED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeDropped()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Dropped.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.DROPPED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimePlanToWatch()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.PlanToWatch.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.PLANNING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        #region MangaStats

        public (ulong, double) GetMangaServerRank(DiscordServer server)
        {
            if (server.mangaRoleIds.Count < 1) return (0, 0);

            double mangaDays = (double)GetMangaReadDays();
            for (int roleIndex = 0; roleIndex < server.mangaRoleDays.Count; roleIndex++)
            {
                if (mangaDays < server.mangaRoleDays[roleIndex])
                {
                    if (roleIndex < 1) return (0,0);
                    roleIndex--;
                    return (server.mangaRoleIds[roleIndex], server.mangaRoleDays[roleIndex]);
                }
            }
            return (server.mangaRoleIds[server.mangaRoleIds.Count-1], server.mangaRoleDays[server.mangaRoleIds.Count-1]);
        }

        public decimal GetMangaReadDays()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    decimal mal_mangaRead = decimal.Round(malProfile.MangaStatistics.DaysRead.GetValueOrDefault(), 1);
                    return mal_mangaRead;
                case AnimeList.Anilist:
                    // Average days it takes to read 1 chapter
                    decimal chaptersRead = decimal.Multiply((anilistProfile.statistics?.manga.chaptersRead).GetValueOrDefault(), (decimal)0.00556);
                    return Math.Round(chaptersRead, 1);
                default:
                    return 0;
            }
        }

        public float GetMangaMeanScore()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return (float)malProfile.MangaStatistics.MeanScore.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.statistics.manga?.meanScore).GetValueOrDefault() / 10.0f;
                default:
                    return 0;
            }
        }

        public int GetMangaTotalEntries()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.TotalEntries.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.manga.count;
                default:
                    return 0;
            }
        }

        public int GetMangaChaptersRead()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.ChaptersRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.manga.chaptersRead;
                default:
                    return 0;
            }
        }

        public int GetMangaVolumesRead()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.VolumesRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.manga.volumesRead;
                default:
                    return 0;
            }
        }

        public int GetMangaReread()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Reread.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.REPEATING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaReading()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Reading.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.CURRENT)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaCompleted()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Completed.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.COMPLETED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaOnHold()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.OnHold.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.PAUSED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaDropped()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Dropped.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.DROPPED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaPlanToRead()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.PlanToRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.PLANNING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        public async Task<bool> UpdateUserInfo()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    if (malProfile == null) return false;
                    return await UpdateMALInfo(malProfile.Username);
                case AnimeList.Anilist:
                    if (anilistProfile == null) return false;
                    return await UpdateAnilistInfo(anilistProfile.name);
                default:
                    return false;
            }
        }

        public async Task<bool> UpdateMALInfo(string username)
        {
            malProfile = await Program._jikan.GetUserProfile(username);
            return malProfile != null;
        }

        public async Task<bool> UpdateAnilistInfo(string username)
        {
            anilistProfile = await AniUserQuery.GetUser(username);
            return anilistProfile != null;
        }
    }
}