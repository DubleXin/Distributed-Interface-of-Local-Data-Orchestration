namespace DILDO.server.models.connectors
{
    public enum OpCode
    {
        BroadcastStringMessage = 0,
        UserConnectMessage = 1,
        StringMessage = 5,
        DisconnectMessage = 10,
        None = 100
    }
    public class Packet
    {
        public OpCode OpCode { get; set; }
        public byte[]? Data { get; set; }
    }
}
