using System;
using System.Net;

namespace ChatProject_v2
{
    public sealed class DisconnectionMessage: Message
    {
        public DisconnectionMessage(IPAddress ip)
            : base(MessageType.Disconnection, ip)
        {

        }

        public DisconnectionMessage(string ip)
            : base(MessageType.Disconnection, ip)
        {

        }


        public override byte[] ToBytes()
        {
            return base.ToBytes();
        }

        public static DisconnectionMessage GetFromBytes(byte[] bytes)
        {
            byte[] ipBytes = new byte[4];
            Buffer.BlockCopy(bytes, 1, ipBytes, 0, ipBytes.Length);
            IPAddress ip = new IPAddress(ipBytes);

            return new DisconnectionMessage(ip);
        }
    }
}
