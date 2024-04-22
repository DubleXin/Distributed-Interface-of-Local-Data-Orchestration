using System.Collections.Concurrent;

namespace DILDO.client.models
{
    public class ClientData
    {
        public Action<(Guid, string)[]>? OnServerAddressFound;
        public ConcurrentDictionary<Guid, ServerData>? Servers { get; private set; }

        public Guid ConnectedTo { get; set; }
        public Guid ID { get; private set; }

        public ClientData()
        {
            Servers = new();

            ConnectedTo = Guid.Empty;
            ID = Guid.NewGuid();
        }
    }
}
