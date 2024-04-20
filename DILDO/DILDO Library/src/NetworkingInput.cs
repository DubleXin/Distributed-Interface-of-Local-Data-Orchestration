using DILDO.client;
using DILDO.client.models;
using DILDO.server;

namespace DILDO;

public static class NetworkingInput
{
    public static void Init(UserData owner) 
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

    public static void SetPairing(bool state)
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.CurrentProfile.Connector is not null)
            {
                if (state == StateBroker.Instance.CurrentProfile.Connector.IsPairing)
                    return;

                if(state)
                    StateBroker.Instance.CurrentProfile.Connector.StartPairing();
                else
                    StateBroker.Instance.CurrentProfile.Connector.StopPairing();

                Debug.Log<StateBroker>($"<WHI> {(StateBroker.Instance.IsServer? "Server" : "Client")} {(state? "started" : "stopped")} pairing.");
            }
            else
                Debug.Exception("SetPairing(bool state) NullReferenceException",
                    "StateBroker.Instance.CurrentProfile.PacketHandler is null, No hints available");
        else
            Debug.Exception("SetPairing(bool state) NullReferenceException",
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
                    Debug.Exception("GetAvailableServers() NoResultsException",
                        "No available servers to return, returning null");
            }
            else
                Debug.Exception("GetAvailableServers() WrongStateContextException",
                    "Current networking state is not suitable, switch to \"Client\"");
        else
            Debug.Exception("GetAvailableServers() NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");

        return null;
    }

    public static void ConfigurateServer(int broadcastTickRate = 64)
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.IsServer)
                if (ServerState.Instance is not null)
                {
                    if (ServerState.Instance.Connector is not null)
                    {
                        ServerState.Instance.Connector.SetConfig(broadcastTickRate);
                    }
                    else
                        Debug.Exception("ConfigurateServer(...) NullReferenceException",
                            "ServerState.Instance.PacketHandler is null, No hints available");
                }
                else
                    Debug.Exception("ConfigurateServer(...) NullReferenceException",
                        "ServerState.Instance is null, NetworkingData wasn't initialized");
            else
                Debug.Exception("ConfigurateServer(...) WrongStateContextException",
                    "Current networking state is not suitable, switch to \"Server\"");
        else
            Debug.Exception("ConfigurateServer(...) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }

    public static void Connect(int index)
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.IsClient)
            {
                var servers = GetAvailableServers();
                if (servers is not null)
                {
                    int i = -1;
                    foreach (var server in servers)
                    {
                        i++;
                        if (i != index)
                            continue;

                        ClientState.Instance.Core.Connect(server.guid);
                        return;
                    }
                    Debug.Exception("Connect(int index) OutOfBoundsException",
                       "index was out of bounds");
                }
                else
                    Debug.Exception("Connect(int index) NullReferenceException",
                        "Client wasn't initializaed properly");
            }
            else
                Debug.Exception("Connect(int index) WrongStateContextException",
                    "Current networking state is not suitable, switch to \"Client\"");
        else
            Debug.Exception("Connect(int index) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }

    public static void Send(string message) //TODO OBJECT / PACKET
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.IsClient)
                if (ClientState.Instance.Core.ConnectedServer != Guid.Empty)
                    ClientState.Instance.Core.Send(message);
                else
                    Debug.Exception("Send(object message) NotConnectedTCPException",
                        "Client wasn't connected to server, connect to Server");
            else
                Debug.Exception("Send(object message) WrongStateContextException",
                    "Current networking state is not suitable, switch to \"Client\"");
        else
            Debug.Exception("Send(object message) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }

    public static void Disconnect()
    {
        if (StateBroker.Instance is not null)
            if (StateBroker.Instance.IsClient)
                if (ClientState.Instance.Core.ConnectedServer != Guid.Empty)
                    ClientState.Instance.Core.Disconnect();
                else
                    Debug.Exception("Send(object message) NotConnectedTCPException",
                        "Already disconnected");
            else
                Debug.Exception("Send(object message) WrongStateContextException",
                    "Current networking state is not suitable, switch to \"Client\"");
        else
            Debug.Exception("Send(object message) NullReferenceException",
                "StateBroker.Instance is null, NetworkingData wasn't initialized");
    }
}