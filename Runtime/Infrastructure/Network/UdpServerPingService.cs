using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using _Project.Application.Configs;
using _Project.Application.Network;
using Cysharp.Threading.Tasks;

namespace _Project.Infrastructure.Network
{
    public class UdpServerPingService : IServerPingService
    {
        private const int Samples = 5;
        private const int ReceiveTimeoutMs = 1500;

        public UniTask<int> PingAsync(ServerRegion region, CancellationToken cancellationToken)
        {
            return UniTask.RunOnThreadPool(() => MeasureBest(region.Address, region.ClientPort + 1), cancellationToken: cancellationToken);
        }

        private static int MeasureBest(string address, int port)
        {
            try
            {
                using UdpClient socket = new UdpClient();
                socket.Client.ReceiveTimeout = ReceiveTimeoutMs;
                socket.Connect(address, port);

                int best = -1;

                for (byte sample = 0; sample < Samples; sample++)
                {
                    int rtt = MeasureOnce(socket, sample);

                    if (rtt >= 0 && (best < 0 || rtt < best))
                    {
                        best = rtt;
                    }
                }

                return best;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int MeasureOnce(UdpClient socket, byte sample)
        {
            try
            {
                byte[] payload = { 0x70, 0x69, 0x6E, 0x67, sample };
                Stopwatch watch = Stopwatch.StartNew();
                socket.Send(payload, payload.Length);

                while (watch.ElapsedMilliseconds < ReceiveTimeoutMs)
                {
                    IPEndPoint sender = null;
                    byte[] response = socket.Receive(ref sender);

                    if (response.Length == payload.Length && response[payload.Length - 1] == sample)
                    {
                        return (int)watch.ElapsedMilliseconds;
                    }
                }

                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
