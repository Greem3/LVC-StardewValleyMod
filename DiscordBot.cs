using System;
using System.Diagnostics;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using StardewValley;

namespace LVCMod
{
    class DiscordBot
    {
        private ModConfig Config;

        private DiscordSocketClient DiscordClient;
        private SocketGuild Guild;

        public DiscordBot(ModConfig config)
        {
            DiscordClient = new DiscordSocketClient();

            Config = config;
            DiscordClient.Log += OnLog;
            DiscordClient.Ready += OnReady;

            _ = Start();
        }

        private Task OnLog(LogMessage message)
        {
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            DiscordClient.SetGameAsync("Stardew Valley");
            DiscordClient.SetCustomStatusAsync("Conversando con todos!");

            Guild = DiscordClient.GetGuild(Config.DiscordServerId);

            Debug.WriteLine($"Guild: {Guild.Id}");

            if (GetCategoryByName(Config.StardewVoiceChatsCategory) is null)
                await CreateVoiceChatsCategory();

            if (GetVoiceChannelByName(Config.ITalkChannelName) is null)
                await CreateVoiceChannel(Config.ITalkChannelName, false);
        }

        private async Task Start()
        {
            await DiscordClient.LoginAsync(TokenType.Bot, Config.DiscordBotToken);
            await DiscordClient.StartAsync();
        }

        public async Task MoveToVoice(Farmer player, string? previousLocation = null)
        {
            ulong discordUserId = Config.HostSavesData[Game1.uniqueIDForThisGame]
                .Players[Convert.ToInt64(player.UniqueMultiplayerID)];

            Debug.WriteLine($"Warped Player Discord Id: {discordUserId}");

            SocketGuildUser? user = Guild.GetUser(discordUserId);

            if (user is null)
                return;

            if (user.VoiceChannel is null)
                return;

            string currentLocation = player.currentLocation.Name;

            SocketVoiceChannel? voiceChannel = GetVoiceChannelByName(currentLocation);

            if (voiceChannel is null)
            {
                RestVoiceChannel channelId = await CreateVoiceChannel(currentLocation);
                
                voiceChannel = Guild.GetVoiceChannel(channelId.Id);
            }
            
            await user.ModifyAsync(x => x.Channel = voiceChannel);

            await DeleteVoiceChannel(
                previousLocation,
                () =>
                {
                    SocketVoiceChannel? previousVoiceChannel = GetVoiceChannelByName(previousLocation);

                    if (previousVoiceChannel is null)
                        return false;

                    if (previousVoiceChannel.Users.Count > 0)
                        return false;

                    return true;
                }
            );
        }

        public async Task SetupBot()
        {
            SocketTextChannel? textChannel = Guild.DefaultChannel;
        }

        public async Task<RestVoiceChannel> CreateVoiceChannel(string channelName, bool canTalk = true)
        {
            RestVoiceChannel newChannel = await Guild.CreateVoiceChannelAsync(
                channelName, 
                c =>
                {
                    c.CategoryId = GetCategoryByName(Config.StardewVoiceChatsCategory).Id;

                    if (canTalk)
                        return;

                    c.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(
                        new List<Overwrite>()
                        {
                            new Overwrite(
                                Guild.EveryoneRole.Id,
                                PermissionTarget.Role,
                                new OverwritePermissions(speak: PermValue.Deny)
                            )
                        }
                    );
                }
                );
            
            return newChannel;
        }

        public async Task<RestCategoryChannel> CreateVoiceChatsCategory()
        {
            RestCategoryChannel newCategory = await Guild.CreateCategoryChannelAsync(Config.StardewVoiceChatsCategory);

            return newCategory;
        }

        public async Task DeleteVoiceChannel(string currentLocation, Func<bool>? beforeCondition = null, Func<bool>? afterCondition = null)
        {
            if (beforeCondition is not null)
            {
                if (!beforeCondition())
                    return;
            }

            SocketVoiceChannel? voiceChannel = GetVoiceChannelByName(currentLocation);

            if (voiceChannel is null)
                return;

            if (afterCondition is not null)
            {
                if (!afterCondition())
                    return;
            }

            await voiceChannel.DeleteAsync();
        }

        private SocketVoiceChannel? GetVoiceChannelByName(string channelName)
        {
            return Guild.VoiceChannels
                .Where(c => c.Name == channelName).FirstOrDefault();
        }

        private SocketCategoryChannel? GetCategoryByName(string categoryName)
        {
            return Guild.CategoryChannels
                .Where(c => c.Name == categoryName).FirstOrDefault();
        }

        public async Task UpdateConfig(ModConfig newConfig)
        {
            Config = newConfig;
        }
    }
}
