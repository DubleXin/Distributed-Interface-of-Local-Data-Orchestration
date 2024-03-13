using System.Collections.Concurrent;
using System.Net.Sockets;

namespace DILDO.server.models
{
    public class ServerModel : IDisposable
    {
        public const ushort DEFAULT_SERVER_LISTEN_PORT = 8085;
        public const ushort DEFAULT_SERVER_RECEIVE_PORT = 8080;
        public const ushort DEFAULT_SERVER_SEND_PORT = 8000;

        public readonly Guid ServerID;
        /// <summary>
        /// UDP receiveng point. It has asociated port.
        /// To get it without reference of the CLient you can use 
        /// "ServerReceivePort" property.
        /// </summary>
        public UdpClient Client { get; private set; }
        /// <summary>
        /// UDP sending point. So it doesn't have asociated port. 
        /// Use the "ServerSendPort".
        /// </summary>
        public UdpClient Server { get; private set; }
        public TcpListener Listener { get; set; }

        public ConcurrentDictionary<Guid, string> Users { get; private set; }

        private bool _isDisposed;

        public ServerModel()
        {
            ServerID = Guid.NewGuid();
            Users = new();

            Client = new(DEFAULT_SERVER_RECEIVE_PORT);
            Server = new();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            Server.Dispose();
            Client.Dispose();
            Listener?.Stop();

            _isDisposed = true;

            Debug.Log<ServerModel>(" <WHI>Server<DRE> Closed and Disposed.");
            StateBroker.Instance.OnStateClosed?.Invoke();
        }
    }
}
