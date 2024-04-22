using System.Net.Sockets;
using static StreamUtil;

namespace DILDO.server.models
{
    public class ServerData
    {
        public const ushort DEFAULT_SERVER_LISTEN_PORT = 8085;
        public const ushort DEFAULT_SERVER_RECEIVE_PORT = 8080;
        public const ushort DEFAULT_SERVER_SEND_PORT = 8000;

        public Dictionary<string, TcpClient> Clients;

        public Queue<(string from, string to)> PendingUsernameChanges;
        public List<(string[] mask, Packet packet)> PendingPackets;

        public readonly Guid ServerID;

        public ServerData()
        {
            ServerID = Guid.NewGuid();

            Clients = new Dictionary<string, TcpClient>();
            PendingPackets = new List<(string[], Packet)>();

            PendingUsernameChanges = new Queue<(string from, string to)>();
        }
    }
}
