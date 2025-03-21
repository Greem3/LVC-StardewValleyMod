using NAudio.Wave;
using System;

public class AudioLoopback
{
    private WaveInEvent waveIn;  // Para capturar el audio del micrófono
    private WaveOutEvent waveOut; // Para reproducir el audio en los altavoces
    private BufferedWaveProvider waveProvider; // Búfer para almacenar el audio capturado

    public AudioLoopback()
    {
        try
        {
            // Configurar el formato de audio (44.1 kHz, mono, 16 bits)
            WaveFormat waveFormat = new WaveFormat(44100, 1);

            // Inicializar el capturador de audio
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = waveFormat;
            waveIn.DataAvailable += OnAudioDataAvailable;

            // Inicializar el reproductor de audio
            waveOut = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(waveFormat);
            waveProvider.DiscardOnBufferOverflow = true; // Descartar datos si el búfer se llena
            waveOut.Init(waveProvider);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al inicializar AudioLoopback: {ex.Message}");
        }
    }

    public void Start()
    {
        try
        {
            // Iniciar la captura y reproducción de audio
            waveIn.StartRecording();
            waveOut.Play();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al iniciar AudioLoopback: {ex.Message}");
        }
    }

    public void Stop()
    {
        try
        {
            // Detener la captura y reproducción de audio
            waveIn.StopRecording();
            waveOut.Stop();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al detener AudioLoopback: {ex.Message}");
        }
    }

    private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
    {
        try
        {
            // Enviar los datos de audio capturados al búfer de reproducción
            waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en OnAudioDataAvailable: {ex.Message}");
        }
    }
}