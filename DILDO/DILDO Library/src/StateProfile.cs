using DILDO.server.models;

namespace DILDO;
public abstract class StateProfile
{
    public StateProfile()
    {
        OnPacketReceived = ((packet) => { });
    }
    public static Action<UDPPacket>? OnPacketReceived { get; set; }
    public void RaisePacketReceiver(UDPPacket packet, string callbackMessage)
    {
        if (OnPacketReceived != null)
            OnPacketReceived.Invoke(packet);
        else
            Debug.Log<StateProfile>(callbackMessage);

    }
    public void SubscribeToPacketReceiver(Action<UDPPacket> action)
    {
        OnPacketReceived += action;
    }

    public abstract void Launch();
    public abstract void Close();
}
