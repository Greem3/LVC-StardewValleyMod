using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using StardewValley;

namespace LVCMod
{
    partial class Bot
    {
        private ModEntry Mod { get; set; }

        private DiscordSocketClient DiscordClient { get; set; }
        
        private SocketGuild Guild { get; set; }

        private TaskCompletionSource<bool> IsBotReady { get; set; } = new();

        public Bot(ModEntry modEntry)
        {
            DiscordClient = new DiscordSocketClient();

            Mod = modEntry;
            DiscordClient.Log += OnLog;
            DiscordClient.Ready += OnReady;

            _ = Start();
        }
    }
}