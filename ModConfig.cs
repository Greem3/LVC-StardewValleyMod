using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVCMod
{
    class ModConfig
    {
        public ulong DiscordUserId { get; set; } = 0;
        public ulong HostDiscordServerId { get; set; } = 0;
        public string HostDiscordBotToken { get; set; } = "";
        public string ITalkChannelName { get; set; } = "";
        public string StardewVoiceChatsCategory { get; set; } = "";
        public Dictionary<ulong, PlayerData> HostSavesData { get; set; } = new();

        public class PlayerData
        {
            public Dictionary<long, ulong> Players { get; set; } = new();
        }
    }
}
