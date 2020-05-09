﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    public enum RankOption
    {
        ANIME, MANGA
    }

    public class ServerRanks
    {
        public ulong RegisterChannelId { get; set; } = 0;
        public List<RoleRank> AnimeRanks { get; set; } = new List<RoleRank>();
        public List<RoleRank> MangaRanks { get; set; } = new List<RoleRank>();
        public List<RoleRank> NotSetRanks { get; set; } = new List<RoleRank>();

        public void UpdateRankPermission(ulong id, ulong permission)
        {
            RoleRank rank;
            if((rank = AnimeRanks.Find(x => x.Id == id)) != null)
            {
                rank.RawGuildPermissionsValue = permission;
            }
            else if ((rank = MangaRanks.Find(x => x.Id == id)) != null)
            {
                rank.RawGuildPermissionsValue = permission;
            }
            else if ((rank = NotSetRanks.Find(x => x.Id == id)) != null)
            {
                rank.RawGuildPermissionsValue = permission;
            }
        }
    }
}
