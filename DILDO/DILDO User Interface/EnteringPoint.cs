using DILDO;
using DILDO.client.MVM.model;

public static class EnteringPoint
{
    [STAThread]
    public static int Main(string[] args)
    {
        Debug.Log("<DGE>Enter \"s\" or \"c\" in order to start Server(s) or Client(c)." +
                     "\n__________________________________________________________");
        string? command;
        while (true)
        {
            command = Console.ReadLine();
            if (command is not null && command == "s" || command == "c")
                break;
        }

        new NetworkingBroker(new User() 
            { 
                UserName = "DEV", 
                NetworkingMode = command == "s"? NetworkingMode.SERVER : NetworkingMode.CLIENT
            });

        Console.ReadLine();
        return 0;
    }
}