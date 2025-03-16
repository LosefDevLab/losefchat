using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

// Mod : Server, Des.: LC原版服务端核心类模组
// Part : 服务器分区功能

public partial class Server
{
    public Dictionary<string, List<ClientInfo>> chatPartitions = new Dictionary<string, List<ClientInfo>>();
    public object partitionLock = new object();
}