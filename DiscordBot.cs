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
        private ModEntry Mod;

        private DiscordSocketClient DiscordClient;
        private SocketGuild Guild;

        private TaskCompletionSource<bool> IsBotReady = new TaskCompletionSource<bool>();

        public DiscordBot(ModEntry modEntry)
        {
            DiscordClient = new DiscordSocketClient();

            Mod = modEntry;
            DiscordClient.Log += OnLog;
            DiscordClient.Ready += OnReady;

            _ = Start();
        }

        private Task OnLog(LogMessage message)
        {
            return Task.CompletedTask;
        }

        public async Task WaitForReady()
        {
            await IsBotReady.Task;
        }

        /// <summary>
        /// Get the Main Player Guild and channels
        /// </summary>
        /// <returns>Task</returns>
        private async Task OnReady()
        {
            Guild = DiscordClient.GetGuild(Mod.Config.HostDiscordServerId);

            if (GetCategoryByName(Mod.Config.StardewVoiceChatsCategory) is null)
                await CreateVoiceChatsCategory();

            if (GetVoiceChannelByName(Mod.Config.ITalkChannelName) is null)
                await CreateVoiceChannel(Mod.Config.ITalkChannelName, false);

            await DiscordClient.SetCustomStatusAsync("Managing conversations");

            IsBotReady.SetResult(true);
        }

        /// <summary>
        /// Start the bot
        /// </summary>
        /// <returns>Task</returns>
        private async Task Start()
        {
            await DiscordClient.LoginAsync(TokenType.Bot, Mod.Config.HostDiscordBotToken);
            await DiscordClient.StartAsync();
        }

        /// <summary>
        /// Get the Discord Id of a Farmer
        /// </summary>
        /// <param name="farmerId">Farmer Id</param>
        /// <returns>ulong</returns>
        private ulong GetDiscordFarmerId(long farmerId)
        {
            return Mod.Config.HostSavesData[Game1.uniqueIDForThisGame]
                .Players[farmerId];
        }

        /// <summary>
        /// Move a Farmer to a voice chat
        /// </summary>
        /// <param name="playerId">Discord User Id</param>
        /// <param name="newLocation">New Location of the User</param>
        /// <returns>Task</returns>
        public async Task MoveToVoice(long playerId, string? newLocation)
        {
            ulong discordUserId = GetDiscordFarmerId(playerId);

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

            SocketVoiceChannel previousVoiceChannel = user.VoiceChannel;

            await user.ModifyAsync(x => x.Channel = voiceChannel);

            if (previousVoiceChannel is null)
                return;

            await DeleteVoiceChannel(
                previousVoiceChannel.Name,
                () =>
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

        /// <summary>
        /// Create a voice channel
        /// </summary>
        /// <param name="channelName">Channel Name</param>
        /// <param name="canTalk">Everyone can talk in the voice chat</param>
        /// <returns>RestVoiceChannel</returns>
        public async Task<RestVoiceChannel> CreateVoiceChannel(string channelName, bool canTalk = true)
        {
            RestVoiceChannel newChannel = await Guild.CreateVoiceChannelAsync(
                channelName, 
                c =>
                {
                    c.CategoryId = GetCategoryByName(Mod.Config.StardewVoiceChatsCategory).Id;

                    if (canTalk)
                        return;

                    c.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(
                        new List<Overwrite>()
                        {
                            new Overwrite(
                                Guild.EveryoneRole.Id,
                                PermissionTarget.Role,
                                new OverwritePermissions(
                                    speak: PermValue.Deny,
                                    sendMessages: PermValue.Deny,
                                    useExternalSounds: PermValue.Deny
                                    )
                            )
                        }
                    );
                }
                );
            
            return newChannel;
        }

        /// <summary>
        /// Create the Main Category for the bot voice channels
        /// </summary>
        /// <returns>RestCategoryChannel</returns>
        public async Task<RestCategoryChannel> CreateVoiceChatsCategory()
        {
            RestCategoryChannel newCategory = await Guild.CreateCategoryChannelAsync(Mod.Config.StardewVoiceChatsCategory);

            return newCategory;
        }

        /// <summary>
        /// Delete a voice channel if the condition is true or null
        /// </summary>
        /// <param name="voiceChannel">Voice Channel To Delete</param>
        /// <param name="beforeCondition">Condition to delete</param>
        /// <returns>Task</returns>
        public async Task DeleteVoiceChannel(SocketVoiceChannel voiceChannel, Func<bool>? beforeCondition = null)
        {
            if (beforeCondition is not null)
            {
                if (!beforeCondition())
                    return;
            }

            await voiceChannel.DeleteAsync();
        }

        /// <summary>
        /// Delete a voice channel searching by name if the condition is true or null
        /// </summary>
        /// <param name="voiceChannelName">Channel Name to Search</param>
        /// <param name="beforeCondition">Condition</param>
        /// <returns>Task</returns>
        public async Task DeleteVoiceChannel(string voiceChannelName, Func<bool>? beforeCondition = null)
        {
            SocketVoiceChannel? voiceChannel = GetVoiceChannelByName(voiceChannelName);

            if (voiceChannel is null)
                return;

            DeleteVoiceChannel(voiceChannel, beforeCondition);
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

        /// <summary>
        /// Delete the voice channels that the bot created for the mod (Except the main one)
        /// </summary>
        /// <returns>Task</returns>
        public async Task CloseVoiceChat()
        {
            SocketVoiceChannel[] voiceChannels = Guild.VoiceChannels
                .Where(c => c.Category.Name == Mod.Config.StardewVoiceChatsCategory)
                .Where(c => c.Name != Mod.Config.ITalkChannelName)
                .ToArray();

            foreach (SocketVoiceChannel voiceChannel in voiceChannels)
            {
                await DeleteVoiceChannel(voiceChannel.Name);
            }
        }

        /// <summary>
        /// Mute a user
        /// </summary>
        /// <param name="playerId">Stardew Valley ID</param>
        /// <returns>Task</returns>
        public async Task MuteUser(long playerId)
        {
            await MuteUser(GetDiscordFarmerId(playerId));
        }

        /// <summary>
        /// Mute a user
        /// </summary>
        /// <param name="userId">Discord ID</param>
        /// <returns>Task</returns>
        public async Task MuteUser(ulong userId)
        {
            SocketGuildUser? user = Guild.GetUser(userId);

            if (user is null)
                return;

            await user.ModifyAsync(u => u.Mute = true);
        }

        /// <summary>
        /// Unmute a user
        /// </summary>
        /// <param name="playerId">Stardew Valley ID</param>
        /// <returns>Task</returns>
        public async Task UnmuteUser(long playerId)
        {
            await UnmuteUser(GetDiscordFarmerId(playerId));
        }

        /// <summary>
        /// Unmute a user
        /// </summary>
        /// <param name="userId">Discord ID</param>
        /// <returns>Task</returns>
        public async Task UnmuteUser(ulong userId)
        {
            SocketGuildUser? user = Guild.GetUser(userId);

            if (user is null)
                return;

            await user.ModifyAsync(u => u.Mute = false);
        }

        /// <summary>
        /// Change the mute state of a user
        /// </summary>
        /// <param name="playerId">Stardew Valley ID</param>
        /// <param name="state">Mute State</param>
        /// <returns>Task</returns>
        public async Task ChangeMuteUserState(long playerId, bool state)
        {
            await ChangeMuteUserState(GetDiscordFarmerId(playerId), state);
        }

        /// <summary>
        /// Change the mute state of a user
        /// </summary>
        /// <param name="playerId">Discord ID</param>
        /// <param name="state">Mute State</param>
        /// <returns>Task</returns>
        public async Task ChangeMuteUserState(ulong playerId, bool state)
        {
            if (state)
            {
                await UnmuteUser(playerId);
                return;
            }

            await MuteUser(playerId);
        }

        /// <summary>
        /// Deaf a user
        /// </summary>
        /// <param name="playerId">Stardew Valley ID</param>
        /// <returns>Task</returns>
        public async Task DeafUser(long playerId)
        {
            await DeafUser(GetDiscordFarmerId(playerId));
        }

        /// <summary>
        /// Deaf a user
        /// </summary>
        /// <param name="userId">Discord ID</param>
        /// <returns>Task</returns>
        public async Task DeafUser(ulong userId)
        {
            SocketGuildUser? user = Guild.GetUser(userId);

            if (user is null)
                return;

            await user.ModifyAsync(u => u.Deaf = true);
        }

        /// <summary>
        /// Undeaf a user
        /// </summary>
        /// <param name="playerId">Stardew Valley ID</param>
        /// <returns>Task</returns>
        public async Task UndeafUser(long playerId)
        {
            await UndeafUser(GetDiscordFarmerId(playerId));
        }

        /// <summary>
        /// Undeaf a user
        /// </summary>
        /// <param name="userId">Discord ID</param>
        /// <returns>Task</returns>
        public async Task UndeafUser(ulong userId)
        {
            SocketGuildUser? user = Guild.GetUser(userId);

            if (user is null)
                return;

            await user.ModifyAsync(u => u.Deaf = false);
        }

        /// <summary>
        /// Change the deaf state of a user
        /// </summary>
        /// <param name="playerId">Stardew Valley ID</param>
        /// <param name="state">Deaf State</param>
        /// <returns>Task</returns>
        public async Task ChangeDeaferUserState(long playerId, bool state)
        {
            await ChangeDeaferUserState(GetDiscordFarmerId(playerId), state);
        }

        /// <summary>
        /// Change the deaf state of a user
        /// </summary>
        /// <param name="playerId">Discord ID</param>
        /// <param name="state">Deaf State</param>
        /// <returns>Task</returns>
        public async Task ChangeDeaferUserState(ulong playerId, bool state)
        {
            if (state)
            {
                await UndeafUser(playerId);
                return;
            }

            await DeafUser(playerId);
        }
    }
}
