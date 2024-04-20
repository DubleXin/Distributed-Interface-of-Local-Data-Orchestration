using DILDO.controllers;
using Client.Controllers;
using DILDO.client.models;

namespace DILDO.client;

public class ClientState : StateProfile
{
    #region FIELDS
    
    public static ClientState? Instance { get; private set; }

    public override NetworkConnector? Connector
    {
        get => _connector;
        protected set => _connector = value as ClientConnector;
    }
    private ClientConnector? _connector;

    public ClientData? Data { get; private set; }
    public ClientCore? Core { get; private set; }

    #endregion

    #region CONSTRUCTOR

    public ClientState() : base()
    {
        Instance = this;
        
        Connector = new ClientConnector();
        Core      = new ClientCore();
    }

    #endregion

    #region LIFE CYCLE

    public override void Launch()
    {
        Data = new ClientData();

        Core.Launch();
        Connector.Launch();

        Debug.Log<ClientState>($" <WHI>Client <GRE>Started.");
    }
    public override void Close()
    {
        Core.Close();
        Connector.Close();

        Debug.Log<ServerData>(" <WHI>Client<DRE> Closed.");
        StateBroker.Instance.OnStateClosed?.Invoke();
    }

    #endregion

    #region CONTROLS
    
    public (Guid, string)[] GetServers()
    {
        var servers = Data.Servers.ToArray();
        List<(Guid, string)> listToInvoke = new();
        
        foreach (var server in servers)
            listToInvoke.Add((server.Key, server.Value.ServerName));

        return listToInvoke.ToArray();
    }

    #endregion
}
