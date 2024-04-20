using DILDO.controllers;

namespace DILDO;
public abstract class StateProfile
{
    public abstract NetworkConnector? Connector { get; protected set; }

    public abstract void Launch();
    public abstract void Close();
}
