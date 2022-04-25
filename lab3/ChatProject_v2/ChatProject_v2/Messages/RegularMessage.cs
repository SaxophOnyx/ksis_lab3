using System;
using System.Net;
using System.Text;

namespace ChatProject_v2
{
    public sealed class RegularMessage: Message
    {
        public string Text { get; }


        public RegularMessage(IPAddress ip, string text)
            : base(MessageType.Regular, ip)
        {
            Text = text;
        }

        public RegularMessage(string ip, string text)
            : base(MessageType.Regular, ip)
        {
            Text = text;
        }


        public override byte[] ToBytes()
        {
            byte[] header = base.ToBytes();
            byte[] textBytes = Encoding.Unicode.GetBytes(Text);
            byte[] textLength = BitConverter.GetBytes(textBytes.Length);

            byte[] res = new byte[header.Length + textLength.Length + textBytes.Length];
            Buffer.BlockCopy(header, 0, res, 0, header.Length);
            Buffer.BlockCopy(textLength, 0, res, header.Length, textLength.Length);
            Buffer.BlockCopy(textBytes, 0, res, header.Length + textLength.Length, textBytes.Length);

            return res;
        }

        public static RegularMessage GetFromBytes(byte[] bytes)
        {
            byte[] ipBytes = new byte[4];
            Buffer.BlockCopy(bytes, 1, ipBytes, 0, ipBytes.Length);
            IPAddress ip = new IPAddress(ipBytes);

            int textLength = BitConverter.ToInt32(bytes, 5);
            string text = Encoding.Unicode.GetString(bytes, 9, textLength);

            return new RegularMessage(ip, text);
        }
    }
}
