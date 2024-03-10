using DILDO.client.MVM.model;
using DILDO.server;

namespace DILDO;

public static class NetworkingData
{
    private static bool _initialized = false;

    public static User? This { get; private set; }
    public static StateBroker? Broker { get; private set; }

    public static bool Init(User user)
    {
        if (_initialized)
            return false;

        This = user;
        Broker = new();

        return _initialized = true;
    }
}

public static class NetworkingInput
{
    public static void Init(User owner) 
    {
        if (!NetworkingData.Init(owner))
            Debug.Exception("NetworkingInput.Init(User user) actively refuses", 
                "Already initialized");
    }
    public static void Switch(NetworkingState state) 
    {
        if (StateBroker.Instance is not null)
            StateBroker.Instance.SwitchMode(state);
        else
            Debug.Exception("NetworkingInput.Switch(NetworkingState state) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }

    public static void SetBroadcastCredentials(bool state)
    {
        if(StateBroker.Instance is not null)
            if (StateBroker.Instance.IsServer)
                if (ServerState.Instance.PacketHandler is not null)
                    ServerState.Instance.PacketHandler.BroadcastCredentials = state;
                else
                    Debug.Exception("SetBroadcastCredentials(bool state) NullReferenceException", 
                        "ServerState.Instance.PacketHandler is null, No hints available");
            else
                Debug.Exception("SetBroadcastCredentials(bool state) actively refuses",
                    "Current networking state is not suitable, switch to \"Server\"");
        else
            Debug.Exception("SetBroadcastCredentials(bool state) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }

    public static (Guid guid, string name)[]? GetAvailableServers()
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.IsClient)
            {
                var addresses = StateBroker.Instance.GetAddresses();
                if (addresses is not null && addresses.Count() > 0)
                    return addresses;
                else
                    Debug.Exception("GetAvailableServers() No Result",
                        "No available servers to return, returning null");
            }
            else
                Debug.Exception("GetAvailableServers() actively refuses",
                    "Current networking state is not suitable, switch to \"Client\"");
        else
            Debug.Exception("GetAvailableServers() NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");

        return null;
    }

    public static void ConfigurateServer(int tickRate = 32)
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.IsServer)
                if (ServerState.Instance is not null)
                {
                    if (ServerState.Instance.PacketHandler is not null)
                    {
                        ServerState.Instance.PacketHandler.TickRate = tickRate;
                    }
                    else
                        Debug.Exception("ConfigurateServer(...) NullReferenceException",
                            "ServerState.Instance.PacketHandler is null, No hints available");
                }
                else
                    Debug.Exception("ConfigurateServer(...) NullReferenceException",
                        "ServerState.Instance is null, NetworkingData wasn't initialized");
            else
                Debug.Exception("ConfigurateServer(...) NullReferenceException",
                    "Current networking state is not suitable, switch to \"Server\"");
        else
            Debug.Exception("ConfigurateServer(...) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }

}