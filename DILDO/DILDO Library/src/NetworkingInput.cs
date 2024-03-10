using DILDO.client.MVM.model;
using DILDO.server;

namespace DILDO;

public static class NetworkingData
{
    public static User? This { get; private set; }
    public static StateBroker? Broker { get; private set; }

    public static void Init(User user)
    {
        This = user;
        Broker = new();
    }
}

public static class NetworkingInput
{
    public static void Init(User owner) => NetworkingData.Init(owner);
    public static void Switch(NetworkingState state) => StateBroker.Instance.SwitchMode(state);
    
}