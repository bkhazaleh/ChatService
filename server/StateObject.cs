using System;
using System.Net.Sockets;
using System.Text;
using static server.General;

namespace server
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        // Current status
        public Status CurrentStatus = Status.NotSet;
        // Time of last received message
        public DateTime LastMessageTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 3));
        // Action to be done after receiving the message
        public General.Action ActionToBeDone = General.Action.doNothing;

    }
}
