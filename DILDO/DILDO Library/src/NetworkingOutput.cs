using System.Collections.Concurrent;
using static StreamUtil;

#if UNITY_5_OR_NEWER
using UnityEngine.LowLevel;
#endif

namespace DILDO;

public static class NetworkingOutput
{
#if UNITY_5_OR_NEWER
    private static bool _initialised = false;

    public delegate void OnPacketReceivedHandler(Packet packet);
    public static OnPacketReceivedHandler? OnPacketReceived;
#endif

    public delegate void OnPacketReceivedAsyncHandler();
    public static OnPacketReceivedAsyncHandler? OnPacketReceivedAsync;

    private static readonly ConcurrentQueue<Packet> _pendingPackets = new();

    public static void EnqueuePacket(Packet packet)
    { 
        _pendingPackets.Enqueue(packet);
        OnPacketReceivedAsync?.Invoke();
    }
    public static Packet? TryDequeuePacket()
    {
        if (_pendingPackets.Count == 0)
            return null;

        _pendingPackets.TryDequeue(out var packet);
        return packet;
    }
    public static List<Packet> DequeueAllPackets()
    {
        var result = _pendingPackets.ToList();
        _pendingPackets.Clear();
        return result;
    }

#if UNITY_5_OR_NEWER
    public static bool SubscribeToSyncThread()
    {
        if (_initialised)
            return false;

        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        playerLoop.subSystemList[5].subSystemList += DispatchPackets;
        PlayerLoop.SetPlayerLoop(playerLoop);

        _initialised = true;
        return true;
    }

    public static void DispatchPackets()
    {
        var packets = DequeueAllPackets();
        foreach (var packet in packets)
            OnPacketReceived?.Invoke(packet);
    }
#endif

}