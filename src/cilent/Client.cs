using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Net;

// Mod : Client, Des.: LC原版客户端核心类模组
// Part : Client主部分
public partial class Client
{
    public TcpClient? tcpClient;
    public TcpClient? tcpClient2;
    public NetworkStream? clientStream;
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
            Console.Write("请输入用户名（按 Enter 使用计算机名, 空格会被忽略）: ");
            string username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
                username = Environment.MachineName;
            if (username.Contains(" "))
            {
                username = username.Replace(" ", "");//删除空格
            }
            
            usernamecpy = username;

            // 发送用户名到服务器
            SendMessage(username);

            // 用户输入密码
            Console.Write("请输入密码(空格会被忽略): ");
            string password = Console.ReadLine();
            //检测是否有空格,有空格直接给他干了
            if (password.Contains(" "))
            {
                    password = password.Replace(" ","");//删除空格
            }


            // 发送密码到服务器
            SendMessage(password);

            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();

            // 显示连接成功提示信息
            Console.WriteLine("已连接到服务器。输入 'exit' 以关闭客户端。");

            while (true)
            {
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    SendMessage("我下线了啊拜拜");
                    tcpClient?.Close();
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
                bytesRead = clientStream?.Read(message, 0, 32567) ?? 0;
            }
            catch
            {
                break;
            }

            if (bytesRead == 0)
                break;

            string data = Encoding.UTF8.GetString(message, 0, bytesRead);
            messages.Add($"\a{DateTime.Now} > {data}");
            string logtmp = $"{DateTime.Now} > {data}";
            Log(logtmp);
            // 清除控制台并重新打印所有消息
            Console.Clear();
            foreach (var msg in messages)
            {
                Console.WriteLine(msg);
            }

            if (!connectionMessageShown)
            {
                Console.WriteLine($"已连接到服务器。输入 'exit' 以关闭客户端。");
                Log($"我({usernamecpy})已连接到服务器。输入 'exit' 以关闭客户端。");
                connectionMessageShown = true;
            }
        }
    }

    public void SendMessage(string message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        clientStream?.Write(messageBytes, 0, messageBytes.Length);
        clientStream?.Flush();
    }

    // Mod开发区域
    // 以下空间供Mod的开发
    // Mod开发规则:
    // 一个mod只能使用一个Class,Class名称必须为mod名称
    // Mod必须要有一个构造函数,构造函数必须要有一个Client/Server形参,并且在模组类内部创建一个和形参同等类型的对象
    // 使这个内部对象和实参(形参)完全一样
    // 必须为public类

    // Mod : MyMod, Des.: 简易模组示例
    public class mod
    {
        public Client clientcpy;
        public mod(Client client)
        {
            clientcpy = client;
        }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public void Start()
        {
            // Console.WriteLine();
        }
    }
}
