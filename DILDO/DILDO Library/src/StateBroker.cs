using DILDO.server;
using DILDO.client;

namespace DILDO;

public class StateBroker
{
    public static StateBroker? Instance { get; private set; }
    public StateProfile? CurrentProfile { get; private set; }

    public bool IsServer => CurrentProfile is ServerState;
    public bool IsClient => CurrentProfile is ClientState;

    public Action? OnStateClosed { get; private set; }

    public StateBroker()
    {
        if (Instance != null)
            return;

        new ServerState();
        new ClientState();

        Instance = this;

        SwitchMode(NetworkingData.This.State);
    }
    public void SwitchMode(NetworkingState mode)
    {
        if (OnStateClosed != null)
            return;

        StateProfile? 
            next = mode == NetworkingState.SERVER ? 
            ServerState.Instance : 
            ClientState.Instance ;

        if (CurrentProfile == next)
            return;

        OnStateClosed = () =>
        {
            CurrentProfile = next;
            CurrentProfile.Launch();

            NetworkingData.This.State = mode;

            OnStateClosed = null;
        };

        if (CurrentProfile != null)
            CurrentProfile.Close();
        else
            OnStateClosed.Invoke();
    }

    public (Guid,string)[] GetAddresses() => ClientState.Instance.GetServers();
}
