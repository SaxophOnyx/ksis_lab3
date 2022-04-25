using System;
using System.Net;

namespace ChatProject_v2
{
    public enum MessageType { Regular, Connection, Disconnection}

    public abstract class Message
    {
        private const int HeaderLength = 5;

        public MessageType Type { get; }

        public IPAddress SenderIP { get; }


        public Message(MessageType type, IPAddress ip)
        {
            Type = type;
            SenderIP = ip;
        }

        public Message(MessageType type, string ip)
        {
            Type = type;
            if (IPAddress.TryParse(ip, out IPAddress tmp))
                SenderIP = tmp;
            else
                throw new Exception("Invalid IP");
        }


        public virtual byte[] ToBytes()
        {
            byte[] res = new byte[HeaderLength];
            res[0] = (byte)Type;
            Buffer.BlockCopy(SenderIP.GetAddressBytes(), 0, res, 1, 4);
            return res;
        }

        public static MessageType DefineType(byte[] bytes)
        {
            return (MessageType)bytes[0];
        }
    }
}
