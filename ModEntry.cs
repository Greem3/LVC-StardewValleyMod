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

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Inicializando mod...", LogLevel.Info);
            players = new List<Farmer>();
            microphoneCapture = new MicrophoneCapture();
            audioTransmitter = new AudioTransmitter("127.0.0.1", 5000);
            audioReceiver = new AudioReceiver(5000);
            audioLoopback = new AudioLoopback();

            Monitor.Log("Cargando texturas...", LogLevel.Info);
            microphoneTexture = helper.ModContent.Load<Texture2D>("assets/MicrophoneNew.png");

            Monitor.Log("Configurando Eventos...", LogLevel.Info);
            microphoneCapture.AudioDataAvailable += this.OnAudioDataAvailable;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

            try
            {
                Monitor.Log("Starting audio receiver...", LogLevel.Info);
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

                Vector2 position = new Vector2(25, 25);

                sprite.Draw(microphoneTexture, position, Color.White);
            }
        }

        private void OnAudioDataAvailable(object sender, byte[] audioData)
        {
            // Transmitir audio a todos los jugadores en proximidad
            foreach (var player in players)
            {
                if (player != Game1.player)
                    audioTransmitter.SendAudioAsync(audioData).Wait();
            }
        }

        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Context.IsMultiplayer)
                return;

            // Obtener la lista de jugadores
            players.Clear();
            foreach (var player in Game1.getOnlineFarmers())
            {
                players.Add(player);
            }

            // Calcular distancias y transmitir audio
            foreach (var player in players)
            {
                if (player == Game1.player)
                    continue;

                float distance = Vector2.Distance(player.Position, Game1.player.Position);

                if (distance <= 5f * Game1.tileSize) // Rango de proximidad
                {
                    // Transmitir audio a este jugador
                    // (ya se maneja en OnAudioDataAvailable)
                }
            }
        }
    }
}
