using System;
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
            Config = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
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

                Monitor.Log("Guardando nuevo mundo...", LogLevel.Info);
                Config.HostSavesData.Add(saveId, new ModConfig.PlayerData());
                Config.HostSavesData[saveId].Players.Add(Game1.MasterPlayer.UniqueMultiplayerID, Config.HostDiscordUserId);

                SaveConfig();
            }

            Helper.Multiplayer.SendMessage(
                Config,
                "PVCJoined",
                new[] {ModManifest.UniqueID},
                new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
        }

        private async void OnMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID)
                return;

            if (e.Type == "PVCJoined")
            {
                ModConfig clientConfig = e.ReadAs<ModConfig>();

                if (Config.HostSavesData[Game1.uniqueIDForThisGame].Players.ContainsKey(e.FromPlayerID))
                    return;

                Config.HostSavesData[Game1.uniqueIDForThisGame].Players.Add(e.FromPlayerID, clientConfig.HostDiscordUserId);

                SaveConfig();
            }

            if (e.Type == "PVCPlayerWarped")
            {
                var data = e.ReadAs<(Farmer Player, GameLocation beforeLocation)>();
                
                await Bot.MoveToVoice(data.Player, data.beforeLocation.Name);
            }

        }

        private async void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.Player == Game1.MasterPlayer)
            {
                await Bot.MoveToVoice(e.Player, e.OldLocation.Name);
                return;
            }

            (Farmer, GameLocation) message = (e.Player, e.OldLocation);

            Helper.Multiplayer.SendMessage(
                message,
                "PVCPlayerWarped",
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
