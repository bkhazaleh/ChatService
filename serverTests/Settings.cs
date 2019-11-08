using System;
namespace serverTests
{
    public class Settings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string EndOfMessage { get; set; }
        public int NumberOfMessagesToBeSend { get; set; }
        public int MilliSecondsBetweenMessagesTobeSend { get; set; }
    }
}
