using System.Collections.Concurrent;
using System.Net.Sockets;

namespace DILDO.server.models
{
    public class ClientModel : IDisposable
    {
        public UdpClient SendClient { get; set; }
        public UdpClient ReceiveClient { get; set; }

        public Action? OnUserConnectEvent;
        public Action? OnUserDisconnectEvent;
        public Action<(Guid, string)[]>? OnServerAddressFound;

        public ConcurrentDictionary<Guid, string>? ServerNames { get; set; }
        public ConcurrentDictionary<Guid, Guid>? ServerConnectInfo { get; set; }
        public ConcurrentDictionary<Guid, UDPPacket>? ReceivedPackets { get; set; }

        public Guid ID { get; set; }

        public CancellationTokenSource CancellationToken { get; set; }

        public ClientModel()
        {
            SendClient = new UdpClient();
            ReceiveClient = new UdpClient();

            ServerNames = new ConcurrentDictionary<Guid, string>();
            ServerConnectInfo = new ConcurrentDictionary<Guid, Guid>();
            ReceivedPackets = new ConcurrentDictionary<Guid, UDPPacket>();

            ID = Guid.NewGuid();

            CancellationToken = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SendClient?.Dispose();
                ReceiveClient?.Dispose();
                CancellationToken?.Cancel();
                CancellationToken?.Dispose();
            }
        }
    }
}
