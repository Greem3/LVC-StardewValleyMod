using System;
using System.Diagnostics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace LVCMod
{
    internal sealed class ModEntry : Mod
    {
        private ModConfig Config;
        private DiscordBot? Bot;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Context.IsMultiplayer)
                return;

            Helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;
            Helper.Events.Player.Warped += OnWarped;

            if (Context.IsMainPlayer)
            {
                Bot = new DiscordBot(Config);

                ulong saveId = Game1.uniqueIDForThisGame;

                if (Config.HostSavesData.ContainsKey(saveId))
                    return;

                Config.HostSavesData.Add(saveId, new ModConfig.PlayerData());
                Config.HostSavesData[saveId].Players.Add(Game1.MasterPlayer.UniqueMultiplayerID, Config.DiscordUserId);

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

                if (Config.HostSavesData[Game1.uniqueIDForThisGame].Players.ContainsKey(e.FromPlayerID))
                    return;

                Config.HostSavesData[Game1.uniqueIDForThisGame].Players.Add(e.FromPlayerID, clientConfig.DiscordUserId);

                SaveConfig();

                return;
            }

            if (e.Type == "LVCPlayerWarped")
            {
                var data = e.ReadAs<(long PlayerName, string newLocation, string oldLocation)>();

                await Bot.MoveToVoice(data.PlayerName, data.newLocation, data.oldLocation);

                return;
            }

        }

        private async void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                await Bot.MoveToVoice(e.Player.UniqueMultiplayerID, e.NewLocation.Name, e.OldLocation.Name);
                return;
            }

            (long, string, string) message = (e.Player.UniqueMultiplayerID, e.NewLocation.Name ,e.OldLocation.Name);

            Helper.Multiplayer.SendMessage(
                message,
                "LVCPlayerWarped",
                new[] { ModManifest.UniqueID },
                new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
        }

        private void SaveConfig()
        {
            Helper.WriteConfig(Config);
            Bot.UpdateConfig(Config);
        }
    }
}
