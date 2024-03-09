using DILDO.client.MVM.model;
using DILDO.server;
using DILDO.client;

namespace DILDO;
public enum NetworkingMode
{
    SERVER,
    CLIENT,
}

public class NetworkingBroker
{
    public static NetworkingBroker? Instance { get; private set; }

    private readonly User _owner;

    private readonly Server _server;
    private readonly Client _client;

    public NetProfile CurrentProfile { get; private set; }

    public bool IsServer => CurrentProfile == _server;
    public bool IsClient => CurrentProfile == _client;


    public NetworkingBroker(User owner)
    {
        if (Instance != null)
            return;

        _owner = owner;

        _server = new(owner);
        _client = new(owner);

        Instance = this;

        SwitchMode(owner.NetworkingMode);
    }
    public void SwitchMode(NetworkingMode mode)
    {
        NetProfile next = mode == NetworkingMode.SERVER ? _server : _client;
        if (CurrentProfile == next)
            return;

        CurrentProfile?.Close();
        CurrentProfile = next;
        CurrentProfile.Launch();


        _owner.NetworkingMode = mode;
    }
    public Client UnsafeGetClient() => _client;
    public Server UnsafeGetHost() => _server;

    public (Guid,string)[] GetAddresses() => _client.GetServers();
}