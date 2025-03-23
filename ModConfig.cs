using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVCMod
{
    class ModConfig
    {
        public ulong HostDiscordUserId { get; set; } = 0;
        public ulong DiscordServerId { get; set; } = 0;
        public string DiscordBotToken { get; set; } = "";
        public string ITalkChannelName { get; set; } = "";
        public string StardewVoiceChatsCategory { get; set; } = "";
        public Dictionary<ulong, PlayerData> HostSavesData { get; set; } = new();

        public class PlayerData
        {
            public Dictionary<long, ulong> Players { get; set; } = new();
        }
    }
}
