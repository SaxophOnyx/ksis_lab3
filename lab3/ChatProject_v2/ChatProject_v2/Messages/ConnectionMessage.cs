using System;
using System.Net;
using System.Text;

namespace ChatProject_v2
{
    public sealed class ConnectionMessage: Message
    {
        public string Username { get; }

        public bool IsResponse { get; }


        public ConnectionMessage(IPAddress ip, string name, bool isResponse)
            : base(MessageType.Connection, ip)
        {
            Username = name;
            IsResponse = isResponse;
        }

        public ConnectionMessage(string ip, string name, bool isResponse)
            : base(MessageType.Connection, ip)
        {
            Username = name;
            IsResponse = isResponse;
        }


        public override byte[] ToBytes()
        {
            byte[] header = base.ToBytes();
            byte[] isResponseBytes = BitConverter.GetBytes(IsResponse);
            byte[] nameBytes = Encoding.Unicode.GetBytes(Username);
            byte[] nameLength = BitConverter.GetBytes(nameBytes.Length);

            byte[] res = new byte[header.Length + isResponseBytes.Length + nameLength.Length + nameBytes.Length];
            Buffer.BlockCopy(header, 0, res, 0, header.Length);
            Buffer.BlockCopy(isResponseBytes, 0, res, header.Length, isResponseBytes.Length);
            Buffer.BlockCopy(nameLength, 0, res, header.Length + isResponseBytes.Length, nameLength.Length);
            Buffer.BlockCopy(nameBytes, 0, res, header.Length + nameLength.Length + isResponseBytes.Length, nameBytes.Length);

            return res;
        }

        public static ConnectionMessage GetFromBytes(byte[] bytes)
        {
            byte[] ipBytes = new byte[4];
            Buffer.BlockCopy(bytes, 1, ipBytes, 0, ipBytes.Length);
            IPAddress ip = new IPAddress(ipBytes);

            bool isResponse = BitConverter.ToBoolean(bytes, 5);

            int nameLength = BitConverter.ToInt32(bytes, 6);
            string name = Encoding.Unicode.GetString(bytes, 10, nameLength);

            return new ConnectionMessage(ip, name, isResponse);
        }
    }
}
