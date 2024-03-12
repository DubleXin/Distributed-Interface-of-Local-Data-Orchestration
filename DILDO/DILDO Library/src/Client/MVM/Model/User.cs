namespace DILDO.client.MVM.model
{
    public class User
    {
        public string? UserName { get; set; }               = "NONAME";
        public string? UserId { get; set; }                 = "NONE";
        public NetworkingState NetworkingMode { get; set; }  = NetworkingState.CLIENT;
    }
}
