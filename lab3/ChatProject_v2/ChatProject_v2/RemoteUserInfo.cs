using System;
using System.Net;
using System.Net.Sockets;

namespace ChatProject_v2
{
    public class RemoteUserInfo
    {
        public string Name { get; }

        public TcpClient TcpClient { get; }


        public RemoteUserInfo(string name, TcpClient client)
        {
            Name = name;
            TcpClient = client;
        }
    }
}
