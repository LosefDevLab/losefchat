using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

//"我们真的没使用AI"--losef曾经说过的顶级笑话

class Client
{
    private TcpClient tcpClient;
    private NetworkStream clientStream;

    public void Connect(string serverIP, int serverPort)
    {
        tcpClient = new TcpClient();
        tcpClient.Connect(serverIP, serverPort);

        clientStream = tcpClient.GetStream();

        // 用户输入用户名，如果没有输入则使用计算机名称
        Console.Write("请输入用户名（按 Enter 使用计算机名）: ");
        string username = Console.ReadLine();

        if (string.IsNullOrEmpty(username))
            username = Environment.MachineName;

        // 发送用户名到服务器
        SendMessage(username);

        Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
        receiveThread.Start();

        Console.WriteLine("已连接到服务器。输入 'exit' 以关闭客户端。");

        while (true)
        {
            Console.Write($"{username}&{DateTime.Now}$> ");
            string message = Console.ReadLine();

            if (message.ToLower() == "exit")
            {
                SendMessage("我下线了啊拜拜");
                tcpClient.Close();
                break;
            }

            SendMessage(message);
        }
    }

    private void ReceiveMessage()
    {
        byte[] message = new byte[32567];
        int bytesRead;

        while (true)
        {
            bytesRead = 0;

            try
            {
                bytesRead = clientStream.Read(message, 0, 32567);
            }
            catch
            {
                break;
            }

            if (bytesRead == 0)
                break;

            string data = Encoding.UTF8.GetString(message, 0, bytesRead);
            Console.WriteLine(data);
        }

        Console.WriteLine("已断开与服务器的连接。如服务器多次连接不上，可能已被封禁");
        tcpClient.Close();
    }

    private void SendMessage(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        clientStream.Write(messageBytes, 0, messageBytes.Length);
        clientStream.Flush();
    }
}

class Server
{
    private TcpListener tcpListener;
    private List<ClientInfo> clientList = new List<ClientInfo>();
    private object lockObject = new object();
    private string logFilePath = "log.txt"; // Log file path
    private string searchFilePath = "search_results.txt"; // Search results file path
    private string bannedUsersFilePath = "banned_users.txt"; // Banned users file path
    private HashSet<string> bannedUsersSet;

    public Server(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);

        // Create log file if it doesn't exist
        if (!File.Exists(logFilePath))
        {
            using (File.Create(logFilePath)) { }
        }
        // Create search results file if it doesn't exist
        if (!File.Exists(searchFilePath))
        {
            using (File.Create(searchFilePath)) { }
        }

        // Create or read banned users file
        if (!File.Exists(bannedUsersFilePath))
        {
            using (File.Create(bannedUsersFilePath)) { }
        }
        bannedUsersSet = File.ReadAllLines(bannedUsersFilePath).ToHashSet();
    }

    public void Start()
    {
        Log("Server started.");

        tcpListener.Start();

        // Start a new thread to handle console input
        Thread consoleInputThread = new Thread(new ThreadStart(ReadConsoleInput));
        consoleInputThread.Start();

        while (true)
        {
            TcpClient tcpClient = tcpListener.AcceptTcpClient();

            // User connected to the server message
            byte[] connectionMessageBytes = new byte[32567];
            int bytesRead = tcpClient.GetStream().Read(connectionMessageBytes, 0, 32567);
            string connectionMessage = Encoding.UTF8.GetString(connectionMessageBytes, 0, bytesRead);

            // Create client info object to store client information
            ClientInfo clientInfo = new ClientInfo { TcpClient = tcpClient, ConnectionMessage = connectionMessage };

            // Check if in banned list
            if (bannedUsersSet.Contains(clientInfo.Username))
            {
                Console.WriteLine($"拒绝了一个封禁用户的连接请求: '{clientInfo.Username}' 好像不知道他在封禁名单里面.");
                tcpClient.Close();
                continue;
            }

            // Check for duplicate username
            if (!IsUsernameAvailable(clientInfo.Username))
            {
                SendMessage(clientInfo, "qwq在这个服务器里面已经存在这个用户名字了捏.");
                tcpClient.Close();
                continue;
            }

            lock (lockObject)
            {
                clientList.Add(clientInfo);
            }

            // Broadcast new user joined message
            BroadcastMessage($"{clientInfo.Username} 加入了服务器");
            // Start a new thread to handle client communication
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
            clientThread.Start(clientInfo);
        }
    }

    private bool IsUsernameAvailable(string username)
    {
        lock (lockObject)
        {
            return !clientList.Exists(c => c.Username == username);
        }
    }

    private void HandleClientCommunication(object clientInfoObj)
    {
        ClientInfo clientInfo = (ClientInfo)clientInfoObj;
        TcpClient tcpClient = clientInfo.TcpClient;
        NetworkStream clientStream = tcpClient.GetStream();

        Log($"用户 '{clientInfo.Username}' 连接到服务器.");

        byte[] messageBytes = new byte[32567];
        int bytesRead;

        while (true)
        {
            bytesRead = 0;

            try
            {
                bytesRead = clientStream.Read(messageBytes, 0, 32567);
            }
            catch
            {
                break;
            }

            if (bytesRead == 0)
                break;

            string data = Encoding.UTF8.GetString(messageBytes, 0, bytesRead);

            // Broadcast message to all clients
            BroadcastMessage($"{clientInfo.Username}: {data}");
        }

        Console.WriteLine("用户 '" + clientInfo.Username + "' 下线了.");
        lock (lockObject)
        {
            clientList.Remove(clientInfo);
        }
        BroadcastMessage($"{clientInfo.Username} 下线.");
        tcpClient.Close();
    }

    private void BroadcastMessage(string message)
    {
        byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);

        lock (lockObject)
        {
            foreach (var client in clientList)
            {
                NetworkStream clientStream = client.TcpClient.GetStream();
                clientStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                clientStream.Flush();
            }
        }

        // Log the broadcasted message to the log file
        Log(message);
    }

    private void SendMessage(ClientInfo clientInfo, string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        clientInfo.TcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);
        clientInfo.TcpClient.GetStream().Flush();
    }

    private void BanUser(string targetUsername)
    {
        try
        {
            lock (lockObject)
            {
                if (!bannedUsersSet.Contains(targetUsername))
                {
                    bannedUsersSet.Add(targetUsername);

                    // Update banned users file
                    File.WriteAllLines(bannedUsersFilePath, bannedUsersSet);

                    Console.WriteLine($"用户 '{targetUsername}' 已被封禁.");

                    // Try to kick out the banned user
                    KickBannedUser(targetUsername);

                    // Log ban operation to the log file
                    Log($"用户 '{targetUsername}' 已被服务器封禁.");
                }
                else
                {
                    Console.WriteLine($"用户 '{targetUsername}' 已经被封了,不要重复BAN哦.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"封禁用户时发生异常: {ex}");
            Log($"封禁用户时发生了异常: {ex}");
        }
    }

    private void KickBannedUser(string targetUsername)
    {
        try
        {
            lock (lockObject)
            {
                ClientInfo targetClient = clientList.FirstOrDefault(c => c.Username == targetUsername);

                if (targetClient != null)
                {
                    SendMessage(targetClient, "你被封了!");

                    // Send kicked out message to other users
                    BroadcastMessage($"用户 '{targetUsername}' 被服务器封禁");

                    // Remove the kicked out user from the client list
                    clientList.Remove(targetClient);

                    Thread.Sleep(3000);

                    // Close the connection with the kicked out user
                    targetClient.TcpClient.Close();
                }
                else
                {
                    // If user does not exist, log invalid user message to the log file
                    Log($"虽然没有踢出 '{targetUsername}'(可能不在线), 但是我们封禁了, 下一次他进不来.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在踢出用户的时候发生异常: {ex}");
            Log($"在踢出用户的时候发生异常: {ex}");
        }
    }

    private void KickUser(string targetUsername)
    {
        try
        {
            lock (lockObject)
            {
                ClientInfo targetClient = clientList.FirstOrDefault(c => c.Username == targetUsername);

                if (targetClient != null)
                {
                    SendMessage(targetClient, "你被管理员踢了");

                    // Send kicked out message to other users
                    BroadcastMessage($"用户 '{targetUsername}' 被管理员踢了");

                    // Remove the kicked out user from the client list
                    clientList.Remove(targetClient);

                    // Close the connection with the kicked out user
                    targetClient.TcpClient.Close();
                }
                else
                {
                    // If user does not exist, log invalid user message to the log file
                    Log($"'{targetUsername}' 这人我寻思着也不在线啊怎么踢？");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在踢出用户的时候发生异常: {ex}");
            Log($"在踢出用户的时候发生异常: {ex}");
        }
    }

    private void UnbanUser(string targetUsername)
    {
        try
        {
            lock (lockObject)
            {
                if (bannedUsersSet.Contains(targetUsername))
                {
                    bannedUsersSet.Remove(targetUsername);

                    // Update banned users file
                    File.WriteAllLines(bannedUsersFilePath, bannedUsersSet);

                    Console.WriteLine($"'{targetUsername}' 出狱了");

                    // Log unban operation to the log file
                    Log($"'{targetUsername}' 经服务器官方批准,出狱");
                }
                else
                {
                    Console.WriteLine($"'{targetUsername}' 本来就不是被封禁用户 你这让我很为难啊qwq.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在解封用户时发生异常 {ex}");
            Log($"在解封用户时发生异常 {ex}");
        }
    }

    private void DisplayAllUsers()
    {
        try
        {
            lock (lockObject)
            {
                Console.WriteLine("当前在线用户:");
                foreach (var client in clientList)
                {
                    Console.WriteLine(client.Username);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在投影当前在线用户的时候发生异常 {ex}");
            Log($"在投影当前在线用户的时候发生异常 {ex}");
        }
    }

    private void ReadConsoleInput()
    {
        while (true)
        {
            string input = Console.ReadLine();

            if (input.StartsWith("/kick"))
            {
                string targetUsername = input.Split(' ')[1];
                KickUser(targetUsername);
            }
            else if (input.StartsWith("/ban"))
            {
                string targetUsername = input.Split(' ')[1];
                BanUser(targetUsername);
            }
            else if (input.StartsWith("/unban"))
            {
                string targetUsername = input.Split(' ')[1];
                UnbanUser(targetUsername);
            }
            else if (input.StartsWith("/users"))
            {
                DisplayAllUsers();
            }
            else if (input.StartsWith("/search"))
            {
                string searchKeyword = input.Substring(8);
                SearchLog(searchKeyword);
            }
        }
    }

    private void Log(string message)
    {
        using (StreamWriter logFile = new StreamWriter(logFilePath, true))
        {
            logFile.WriteLine($"{DateTime.Now}: {message}");
            Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    private void SearchLog(string searchKeyword)
    {
        try
        {
            var logContent = File.ReadAllLines(logFilePath);
            var matchingResults = logContent.Where(line => line.Contains(searchKeyword)).ToList();

            using (StreamWriter searchResultsFile = new StreamWriter(searchFilePath, false))
            {
                foreach (var matchingLine in matchingResults)
                {
                    searchResultsFile.WriteLine(matchingLine);
                }
            }

            Console.WriteLine($"找到了 {matchingResults.Count} 条关于 \"{searchKeyword}\" 的记录日志, 我已保存(\"{searchFilePath}\"), 感觉良好.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在搜索日志时发生异常 {ex}");
            Log($"在搜索日志时发生异常 {ex}");
        }
    }

    public class ClientInfo
    {
        public TcpClient TcpClient { get; set; }
        public string Username => ConnectionMessage.Split(':')[0];
        public string ConnectionMessage { get; set; }
    }
}

class 程序
{
    static void Main()
    {
        Console.WriteLine("欢迎使用LosefChat v0.1.d1.b7\n输入1 开始聊天,输入2 服务器,输入3 EXIT");
        
        int choose = int.Parse(Console.ReadLine());
        if (choose == 1)
        {
            Console.Write("请输入服务器 IP 地址: ");
            string 服务器IP = Console.ReadLine();

            Console.Write("请输入服务器端口号: ");
            int 服务器端口号 = int.Parse(Console.ReadLine());

            Client 客户端 = new Client();
            客户端.Connect(服务器IP, 服务器端口号);
        }
        if (choose == 2)
        {
            Console.Write("请输入服务器端口号: ");
            int 端口 = int.Parse(Console.ReadLine());

            Server 服务器 = new Server(端口);
            服务器.Start();
        }
        if (choose == 3)
        {
            Environment.Exit(0);
        }
    }
}