using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection;

namespace DILDO.client.models
{
    public class ClientModel : IDisposable
    {
        public TcpClient TcpClient { get; set; }

        public UdpClient SendClient { get; private set; }
        public UdpClient ReceiveClient { get; private set; }

        public Action? OnUserConnectEvent;
        public Action? OnUserDisconnectEvent;
        public Action<(Guid, string)[]>? OnServerAddressFound;

        public ConcurrentDictionary<Guid, ServerData>? Servers { get; private set; }

        public Guid ConnectedTo { get; set; }

        public Guid ID { get; private set; }

        private bool _isDisposed;

        public ClientModel()
        {
            TcpClient = new TcpClient();
            SendClient = new UdpClient();
            ReceiveClient = new UdpClient(server.models.ServerModel.DEFAULT_SERVER_SEND_PORT);

            Servers = new();

            ConnectedTo = Guid.Empty;

            ID = Guid.NewGuid();
        }

        public void Dispose() 
        {
            if (_isDisposed)
                return;

            TcpClient?.Dispose();

            SendClient.Dispose();
            ReceiveClient.Dispose();

            Servers.Clear();

            _isDisposed = true;

            Debug.Log<ClientModel>(" <WHI>Client<DRE> Closed and Disposed.");
            StateBroker.Instance.OnStateClosed?.Invoke();
        }
    }
}
