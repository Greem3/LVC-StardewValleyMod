using Discord.WebSocket;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVCMod
{
    partial class Bot
    {
        /// <summary>
        /// Get the Discord Id of a Farmer
        /// </summary>
        /// <param name="farmerId">Farmer Id</param>
        /// <returns>ulong</returns>
        private ulong GetDiscordFarmerId(long farmerId)
        {
            return Mod.Config.Host.SavesData[Game1.uniqueIDForThisGame]
                .Players[farmerId];
        }

        /// <summary>
        /// Get a voice channel by name
        /// </summary>
        /// <param name="channelName">Channel Name</param>
        /// <returns>SocketVoiceChannel</returns>
        private SocketVoiceChannel? GetVoiceChannelByName(string channelName)
        {
            return Guild.VoiceChannels
                .Where(c => c.Name == channelName).FirstOrDefault();
        }

        /// <summary>
        /// Get a category by name 
        /// </summary>
        /// <param name="categoryName">Category Name</param>
        /// <returns>SocketCategoryChannel</returns>
        private SocketCategoryChannel? GetCategoryByName(string categoryName)
        {
            return Guild.CategoryChannels
                .Where(c => c.Name == categoryName).FirstOrDefault();
        }
    }
}
