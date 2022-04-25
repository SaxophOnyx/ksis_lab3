using System;

namespace ChatProject_v2
{
    public class ChatLogEntry
    {
        private const string DefaultName = "SYSTEM";

        private const string DefaultIP = "255.255.255.255";

        public TimeSpan Time { get; }

        public string Username { get; }

        public string UserIP { get; }

        public string Message { get; }


        public ChatLogEntry(TimeSpan time, string username, string ip, string message)
        {
            Time = time;
            Username = username;
            UserIP = ip;
            Message = message;
        }

        public ChatLogEntry(string username, string ip, string message)
        {
            Time = DateTime.Now.TimeOfDay;
            Username = username;
            UserIP = ip;
            Message = message;
        }

        public ChatLogEntry(TimeSpan time, string message)
        {
            Time = time;
            Username = DefaultName;
            UserIP = DefaultIP;
            Message = message;
        }

        public ChatLogEntry(string message)
        {
            Time = DateTime.Now.TimeOfDay;
            Username = DefaultName;
            UserIP = DefaultIP;
            Message = message;
        }


        public override string ToString()
        {
            string timeStr = String.Format("{0:00}:{1:00}:{2:00.#}", Time.Hours, Time.Minutes, Time.Seconds);
            string[] ipArr = UserIP.Split('.');

            for (int i = 0; i < ipArr.Length; ++i)
                ipArr[i] = ipArr[i].PadLeft(3, '0');

            string ipStr = String.Format("{0}.{1}.{2}.{3}", ipArr[0], ipArr[1], ipArr[2], ipArr[3]);
            return String.Format("[{0}]  [{1,15}]-{2}:    {3}", timeStr, ipStr, Username, Message);
        }
    }
}
