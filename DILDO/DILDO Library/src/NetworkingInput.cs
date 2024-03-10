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
    public static bool Init(User owner) => NetworkingData.Init(owner);
    public static void Switch(NetworkingState state) => StateBroker.Instance.SwitchMode(state);
    
}