using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using static StreamUtil;

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
    private void AddPendingPacket(string[]? mask, Packet packet)
    {
        ServerState.Instance.Data.PendingPackets.Add((mask, packet));
    }
    private void AddPendingPacket(string[]? mask, object obj)
    {
        var packet = new Packet
        {
            TypeName = obj.GetType().Name,
            ObjectData = obj
        };
        ServerState.Instance.Data.PendingPackets.Add((mask, packet));
    }

    private void LifeCycle()
    {
        while (_active)
        {
            ProcessNewClients();
            ProcessExistingClients();
            CheckFaultyClients();
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

            AddPendingPacket(new string[] { username }, $"You have connected to the server with username: {username}.");
            AddPendingPacket(GetAllApartFrom(username), $"{username} connected to the server.");

            Debug.Log<ServerCore>($" <WHI>{username} connected.");
        }
    }
    private void CheckFaultyClients()
    {
        ConcurrentBag<string> faultyClientsBuffer = new ConcurrentBag<string>();
        Task[] clientPolls = new Task[ServerState.Instance.Data.Clients.Count];
        int i = 0;

        foreach (var client in ServerState.Instance.Data.Clients)
        {
            clientPolls[i++] = Task.Run(() =>
            {
                try
                {
                    byte[] buffer = new byte[1];

                    var stream = client.Value.GetStream();
                    stream.Read(buffer, 0, 0);
                }
                catch { }

                if (!client.Value.Connected && !faultyClientsBuffer.Contains(client.Key))
                    faultyClientsBuffer.Add(client.Key);
            });
        }
        Task.WaitAll(clientPolls, 32);
        foreach (var client in faultyClientsBuffer)
        {
            ServerState.Instance.Data.Clients.Remove(client);
            AddPendingPacket(null, $"{client} has disconnected from the server.");

            Debug.Log<ServerCore>($" <WHI>{client} disconnected, due to unpredicted socket close.");
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
            if (user.Value.Available <= 4) continue;

            NetworkStream stream = user.Value.GetStream();
            var packet = Read(stream);

            if (packet is null
                || string.IsNullOrEmpty(packet.TypeName) 
                || packet.ObjectData is null) 
                continue;

            if (packet.TypeName == typeof(string).Name)
            {

                string decodedMessage = (string)packet.ObjectData;
                if (TryParsingCommand(decodedMessage, user.Key))
                    continue;

                packet.ObjectData = $"[{DateTime.Now}] {user.Key} : {decodedMessage}";
            }

            AddPendingPacket(null, packet);
        }
        if (ServerState.Instance.Data.PendingPackets.Count > 0)
        {
            foreach (var user in ServerState.Instance.Data.Clients)
            foreach (var entry in ServerState.Instance.Data.PendingPackets)
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
                Write(stream, entry.packet);
            }
        }
        ServerState.Instance.Data.PendingPackets.Clear();
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
                case "disconnect":
                    ServerState.Instance.Data.Clients.Remove(sender);
                    AddPendingPacket(null, $"{sender} has disconnected from the server.");
                    Debug.Log<ServerCore>($" <WHI>{sender} disconnected");
                    break;
                case "help":
                    AddPendingPacket(new string[] { sender }, "List of available commands:\n\t/help\n\t/setname or /sn\n\t/list\n\t/whisper or /w");
                    break;
                case "setname":
                    if (!string.IsNullOrEmpty(commandParts[1]))
                    {
                        string username = commandParts[1].ToLower();
                        if (ServerState.Instance.Data.Clients.ContainsKey(username))
                        {
                            AddPendingPacket(new string[] { sender }, "You can not use this name, there is already a user with this name.");
                        }
                        else
                        {
                            ServerState.Instance.Data.PendingUsernameChanges.Enqueue((sender, username));
                            AddPendingPacket(new string[] { sender }, "Successfully changed name.");
                        }
                    }
                    else
                    {
                        AddPendingPacket(new string[] { sender },"You can not use this name, it is empty.");
                    }
                    break;
                case "sn":
                    if (!string.IsNullOrEmpty(commandParts[1]))
                    {
                        string username = commandParts[1].ToLower();
                        if (ServerState.Instance.Data.Clients.ContainsKey(username))
                        {
                            AddPendingPacket(new string[] { sender }, "You can not use this name, there is already a user with this name.");
                        }
                        else
                        {
                            ServerState.Instance.Data.PendingUsernameChanges.Enqueue((sender, username));
                            AddPendingPacket(new string[] { sender }, "Successfully changed name.");
                        }
                    }
                    else
                    {
                        AddPendingPacket(new string[] { sender }, "You can not use this name, it is empty.");
                    }
                    break;
                case "list":
                    string reply = "List of connected users:\n";
                    foreach (var client in ServerState.Instance.Data.Clients.Keys)
                        reply += $"\t{client}\n";
                    AddPendingPacket(new string[] { sender }, reply);
                    break;
                case "whisper":
                    {
                        string targetName = commandParts[1];
                        if (string.IsNullOrEmpty(targetName))
                        {
                            AddPendingPacket(new string[] { sender }, "Couldn't parse the target nickname, it was empty");
                            AddPendingPacket(new string[] { sender }, "Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'");
                        }
                        else if (!ServerState.Instance.Data.Clients.ContainsKey(targetName))
                        {
                            AddPendingPacket(new string[] { sender }, "There is no such player");
                            AddPendingPacket(new string[] { sender }, "Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'");
                        }
                        else
                        {
                            var restoredMessage = potentialFullCommand.Substring(commandParts[0].Length + commandParts[1].Length + 2);
                            AddPendingPacket(new string[] { targetName }, $"[{DateTime.Now}] {sender} whispered to you: \"{restoredMessage}\"");
                            AddPendingPacket(new string[] { sender }, $"[{DateTime.Now}] you whispered to {targetName}: \"{restoredMessage}\"");
                        }
                        break;
                    }
                case "w":
                    {
                        string targetName = commandParts[1];
                        if (string.IsNullOrEmpty(targetName))
                        {
                            AddPendingPacket(new string[] { sender }, "Couldn't parse the target nickname, it was empty");
                            AddPendingPacket(new string[] { sender }, "Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'");
                        }
                        else if (!ServerState.Instance.Data.Clients.ContainsKey(targetName))
                        {
                            AddPendingPacket(new string[] { sender }, "There is no such player");
                            AddPendingPacket(new string[] { sender }, "Couldn't parse the target nickname due to invalid formatting, format should be -> '/w or /whisper <name> <message>'");
                        }
                        else
                        {
                            var restoredMessage = potentialFullCommand.Substring(commandParts[0].Length + commandParts[1].Length + 2);
                            AddPendingPacket(new string[] { targetName }, $"[{DateTime.Now}] {sender} whispered to you: \"{restoredMessage}\"");
                            AddPendingPacket(new string[] { sender }, $"[{DateTime.Now}] you whispered to {targetName}: \"{restoredMessage}\"");
                        }
                        break;
                    }
                default:
                    AddPendingPacket(new string[] { sender }, "Couldn't parse the command.");
                    break;
            }
        }
        return parsed;
    }
}




