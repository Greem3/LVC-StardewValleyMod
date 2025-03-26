using System;
using System.Diagnostics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace LVCMod
{
    internal sealed class ModEntry : Mod
    {
        public ModConfig Config;
        private DiscordBot? Bot;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private async void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMultiplayer)
                return;

            Config = Helper.ReadConfig<ModConfig>();

            if (Config.DiscordUserId == 0)
            {
                Monitor.Log("You have to put your discord id!\r\n\r\nVoice chat won't work for you.", LogLevel.Error);
                return;
            }

            LoadEvents();

            if (Context.IsMainPlayer)
            {
                if (Config.HostDiscordServerId == 0)
                {
                    Monitor.Log(
                        "As the main player you have to put your guild ID!\r\n\r\nVoice chat won't work.",
                        LogLevel.Error
                    );
                    UnloadEvents();
                    return;
                }

                if (Config.HostDiscordBotToken == "")
                {
                    Monitor.Log(
                        "As the main player, you have to enter your bot's token!\r\n\r\nVoice chat won't work.",
                        LogLevel.Error
                    );
                    UnloadEvents();
                    return;
                }

                Bot = new DiscordBot(this);
                await Bot.WaitForReady();

                ulong saveId = Game1.uniqueIDForThisGame;

                Config.HostSavesData.TryAdd(saveId, new ModConfig.PlayerData());

                Config.HostSavesData[saveId].Players[Game1.MasterPlayer.UniqueMultiplayerID] = Config.DiscordUserId;
                await Bot.ChangeMuteUserState(Game1.MasterPlayer.UniqueMultiplayerID, Config.DiscordUserMicrophoneActivated);

                SaveConfig();
                return;
            }

            Helper.Multiplayer.SendMessage(
                Config,
                "LVCJoined",
                new[] {ModManifest.UniqueID},
                new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
        }

        private async void OnMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID)
                return;

            if (!Context.IsMainPlayer)
                return;

            if (e.Type == "LVCJoined")
            {
                ModConfig clientConfig = e.ReadAs<ModConfig>();

                Config.HostSavesData[Game1.uniqueIDForThisGame].Players[e.FromPlayerID] = clientConfig.DiscordUserId;

                await Bot.ChangeMuteUserState(clientConfig.DiscordUserId, clientConfig.DiscordUserMicrophoneActivated);

                SaveConfig();

                return;
            }

            if (e.Type == "LVCPlayerWarped")
            {
                var data = e.ReadAs<(long PlayerId, string NewLocation)>();

                await Bot.MoveToVoice(data.PlayerId, data.NewLocation);

                return;
            }

            if (e.Type == "LVCChangeMicrophoneState")
            {
                var playerInfo = e.ReadAs<(long Id, bool State)>();

                await Bot.ChangeMuteUserState(playerInfo.Id, playerInfo.State);
                return;
            }

            if (e.Type == "LVCChangeDeaferState")
            {
                var playerInfo = e.ReadAs<(long Id, bool State)>();

                await Bot.ChangeDeaferUserState(playerInfo.Id, playerInfo.State);
                return;
            }
        }

        private async void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            UnloadEvents();

            if (!Context.IsMainPlayer)
                return;

            if (Config.DeleteVoiceChannels)
                await Bot.CloseVoiceChat();
        }

        private async void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                await Bot.MoveToVoice(e.Player.UniqueMultiplayerID, e.NewLocation.Name);
                return;
            }

            (long, string) message = (e.Player.UniqueMultiplayerID, e.NewLocation.Name);

            Helper.Multiplayer.SendMessage(
                message,
                "LVCPlayerWarped",
                new[] { ModManifest.UniqueID },
                new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
        }

        private async void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {

            if (e.Button == Config.ChangeStateMicrophone)
            {
                Config.DiscordUserMicrophoneActivated = !Config.DiscordUserMicrophoneActivated;
                SaveConfig();

                (long Id, bool State) userInfo = (Game1.player.UniqueMultiplayerID, Config.DiscordUserMicrophoneActivated);

                if (Context.IsMainPlayer)
                {
                    await Bot.ChangeMuteUserState(userInfo.Id, userInfo.State);
                    return;
                }

                Helper.Multiplayer.SendMessage(
                    userInfo,
                    "LVCChangeMicrophoneState",
                    new[] { ModManifest.UniqueID },
                    new [] { Game1.MasterPlayer.UniqueMultiplayerID }
                    );

                return;
            }

            if (e.Button == Config.ChangeStateAudio)
            {
                Config.DiscordUserDeaferActivated = !Config.DiscordUserDeaferActivated;
                SaveConfig();

                (long Id, bool State) userInfo = (Game1.player.UniqueMultiplayerID, Config.DiscordUserDeaferActivated);

                if (Context.IsMainPlayer)
                {
                    await Bot.ChangeDeaferUserState(userInfo.Id, userInfo.State);
                    return;
                }

                Helper.Multiplayer.SendMessage(
                    userInfo,
                    "LVCChangeDeaferState",
                    new[] { ModManifest.UniqueID },
                    new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
                return;
            }
        }

        private void LoadEvents()
        {
            Helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void UnloadEvents()
        {
            Helper.Events.Multiplayer.ModMessageReceived -= OnMessageReceived;
            Helper.Events.Player.Warped -= OnWarped;
            Helper.Events.GameLoop.ReturnedToTitle -= OnReturnedToTitle;
            Helper.Events.Input.ButtonPressed -= OnButtonPressed;
        }

        private void SaveConfig()
        {
            Helper.WriteConfig(Config);
        }
    }
}
