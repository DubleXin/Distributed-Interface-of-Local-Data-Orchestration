using DILDO.client.models;

namespace DILDO;

public static class NetworkingData
{
    private static bool _initialized = false;

    public static UserData? This { get; private set; }
    public static StateBroker? Broker { get; private set; }

    public static bool Init(UserData user)
    {
        if (_initialized)
            return false;

        This = user;
        Broker = new();

        return _initialized = true;
    }
}
