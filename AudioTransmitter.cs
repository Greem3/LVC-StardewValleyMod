using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PVCMod
{
    class AudioTransmitter
    {
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;

        public AudioTransmitter(string ipAddress)
        {
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 24643);
        }

        public AudioTransmitter(string ipAddress, int port)
        {
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        public async Task SendAudioAsync(byte[] audioData)
        {
            await udpClient.SendAsync(audioData, audioData.Length, remoteEndPoint);
        }

        public void Close()
        {
            udpClient.Close();
        }
    }
}
