using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace PVCMod
{
    class MicrophoneCapture
    {
        private WaveInEvent waveIn;

        public event EventHandler<byte[]> AudioDataAvailable;

        public MicrophoneCapture()
        {
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1 kHz, mono
            waveIn.DataAvailable += OnDataAvailable;
        }

        public void Start()
        {
            waveIn.StartRecording();
        }

        public void Stop()
        {
            waveIn.StopRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            // Enviar los datos de audio a través del evento
            AudioDataAvailable?.Invoke(this, e.Buffer);
        }
    }
}
