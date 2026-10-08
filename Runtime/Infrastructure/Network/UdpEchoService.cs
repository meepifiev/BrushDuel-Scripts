using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace _Project.Infrastructure.Network
{
    public class UdpEchoService
    {
        private const int MaxPayloadBytes = 32;

        private UdpClient _socket;
        private Thread _thread;

        public void Start(int port)
        {
            Stop();
            _socket = new UdpClient(port);
            _thread = new Thread(Loop) { IsBackground = true };
            _thread.Start();
        }

        public void Stop()
        {
            if (_socket == null)
            {
                return;
            }

            _socket.Close();
            _socket = null;
            _thread = null;
        }

        private void Loop()
        {
            UdpClient socket = _socket;

            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] payload = socket.Receive(ref sender);

                    if (payload.Length <= MaxPayloadBytes)
                    {
                        socket.Send(payload, payload.Length, sender);
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
