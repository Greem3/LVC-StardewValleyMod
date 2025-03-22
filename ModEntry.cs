using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NAudio.Wave;
using NAudio;
using ProximityVoiceChatMod;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using xTile.Layers;
using PVCMod;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ProximityVoiceChatMod
{
    internal sealed class ModEntry : Mod
    {
        private MicrophoneCapture microphoneCapture;
        private AudioTransmitter audioTransmitter;
        private AudioReceiver audioReceiver;
        private AudioLoopback audioLoopback;
        private List<Farmer> players;
        private bool isMicrophoneActive;
        private Texture2D microphoneTexture;
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Inicializando mod...", LogLevel.Info);
            players = new List<Farmer>();

            Config = Helper.ReadConfig<ModConfig>();

            microphoneCapture = new MicrophoneCapture();
            audioReceiver = new AudioReceiver(Config.DefaultPort);
            audioLoopback = new AudioLoopback();

            Monitor.Log("Cargando assets...", LogLevel.Info);
            microphoneTexture = helper.ModContent.Load<Texture2D>("assets/Microphone.png");

            Monitor.Log("Configurando Eventos del Mod...", LogLevel.Info);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
            helper.Events.Display.RenderedHud += OnRenderedHud;

            Monitor.Log("Configurando Eventos del Microfono...", LogLevel.Info);

            microphoneCapture.AudioDataAvailable += OnAudioDataAvailable;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Monitor.Log($"Iniciando transmisor de audio en la IP {Config.ServerIp}", LogLevel.Info);
            audioTransmitter = new AudioTransmitter(Config.ServerIp);

            try
            {
                Task.Run(() => this.audioReceiver.StartReceivingAsync());
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error al inicializar el analizador de audio: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnButtonChanged(object? sender, ButtonsChangedEventArgs e)
        {
            
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Context.IsMultiplayer)
                return;

            if (e.Button == SButton.H)
            {
                this.microphoneCapture.Start();
                isMicrophoneActive = true;
                return;
            }

            try
            {
                if (e.Button == SButton.J)
                {
                    Monitor.Log("Starting audio loopback...", LogLevel.Debug);
                    this.audioLoopback.Start();
                    return;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error al iniciar el bucle de audio: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Context.IsMultiplayer)
                return;

            if (e.Button == SButton.H)
            {
                this.microphoneCapture.Stop();
                isMicrophoneActive = false;
                return;
            }

            if (e.Button == SButton.J)
            {
                Monitor.Log("Stopped audio loopback.", LogLevel.Debug);
                this.audioLoopback.Stop();
                return;
            }
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (isMicrophoneActive)
            {
                SpriteBatch sprite = e.SpriteBatch;

                Vector2 position = new Vector2(Game1.player.Position.X, Game1.player.Position.Y - 25);

                sprite.Draw(microphoneTexture, position, Color.White);
            }
        }

        private void OnAudioDataAvailable(object sender, byte[] audioData)
        {
            // Transmitir audio a todos los jugadores en proximidad
            foreach (var player in players)
            {
                float distance = Vector2.Distance(player.Position, Game1.player.Position);

                if (distance > 5f * Game1.tileSize) // Rango de proximidad
                    return;

                audioTransmitter.SendAudioAsync(audioData).Wait();
            }
        }

        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Context.IsMultiplayer)
                return;

            players.Clear();

            players.AddRange(
                Game1.getOnlineFarmers()
                    .Where(p => p.currentLocation == Game1.player.currentLocation)
                    .Where(p => p != Game1.player)
                );
        }
    }
}
