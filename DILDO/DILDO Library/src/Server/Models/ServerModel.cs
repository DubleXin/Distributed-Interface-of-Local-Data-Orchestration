using System.Collections.Concurrent;
using System.Net.Sockets;

namespace DILDO.server.models
{
    public class ServerModel : IDisposable
    {
        public const ushort DEFAULT_SERVER_RECEIVE_PORT = 8080;
        public const ushort DEFAULT_SERVER_SEND_PORT = 8000;
        public readonly Guid ServerID;
        /// <summary>
        /// UDP receiveng point. It has asociated port.
        /// To get it without reference of the CLient you can use 
        /// "ServerReceivePort" property.
        /// </summary>
        public UdpClient Client { get; set; }
        /// <summary>
        /// UDP sending point. So it doesn't have asociated port. 
        /// Use the "ServerSendPort".
        /// </summary>
        public UdpClient Server { get; set; }

        private bool _isDisposed;

        public ushort ServerReceivePort { get; private set; }
        public ushort ServerSendPort { get; private set; }
        public ConcurrentDictionary<Guid, string> Users { get; set; }
        public ReaderWriterLockSlim ListenerLock { get; set; }
        public CancellationTokenSource CancellationToken { get; private set; }

        public ServerModel(
            ushort sendPort = DEFAULT_SERVER_SEND_PORT,
            ushort receivePort = DEFAULT_SERVER_RECEIVE_PORT
            )
        {
            ServerID = Guid.NewGuid();
            ServerReceivePort = receivePort;
            ServerSendPort = sendPort;
            Users = new();
            ListenerLock = new();
            CancellationToken = new CancellationTokenSource();

            Client = new(receivePort);
            Server = new();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Server.Close();
                    Client.Close();
                    CancellationToken.Cancel();
                }
                _isDisposed = true;
            }
        }
    }
}
