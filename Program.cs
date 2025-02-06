using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

class Client
{
    public TcpClient tcpClient;
    public TcpClient tcpClient2;
    public NetworkStream clientStream;
    public string logFilePath = "logclient.txt"; // Log file path
    public StreamWriter logFile;

    public string usernamecpy = "";

    public Client()
    {
        // 确保日志文件存在
        if (!File.Exists(logFilePath))
        {
            using (File.Create(logFilePath)) { }
        }
        // 初始化 StreamWriter
        logFile = new StreamWriter(logFilePath, true);
    }

    ~Client()
    {
        // 关闭 StreamWriter
        logFile?.Close();
    }

    public void Log(string message)
    {
        logFile.WriteLine($"{DateTime.Now}: {message}");
        logFile.Flush();
    }

    public void Connect(int ipvx, string serverIP, int serverPort)
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient2 = new TcpClient(AddressFamily.InterNetworkV6);

            if (ipvx == 4)
            {
                tcpClient.Connect(serverIP, serverPort);
                clientStream = tcpClient.GetStream();
            }
            else if (ipvx == 6)
            {
                tcpClient2.Connect(serverIP, serverPort);
                clientStream = tcpClient2.GetStream();
            }
            else
            {
                Console.WriteLine("我不说了，不要乱输入吗？qwq你不爱我了qwq");
                Console.WriteLine("既然这样，那我就放炸弹!10秒倒计时!");
                Thread.Sleep(1000);
                Console.WriteLine("9秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("8秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("7秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("6秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("5秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("4秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("3秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("2秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("1秒后爆炸");
                Thread.Sleep(1000);
                Console.WriteLine("装逼我让你飞起来!");
                Environment.Exit(-2147483648);
                return;
            }

            // 用户输入用户名，如果没有输入则使用计算机名称
            Console.Write("请输入用户名（按 Enter 使用计算机名）: ");
            string username = Console.ReadLine();
            usernamecpy = username;

            if (string.IsNullOrEmpty(username))
                username = Environment.MachineName;

            // 发送用户名到服务器
            SendMessage(username);

            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();

            // Mod运行区域
            // 用户请在这里安装mod,下面为一个简易示例
            mod moda = new mod();
            Thread modThread = new Thread(new ThreadStart(moda.Start)); // <<<这里在实际中要看Mod的启动方法是哪一个,具体请看mod作者如何要求
            modThread.Start();

            // 显示连接成功提示信息
            Console.WriteLine("已连接到服务器。输入 'exit' 以关闭客户端。");

            while (true)
            {
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
        catch (Exception ex)
        {
            Log($"连接服务器时发生异常: {ex.Message}");
        }
    }

    public void ReceiveMessage()
    {
        byte[] message = new byte[32567];
        int bytesRead;

        List<string> messages = new List<string>();
        bool connectionMessageShown = false;

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
            messages.Add($"\a{DateTime.Now} > {data}");

            // 清除控制台并重新打印所有消息
            Console.Clear();
            foreach (var msg in messages)
            {
                Console.WriteLine(msg);
                Log(msg);
            }

            if (!connectionMessageShown)
            {
                Console.WriteLine($"我({usernamecpy})已连接到服务器。输入 'exit' 以关闭客户端。");
                connectionMessageShown = true;
            }
        }
    }

    public void SendMessage(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        clientStream.Write(messageBytes, 0, messageBytes.Length);
        clientStream.Flush();
    }

    // Mod开发区域
    // 以下空间供Mod的开发
    // Mod开发规则:
    // 一个mod只能使用一个Class,Class名称必须为mod名称
    class mod
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public void Start()
        {
            // Console.WriteLine();
        }
    }
}

class Server
{
    public TcpListener tcpListener;
    public List<ClientInfo> clientList = new List<ClientInfo>();
    public object lockObject = new object();
    public string logFilePath = "log.txt"; // Log file path
    public string searchFilePath = "search_results.txt"; // Search results file path
    public string bannedUsersFilePath = "banned_users.txt"; // Banned users file path
    public HashSet<string> bannedUsersSet;

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

        //Mod运行区域
        //用户请在这里添加mod,下面是一个简单示范
        mod moda = new mod();
        Thread modThread = new Thread(new ThreadStart(moda.Start));//<<<这里在实际中要看Mod的启动方法是哪一个,具体请看mod作者如何要求
        modThread.Start();

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

    public bool IsUsernameAvailable(string username)
    {
        lock (lockObject)
        {
            return !clientList.Exists(c => c.Username == username);
        }
    }

    public void HandleClientCommunication(object clientInfoObj)
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

    public void BroadcastMessage(string message)
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

    public void SendMessage(ClientInfo clientInfo, string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        clientInfo.TcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);
        clientInfo.TcpClient.GetStream().Flush();
    }

    public void BanUser(string targetUsername)
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

    public void KickBannedUser(string targetUsername)
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

    public void KickUser(string targetUsername)
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

    public void UnbanUser(string targetUsername)
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

    public void DisplayAllUsers()
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

    public void ReadConsoleInput()
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

    public void Log(string message)
    {
        using (StreamWriter logFile = new StreamWriter(logFilePath, true))
        {
            logFile.WriteLine($"{DateTime.Now}: {message}");
            Console.WriteLine($"\a{DateTime.Now}: {message}");
        }
    }

    public void SearchLog(string searchKeyword)
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

    //Mod开发区域
    //以下区域供Mod的开发
    //Mod开发规则:
    //一个mod只能使用一个Class,Class名称必须为mod名称
    class mod
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public void Start()
        {
            // Console.WriteLine();
        }
    }
}

class 程序
{
    static void Main()
    {
        Console.WriteLine("欢迎使用LosefChat v0.1.r2.b42\n输入1 开始聊天,输入2 服务器,输入3 EXIT");
        
        int choose = int.Parse(Console.ReadLine());
        if (choose == 1)
        {
            Console.Write("你要用ipv4协议，还是用ipv6协议？(输入4或者6,乱输我们就要把你请出去了哦awa):");
            int 选择 = int.Parse(Console.ReadLine());

            Console.Write("请输入服务器 IP 地址: ");
            string 服务器IP = Console.ReadLine();

            Console.Write("请输入服务器端口号: ");
            int 服务器端口号 = int.Parse(Console.ReadLine());

            Client 客户端 = new Client();
            客户端.Connect(选择, 服务器IP, 服务器端口号);
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