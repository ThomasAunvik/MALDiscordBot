﻿using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AnimeListBot.Handler;

namespace AnimeListBot.Modules
{
    public class Administrator : ModuleBase<ICommandContext>
    {
        [Command("stop")]
        public async Task StopBot()
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if(Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                embed.Title = "Stopping Bot.";
                await embed.SendMessage(Context.Channel);

                Program.stop = true;
                await Program._client.StopAsync();
            }
            else{
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }

        [Command("errortest")]
        public async Task SendErrorTest([Remainder]string message)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                embed.Title = "Sent fake error.";
                await embed.SendMessage(Context.Channel);
                throw new Exception(message);
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }

        [Command("setgamestatus")]
        [Summary(
            "Setting Game Status for the bot" +
            "\n    Playing = 0," +
            "\n    Streaming = 1," +
            "\n    Listening = 2," +
            "\n    Watching = 3"
        )]
        public async Task SetGameStatus(Discord.ActivityType activityType, string gameMessage)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Set game status to: " + Enum.GetName(typeof(Discord.ActivityType), activityType) + " " + gameMessage);

            if (Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                await Program._client.SetGameAsync(gameMessage, null, activityType);
                await embed.SendMessage(Context.Channel);
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }

        [Command("setonlinestatus")]
        [Summary(
            "Setting User Status for the Bot" +
            "\n    Status:" +
            "\n    Offline = 0," +
            "\n    Online = 1," +
            "\n    Idle = 2," +
            "\n    AFK = 3," +
            "\n    DoNotDisturb = 4," +
            "\n    Invisible = 5"
        )]
        public async Task SetOnlineStatus(Discord.UserStatus status)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Set online status to: " + Enum.GetName(typeof(Discord.UserStatus), status));

            if (Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                await Program._client.SetStatusAsync(status);
                await embed.SendMessage(Context.Channel);
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }
    }
}
