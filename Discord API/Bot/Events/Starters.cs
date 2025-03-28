using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVCMod
{
    partial class Bot
    {
        private Task OnLog(LogMessage message)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the Main Player Guild and channels
        /// </summary>
        /// <returns>Task</returns>
        private async Task OnReady()
        {
            Guild = DiscordClient.GetGuild(Mod.Config.Host.DiscordGuildId);

            if (GetCategoryByName(Mod.Config.Bot.VoiceChatsCategoryName) is null)
                await CreateVoiceChatsCategory();

            if (GetVoiceChannelByName(Mod.Config.Bot.MainVoiceChatName) is null)
                await CreateVoiceChannel(Mod.Config.Bot.MainVoiceChatName, false);

            await DiscordClient.SetCustomStatusAsync("Managing conversations");

            IsBotReady.SetResult(true);

            Debug.WriteLine($"{Mod.ModManifest.UniqueID}, {DiscordClient.ConnectionState}, {Guild.Id}, {IsBotReady}");
        }

        public async Task WaitForReady()
        {
            await IsBotReady.Task;
        }

        /// <summary>
        /// Start the bot
        /// </summary>
        /// <returns>Task</returns>
        private async Task Start()
        {
            await DiscordClient.LoginAsync(TokenType.Bot, Mod.Config.Bot.Token);
            await DiscordClient.StartAsync();
        }
    }
}
