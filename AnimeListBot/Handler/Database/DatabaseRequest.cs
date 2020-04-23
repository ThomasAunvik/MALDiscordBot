﻿/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Data.SqlTypes;

namespace AnimeListBot.Handler
{
    public class DatabaseRequest
    {
        public static List<DiscordServer> GetAllServers()
        {
            return DatabaseConnection.db.DiscordServer.ToList();
        }

        public static List<DiscordUser> GetAllUsers()
        {
            return DatabaseConnection.db.DiscordUser.ToList();
        }

        public static bool DoesServerIdExist(ulong id)
        {
            return DatabaseConnection.db.DiscordServer.Find(id) == null;
        }

        public static async Task<DiscordServer> GetServerById(ulong id)
        {
            DiscordServer server = DatabaseConnection.db.DiscordServer.Find(id);
            if (server == null)
            {
                server = new DiscordServer(Program._client.GetGuild(id));
                await CreateServer(server);
            }
            return server;
        }

        public static async Task<bool> CreateServer(DiscordServer server)
        {
            DatabaseConnection.db.DiscordServer.Add(server);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateServer(DiscordServer server)
        {
            DiscordServer foundServer = DatabaseConnection.db.DiscordServer.Find(server.ServerId);
            foundServer.OverrideData(server);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> RemoveServer(DiscordServer server)
        {
            DatabaseConnection.db.DiscordServer.Remove(await GetServerById(server.ServerId));
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<DiscordUser> GetUserById(ulong id, bool forceUpdate = false)
        {
            DiscordUser user = DatabaseConnection.db.DiscordUser.Find(id);
            if(user == null)
            {
                await CreateUser(user = new DiscordUser(Program._client.GetUser(id)));
                if (user == null || user.UserId == 0) return null;
            }

            if(user.MalUsername != string.Empty && (user.malCachedTime < DateTime.Now || forceUpdate))
            {
                user.malCachedTime = DateTime.Now.AddMinutes(15);
                await user.UpdateMALInfo(user.MalUsername);
            }

            if (user.AnilistUsername != string.Empty && (user.anilistCachedTime < DateTime.Now || forceUpdate))
            {
                user.anilistCachedTime = DateTime.Now.AddMinutes(15);
                await user.UpdateAnilistInfo(user.AnilistUsername);
            }

            return user;
        }

        public static bool DoesUserIdExist(ulong id)
        {
            return DatabaseConnection.db.DiscordUser.Find(id) != null;
        }

        public static async Task<bool> CreateUser(DiscordUser user)
        {
            if (user == null || user.UserId == 0) return false;
            DatabaseConnection.db.DiscordUser.Add(user);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> RemoveUser(DiscordUser user)
        {
            DatabaseConnection.db.DiscordUser.Remove(await GetUserById(user.UserId));
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateUser(DiscordUser user)
        {
            DatabaseConnection.db.DiscordUser.Find(user.UserId).OverrideData(user);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }
    }
}
