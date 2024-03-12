using System.Collections.Concurrent;
using System.Net.Sockets;

namespace DILDO.server.models
{
    public class ClientModel : IDisposable
    {
        public UdpClient SendClient { get; private set; }
        public UdpClient ReceiveClient { get; private set; }

        public Action? OnUserConnectEvent;
        public Action? OnUserDisconnectEvent;
        public Action<(Guid, string)[]>? OnServerAddressFound;

        public ConcurrentDictionary<Guid, string>? ServerNames { get; private set; }
        public ConcurrentDictionary<Guid, Guid>? ServerConnectInfo { get; private set; }
        public ConcurrentDictionary<Guid, UDPPacket>? ReceivedPackets { get; private set; }

        public Guid ID { get; private set; }

        public CancellationTokenSource CancellationToken { get; private set; }

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
