using NAudio.Wave;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PVCMod
{
    class AudioReceiver
    {
        private UdpClient udpClient;
        private WaveOutEvent waveOut;
        private BufferedWaveProvider waveProvider;

        public AudioReceiver()
        {
            udpClient = new UdpClient(24643);
            waveOut = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
            waveOut.Init(waveProvider);
            ChangeVolume();
        }

        public AudioReceiver(int port)
        {
            udpClient = new UdpClient(port);
            waveOut = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
            waveOut.Init(waveProvider);
            ChangeVolume();
        }

        public async Task StartReceivingAsync()
        {
            while (true)
            {
                var result = await udpClient.ReceiveAsync();
                waveProvider.AddSamples(result.Buffer, 0, result.Buffer.Length);
            }
        }

        public void Close()
        {
            udpClient.Close();
            waveOut.Stop();
        }

        public void ChangeVolume()
        {
            float volume = Game1.options.soundVolumeLevel;

            waveOut.Volume = volume;
        }
    }
}
