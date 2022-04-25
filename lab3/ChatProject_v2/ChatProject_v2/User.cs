using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ChatProject_v2
{
    public class User
    {
        public string Name { get; }

        public IPAddress IP { get; }

        public int TcpPort { get; }

        public int UdpPort { get; }

        public ConcurrentDictionary<IPAddress, RemoteUserInfo> ActiveUsers { get; }

        public ConcurrentQueue<ChatLogEntry> ChatLog { get; }

        public bool IsPrinting { get; private set; }

        public bool IsActive { get; private set; }


        public User(string name, string ip)
        {
            Name = name;

            if (IPAddress.TryParse(ip, out IPAddress ipAddr))
                IP = ipAddr;
            else
                throw new Exception("Invalid IP");

            TcpPort = 8001;
            UdpPort = 8002;
            ActiveUsers = new ConcurrentDictionary<IPAddress, RemoteUserInfo>();
            ChatLog = new ConcurrentQueue<ChatLogEntry>();

            IsActive = true;
            ListenTcp();
            ListenUdp();
        }

        public User(string name, IPAddress ip)
        {
            Name = name;
            IP = ip;
            TcpPort = 8001;
            UdpPort = 8002;
            ActiveUsers = new ConcurrentDictionary<IPAddress, RemoteUserInfo>();
            ChatLog = new ConcurrentQueue<ChatLogEntry>();

            IsActive = true;
            ListenTcp();
            ListenUdp();
        }


        private void ListenTcp()
        {
            TcpListener TcpListener = new TcpListener(IP, TcpPort);
            TcpListener.Start();

            Task.Run(() =>
            {
                while (IsActive)
                {
                    TcpClient client = TcpListener.AcceptTcpClient();
                    Task.Factory.StartNew(() => AcceptTcp(client));
                }

                TcpListener.Stop();
            });
        }

        private void AcceptTcp(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buf = new byte[client.ReceiveBufferSize];

            while (IsActive)
            {
                try
                {
                    int count = stream.Read(buf, 0, buf.Length);
                }
                catch
                {
                    return;
                }

                switch (Message.DefineType(buf))
                {
                    case MessageType.Connection:
                    {
                        ConnectionMessage message = ConnectionMessage.GetFromBytes(buf);
                        if (!message.IsResponse)
                            TrySendConnectionResponse(message.SenderIP);
                        else
                            ActiveUsers.TryAdd(message.SenderIP, new RemoteUserInfo(message.Username, client));

                        break;
                    }

                    case MessageType.Disconnection:
                    {
                        DisconnectionMessage message = DisconnectionMessage.GetFromBytes(buf);

                        ActiveUsers.TryRemove(message.SenderIP, out RemoteUserInfo info);
                        stream.Close();
                        client.Close();

                        ChatLogEntry entry = new ChatLogEntry("Пользователь " + info.Name + " (" + message.SenderIP.ToString() + ") покинул чат");
                        ChatLog.Enqueue(entry);
                        if (!IsPrinting)
                            Console.WriteLine(entry.ToString());

                        return;
                    }

                    case MessageType.Regular:
                    {
                        RegularMessage message = RegularMessage.GetFromBytes(buf);
                        
                        ActiveUsers.TryGetValue(message.SenderIP, out RemoteUserInfo info);
                        if (info != null)
                        {
                            ChatLogEntry entry = new ChatLogEntry(info.Name, message.SenderIP.ToString(), message.Text);
                            ChatLog.Enqueue(entry);
                            if (!IsPrinting)
                                Console.WriteLine(entry.ToString());
                        }

                        break;
                    }
                }
            }
        }

        private void ListenUdp()
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, UdpPort);

            UdpClient udpClient = new UdpClient();
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(remote);

            Task.Run(() =>
            {
                while (IsActive)
                {
                    var recvBuffer = udpClient.Receive(ref remote);
                    if (recvBuffer.Length != 0)
                    {
                        if (Message.DefineType(recvBuffer) == MessageType.Connection)
                        {
                            ConnectionMessage message = ConnectionMessage.GetFromBytes(recvBuffer);

                            if ((!message.SenderIP.Equals(IP)) && (!message.IsResponse))
                            {
                                TcpClient client = new TcpClient();
                                client.Connect(message.SenderIP, TcpPort);
                                Task.Factory.StartNew(() => AcceptTcp(client));
                                ActiveUsers.TryAdd(message.SenderIP, new RemoteUserInfo(message.Username, client));
                                SendConnectionResponse(client);

                                string str = String.Format("Пользователь {0} ({1}) подключился к чату", message.Username, message.SenderIP.ToString());
                                ChatLogEntry entry = new ChatLogEntry(str);
                                ChatLog.Enqueue(entry);
                                if (!IsPrinting)
                                    Console.WriteLine(entry.ToString());
                            }
                        }                        
                    }
                }

                udpClient.Close();
                udpClient.Dispose();
            });
        }

        public void Connect()
        {
            UdpClient sender = new UdpClient(new IPEndPoint(IP, UdpPort));
            
            ConnectionMessage message = new ConnectionMessage(IP, Name, false);
            byte[] tmp = message.ToBytes();
            sender.Send(tmp, tmp.Length, IPAddress.Broadcast.ToString(), UdpPort);
            sender.Close();
        }

        public void Disconnect()
        {
            DisconnectionMessage message = new DisconnectionMessage(IP);
            byte[] bytes = message.ToBytes();

            foreach (var user in ActiveUsers)
            {
                NetworkStream stream = user.Value.TcpClient.GetStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                user.Value.TcpClient.Close();
            }

            IsActive = false;
        }

        public void SendMessage(string message)
        {
            RegularMessage regularMessage = new RegularMessage(IP, message);
            byte[] bytes = regularMessage.ToBytes();

            foreach (var user in ActiveUsers)
            {
                NetworkStream stream = user.Value.TcpClient.GetStream();
                stream.Write(bytes, 0, bytes.Length);
            }

            ChatLogEntry entry = new ChatLogEntry(Name, IP.ToString(), message);
            ChatLog.Enqueue(entry);
        }

        private void SendConnectionResponse(TcpClient client)
        {
            byte[] bytes = new ConnectionMessage(IP, Name, true).ToBytes();
            client.GetStream().Write(bytes, 0, bytes.Length);
        }

        private void TrySendConnectionResponse(IPAddress ip)
        {
            RemoteUserInfo info;
            ActiveUsers.TryGetValue(ip, out info);

            if (info != null)
            {
                byte[] bytes = new ConnectionMessage(IP, Name, true).ToBytes();
                info.TcpClient.GetStream().Write(bytes, 0, bytes.Length);
            }
        }

        public void PrintChatLog()
        {
            IsPrinting = true;
            Console.Clear();
            Console.WriteLine("История сообщений: ");

            foreach (ChatLogEntry entry in ChatLog)
            {
                Console.WriteLine(entry.ToString());
            }

            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
            IsPrinting = false;
        }

        public void PrintLastMessages(int count)
        {
            IsPrinting = true;
            Console.Clear();
            int i = ChatLog.Count;

            foreach (ChatLogEntry entry in ChatLog)
            {
                if (--i < count)
                    Console.WriteLine(entry.ToString());
            }
            IsPrinting = false;
        }
    }
}
