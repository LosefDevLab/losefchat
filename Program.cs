using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;

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
            string message = Console.ReadLine();

            if (message.ToLower() == "exit")
            {
                SendMessage("exit");
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