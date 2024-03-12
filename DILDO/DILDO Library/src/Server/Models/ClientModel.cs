using System.Collections.Concurrent;
using System.Net.Sockets;

namespace DILDO.server.models
{
    public class ClientModel
    {
        public UdpClient SendClient { get; set; }
        public UdpClient ReceiveCLient { get; set; }

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
            SendClient = new();
            ReceiveCLient = new();

            ServerNames = new();
            ServerConnectInfo = new();
            ReceivedPackets = new();

            ID = Guid.NewGuid();

            CancellationToken = new();
        }
    }
}
