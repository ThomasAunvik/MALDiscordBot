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
using AnimeListBot.Handler;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Discord;
using System.Net.NetworkInformation;
using System.Reflection;
using AnimeListBot.Handler.Database;

namespace AnimeListBot.Modules
{
    public class BotInfo : ModuleBase<ShardedCommandContext>
    {
        private IDatabaseService _db;

        public BotInfo(IDatabaseService db)
        {
            _db = db;
        }

        struct SavedStats
        {
            public int totalCommands;
        }

        private static SavedStats stats;
        public static async Task CommandUsed() {
            await LoadStats();
            stats.totalCommands++;
            await SaveStats();
        }

        [Command("stats")]
        [Summary("Shows the stats for the discord bot.")]
        public async Task Uptime()
        {
            DateTime now = DateTime.Now;
            DateTime start = Program.BOT_START_TIME;

            TimeSpan uptime = now - start;

            int totalUserCount = 0;
            Program._client.Guilds.ToList().ForEach(x => { totalUserCount += x.MemberCount; });

            int totalChannelCount = 0;
            Program._client.Guilds.ToList().ForEach(x => { totalChannelCount += x.Channels.Count; });

            Process proc = Process.GetCurrentProcess();
            long memorySize = proc.WorkingSet64;
            float memoryMB = MathF.Round(memorySize / 1024 / 1024, 2);

            EmbedHandler embed = new EmbedHandler(Context.User, "Bot Stats");
            embed.AddFieldSecure("Uptime", $"{Math.Round(uptime.TotalHours)} hours, {uptime.Minutes, 0} minutes, {uptime.Seconds} seconds", false);
            embed.AddFieldSecure("Guilds", Program._client.Guilds.Count + $"\n({totalUserCount/Program._client.Guilds.Count}Avg Users/Guild)", true);
            embed.AddFieldSecure("Channels", totalChannelCount, true);
            embed.AddFieldSecure("Users", totalUserCount, false);
            embed.AddFieldSecure("Total Commands", stats.totalCommands, false);
            embed.AddFieldSecure("RAM Usage", memoryMB + " MB", false);

            await embed.SendMessage(Context.Channel);
            await SaveStats();
        }

        [Command("info")]
        [Summary("Gets the info of the bot/server")]
        public async Task Info()
        {
            DateTime now = DateTime.Now;
            DateTime start = Program.BOT_START_TIME;

            TimeSpan uptime = now - start;
            
            const ulong ownerId = 96580514021912576;
            IUser owner = Context.Client.GetUser(ownerId);

            Assembly discordCoreAssembly = Assembly.GetAssembly(typeof(DiscordConfig));
            FileVersionInfo coreInfo = FileVersionInfo.GetVersionInfo(discordCoreAssembly.Location);
            string coreVersion = coreInfo.FileVersion;

            Process process = Process.GetCurrentProcess();

            EmbedHandler embed = new EmbedHandler(Context.User, "Bot Info");
            embed.ThumbnailUrl = Program._client.CurrentUser.GetAvatarUrl();
            embed.AddFieldSecure("Name", "AnimeList", true);
            embed.AddFieldSecure("Developer", owner.Mention + $"\n({owner.Username}#{owner.Discriminator})", true);
            embed.AddFieldSecure("Uptime", $"{Math.Round(uptime.TotalDays)}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s", true);
            embed.AddFieldSecure("Shards", "Bot Shards: " + Context.Client.Shards.Count + "\nGuild Shard Id: " + Context.Client.GetShardFor(Context.Guild).ShardId, true);
            embed.AddFieldSecure("Ping", (Program._client.Latency) + "ms", true);
            embed.AddFieldSecure("Discord.NET Version", coreVersion, false);
            embed.AddFieldSecure("Links",
                "[Invite](https://discordapp.com/api/oauth2/authorize?client_id=515269277553655823&permissions=0&scope=bot) | [Github](https://github.com/ThomasAunvik/AnimeListBot)",
                false
            );
            await embed.SendMessage(Context.Channel);
        }

        [Command("support")]
        public async Task Support()
        {
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);

            EmbedHandler embed = new EmbedHandler(Context.User, "Support");
            embed.AddFieldSecure("Join Support Server", "https://discord.gg/Q9cf46R");
            embed.AddFieldSecure("Contact Command", server.Prefix + "contact");
            await embed.SendMessage(Context.Channel);
        }

        public static async Task SaveStats()
        {
            try
            {
                string json = JsonConvert.SerializeObject(stats);
                await File.WriteAllTextAsync("stats.bot", json);
            }catch(Exception e)
            {
                await Program._logger.LogError(e);
            }
        }

        public static async Task LoadStats()
        {
            try
            {
                if (File.Exists("stats.bot"))
                {
                    string json = await File.ReadAllTextAsync("stats.bot");
                    stats = JsonConvert.DeserializeObject<SavedStats>(json);
                }
            }catch(Exception e)
            {
                await Program._logger.LogError(e);
            }
        }
    }
}
