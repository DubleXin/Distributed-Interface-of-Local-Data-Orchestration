﻿using DILDO.client.MVM.model;
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
    
    private Queue<NetworkingState>? _queuedStateChanges;
    public byte MaxQueuedStates;

    public StateBroker()
    {
        if (Instance != null)
            return;

        new ServerState();
        new ClientState();

        Instance = this;

        MaxQueuedStates = 8;
        _queuedStateChanges = new(MaxQueuedStates);

        SwitchMode(NetworkingData.This.NetworkingMode);
    }
    public void SwitchMode(NetworkingState mode)
    {
        if (OnStateClosed != null)
        {
            if(_queuedStateChanges.Count < MaxQueuedStates)
                _queuedStateChanges.Enqueue(mode);
            return;
        }

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

            NetworkingData.This.NetworkingMode = mode;

            OnStateClosed = null;

            while (_queuedStateChanges.Count > 0)
                SwitchMode(_queuedStateChanges.Dequeue());
        };

        if (CurrentProfile != null)
            CurrentProfile.Close();
        else
            OnStateClosed.Invoke();
    }

    public (Guid,string)[] GetAddresses() => ClientState.Instance.GetServers();
}
