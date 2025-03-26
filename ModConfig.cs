using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace LVCMod
{
    class ModConfig
    {
        public ulong DiscordUserId { get; set; }

        public ulong HostDiscordServerId { get; set; } = 0;

        public string HostDiscordBotToken { get; set; } = "";

        public string ITalkChannelName { get; set; } = "ITalk";

        public string StardewVoiceChatsCategory { get; set; } = "Stardew Valley LVC";

        public bool DiscordUserMicrophoneActivated { get; set; } = true;

        public bool DiscordUserDeaferActivated { get; set; } = true;

        public bool DeleteVoiceChannels { get; set; } = true;

        public SButton ChangeStateMicrophone { get; set; } = SButton.H;

        public SButton ChangeStateAudio { get; set; } = SButton.J;

        public Dictionary<ulong, PlayerData> HostSavesData { get; set; } = new();

        public class PlayerData
        {
            public Dictionary<long, ulong> Players { get; set; } = new();
        }
    }
}
