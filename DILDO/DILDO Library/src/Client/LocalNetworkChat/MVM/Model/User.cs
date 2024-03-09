namespace DILDO.client.MVM.model
{
    public class User
    {
        public string? UserName { get; set; }               = "NONAME";
        public string? UserId { get; set; }                 = "NONE";
        public NetworkingMode NetworkingMode { get; set; }  = NetworkingMode.CLIENT;
    }
}
