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
            Guild = DiscordClient.GetGuild(Config.HostDiscordServerId);

            if (GetCategoryByName(Config.StardewVoiceChatsCategory) is null)
                await CreateVoiceChatsCategory();

            if (GetVoiceChannelByName(Config.ITalkChannelName) is null)
                await CreateVoiceChannel(Config.ITalkChannelName, false);

            await DiscordClient.SetGameAsync("Stardew Valley");
            await DiscordClient.SetCustomStatusAsync("Conversando con todos!");
        }

        private async Task Start()
        {
            await DiscordClient.LoginAsync(TokenType.Bot, Config.HostDiscordBotToken);
            await DiscordClient.StartAsync();
        }

        public async Task MoveToVoice(long playerId, string? newLocation, string? oldLocation = null)
        {
            ulong discordUserId = Config.HostSavesData[Game1.uniqueIDForThisGame]
                .Players[playerId];

            SocketGuildUser? user = Guild.GetUser(discordUserId);

            if (user is null)
                return;

            if (user.VoiceChannel is null)
                return;

            SocketVoiceChannel? voiceChannel = GetVoiceChannelByName(newLocation);

            if (voiceChannel is null)
            {
                RestVoiceChannel channelId = await CreateVoiceChannel(newLocation);
                
                voiceChannel = Guild.GetVoiceChannel(channelId.Id);
            }
            
            await user.ModifyAsync(x => x.Channel = voiceChannel);

            SocketVoiceChannel? previousVoiceChannel = GetVoiceChannelByName(oldLocation);

            if (previousVoiceChannel is null) return;

            await DeleteVoiceChannel(
                oldLocation,
                afterCondition:() =>
                {
                    if (previousVoiceChannel.Users.Count == 0)
                        return true;

                    return false;
                }
            );
        }

        [Obsolete("It is not being used for now.")]
        public async Task SetupBot()
        {
            throw new Exception("Not in use");
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
