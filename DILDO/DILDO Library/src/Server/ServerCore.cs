using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DILDO.server;

public class ServerCore
{
    private Task? _current;
    private bool _active;
    public TcpListener Listener { get; private set; }

    public ServerCore()
    {
        _active = false;
        Listener = new(IPAddress.Any, 0);
    }

    public void Launch()
    {
        Listener.Start();
        _active = true;

        _current = Task.Run(LifeCycle);
    }
    public void Close()
    {
        _active = false;
        _current.Wait();

        Listener.Stop();
    }

    private void LifeCycle()
    {
        while (_active)
        {
            ProcessNewClients();
            ProcessExistingClients();
            CheckFaultyClients();

            Thread.Sleep(100);
        }
    }

    private void ProcessNewClients()
    {
        while (Listener.Pending())
        {
            int userID = ServerState.Instance.Data.Clients.Count;
            string username = $"user{userID}";

            while (ServerState.Instance.Data.Clients.ContainsKey(username))
                username = $"user{++userID}";

            ServerState.Instance.Data.Clients.Add(username, Listener.AcceptTcpClient());

            ServerState.Instance.Data.PendingMessages.Add(
                (new string[] { username }, 
                Encoding.UTF8.GetBytes($"You have connected to the server with username: {username}.")));
            
            ServerState.Instance.Data.PendingMessages.Add(
                (GetAllApartFrom(username), 
                Encoding.UTF8.GetBytes($"{username} connected to the server.")));

            Debug.Log<ServerCore>($" \n<WHI>{username} connected.");
        }
    }
    private void CheckFaultyClients()
    {
        List<string> faultyClientsBuffer = new List<string>();
        foreach (var client in ServerState.Instance.Data.Clients)
        {
            if (!client.Value.Connected)
                faultyClientsBuffer.Add(client.Key);
        }
        foreach (var client in faultyClientsBuffer)
        {
            ServerState.Instance.Data.Clients.Remove(client);
            ServerState.Instance.Data.PendingMessages.Add(
                (null, Encoding.UTF8.GetBytes($"{client} has disconnected from the server.")));
        }
    }
    private string[] GetAllApartFrom(string key)
    {
        string[] result = new string[ServerState.Instance.Data.Clients.Count - 1];
        int counter = 0;
        foreach (var clientName in ServerState.Instance.Data.Clients.Keys)
        {
            if (clientName == key)
                continue;

            result[counter] = clientName;
            counter++;
        }
        return result;
    }
    private void ProcessExistingClients()
    {
        while (ServerState.Instance.Data.PendingUsernameChanges.Count > 0)
        {
            var (from, to) = ServerState.Instance.Data.PendingUsernameChanges.Dequeue();

            var client = ServerState.Instance.Data.Clients[from];
            ServerState.Instance.Data.Clients.Remove(from);
            ServerState.Instance.Data.Clients.Add(to, client);
        }

        foreach (var user in ServerState.Instance.Data.Clients)
        {
            if (user.Value.Available == 0) continue;
            NetworkStream stream = user.Value.GetStream();
            string decodedMessage = Encoding.UTF8.GetString(StreamUtil.Read(stream));

            if (TryParsingCommand(decodedMessage, user.Key))
                continue;

            string message = $"[{DateTime.Now}] {user.Key} : {decodedMessage}";
            byte[] encodedMessage = Encoding.UTF8.GetBytes(message);
            StreamUtil.Write(stream, encodedMessage);

            ServerState.Instance.Data.PendingMessages.Add((GetAllApartFrom(user.Key), encodedMessage));

        }
        if (ServerState.Instance.Data.PendingMessages.Count > 0)
        {
            foreach (var user in ServerState.Instance.Data.Clients)
            foreach (var entry in ServerState.Instance.Data.PendingMessages)
            {
                if (entry.mask != null)
                {
                    bool isValidAddressant = false;
                    foreach (var key in entry.mask)
                    {
                        if (key == user.Key)
                            isValidAddressant = true;
                    }
                    if (!isValidAddressant)
                        continue;
                }

                NetworkStream stream = user.Value.GetStream();
                StreamUtil.Write(stream, entry.encodedMessage);
            }
        }
        ServerState.Instance.Data.PendingMessages.Clear();
    }
    private bool TryParsingCommand(string message, string sender)
    {
        bool parsed = false;
        if (message[0] == '/')
        {
            var potentialFullCommand = message.Substring(1);
            var commandParts = potentialFullCommand.Split(' ');
            parsed = true;

            switch (commandParts[0])
            {
                case "help":
                    ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                        Encoding.UTF8.GetBytes("List of available commands:\n\t/help\n\t/setname or /sn\n\t/list\n\t/whisper or /w")));
                    break;
                case "setname":
                    if (!string.IsNullOrEmpty(commandParts[1]))
                    {
                        string username = commandParts[1].ToLower();
                        if (ServerState.Instance.Data.Clients.ContainsKey(username))
                        {
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("You can not use this name, there is already a user with this name.")));
                        }
                        else
                        {
                            ServerState.Instance.Data.PendingUsernameChanges.Enqueue((sender, username));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("Successfully changed name.")));
                        }
                    }
                    else
                    {
                        ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                            Encoding.UTF8.GetBytes("You can not use this name, it is empty.")));
                    }
                    break;
                case "sn":
                    if (!string.IsNullOrEmpty(commandParts[1]))
                    {
                        string username = commandParts[1].ToLower();
                        if (ServerState.Instance.Data.Clients.ContainsKey(username))
                        {
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("You can not use this name, there is already a user with this name.")));
                        }
                        else
                        {
                            ServerState.Instance.Data.PendingUsernameChanges.Enqueue((sender, username));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("Successfully changed name.")));
                        }
                    }
                    else
                    {
                        ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                            Encoding.UTF8.GetBytes("You can not use this name, it is empty.")));
                    }
                    break;
                case "list":
                    string reply = "List of connected users:\n";
                    foreach (var client in ServerState.Instance.Data.Clients.Keys)
                        reply += $"\t{client}\n";
                    ServerState.Instance.Data.PendingMessages.Add((new string[] { sender }, Encoding.UTF8.GetBytes(reply)));
                    break;
                case "whisper":
                    {
                        string targetName = commandParts[1];
                        if (string.IsNullOrEmpty(targetName))
                        {
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("Couldn't parse the target nickname, it was empty")));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                  Encoding.UTF8.GetBytes("Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'")));
                        }
                        else if (!ServerState.Instance.Data.Clients.ContainsKey(targetName))
                        {
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("There is no such player")));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                  Encoding.UTF8.GetBytes("Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'")));
                        }
                        else
                        {
                            var restoredMessage = potentialFullCommand.Substring(commandParts[0].Length + commandParts[1].Length + 2);
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { targetName },
                                Encoding.UTF8.GetBytes($"[{DateTime.Now}] {sender} whispered to you: \"{restoredMessage}\"")));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes($"[{DateTime.Now}] you whispered to {targetName}: \"{restoredMessage}\"")));
                        }
                        break;
                    }
                case "w":
                    {
                        string targetName = commandParts[1];
                        if (string.IsNullOrEmpty(targetName))
                        {
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("Couldn't parse the target nickname, it was empty")));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                  Encoding.UTF8.GetBytes("Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'")));
                        }
                        else if (!ServerState.Instance.Data.Clients.ContainsKey(targetName))
                        {
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes("There is no such player")));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                  Encoding.UTF8.GetBytes("Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'")));
                        }
                        else
                        {
                            var restoredMessage = potentialFullCommand.Substring(commandParts[0].Length + commandParts[1].Length + 2);
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { targetName },
                                Encoding.UTF8.GetBytes($"[{DateTime.Now}] {sender} whispered to you: \"{restoredMessage}\"")));
                            ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                                Encoding.UTF8.GetBytes($"[{DateTime.Now}] you whispered to {targetName}: \"{restoredMessage}\"")));
                        }
                        break;
                    }
                default:
                    ServerState.Instance.Data.PendingMessages.Add((new string[] { sender },
                        Encoding.UTF8.GetBytes("Couldn't parse the command.")));
                    break;
            }
        }
        return parsed;
    }
}




