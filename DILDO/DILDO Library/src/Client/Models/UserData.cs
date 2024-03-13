namespace DILDO.client.models
{
    public class UserData
    {
        public string? UserName         { get; set; } = "NONAME";
        public Guid? UserId             { get; set; } = Guid.NewGuid();
        public NetworkingState State    { get; set; } = NetworkingState.CLIENT;
    }
}
