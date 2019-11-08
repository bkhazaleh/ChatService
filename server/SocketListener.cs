using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using static server.General;

namespace server
{
    
    public class SocketListener
    {
        private static AppSettings Settings;
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        
        public SocketListener()
        {

        }
       
        private static bool LoadSettings()
        {
            try
            {
                if (File.Exists(General.SettingsFile))
                {
                    using (StreamReader r = new StreamReader(General.SettingsFile))
                    {
                        string json = r.ReadToEnd();
                        Settings = JsonConvert.DeserializeObject<AppSettings>(json);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

        public static void StartListening()
        {
            if (!LoadSettings()) return;
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Settings.Port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.  
                allDone.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                state.CurrentStatus = General.Status.Connected;
               
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //start of the message need to set the message time
                   if(state.sb.Length == 0)
                    {
                        SetAction(state);
                    }
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  
                    content = state.sb.ToString();
                    if (content.IndexOf(Settings.EndOfMessage, StringComparison.Ordinal) > -1)
                    {
                        // All the data has been read from the   
                        // client. Display it on the console.  
                        Console.WriteLine("Client {0} : {1}",
                            handler.RemoteEndPoint.ToString(), content.TrimEnd(Settings.EndOfMessage.ToCharArray()));
                        // send the client the server reply.
                        ApplyAction(handler, state);
                        
                    }
                    else
                    {
                        // Not all data received. Get more.  
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private static void Send(Socket handler, String data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;
                try
                {


                    // Complete sending the data to the remote device.  
                    int bytesSent = handler.EndSend(ar);
                    //Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                }
                catch (Exception ex)
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private static void SetAction(StateObject state)
        {
            try
            {

                    if (DateTime.UtcNow.Subtract(state.LastMessageTime).TotalSeconds <= 1.0)
                    {
                        if (state.CurrentStatus == Status.Warning) {
                            state.CurrentStatus = Status.ConnectionRefused;
                            state.ActionToBeDone = General.Action.StopTheClient;

                        }else
                        {
                            state.CurrentStatus = Status.Warning;
                            state.ActionToBeDone = General.Action.WarnTheClient;
                        }
                       

                    }else
                    {
                        state.ActionToBeDone = General.Action.doNothing;
                    }
                    
                    state.LastMessageTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
            }
        }

        private static void ApplyAction(Socket handler, StateObject state)
        {
            try
            {
                
                string content;
                
             
                switch (state.ActionToBeDone)
                {
                    //Send Ok message
                    case General.Action.doNothing:
                        content = "OK" + Settings.EndOfMessage;
                        Send(handler, content);
                        state.sb.Clear();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                        break;
                    case General.Action.WarnTheClient:
                        content = "Warning!! Please do not send more than one message in a second." + Settings.EndOfMessage;
                        Send(handler, content);
                        state.sb.Clear();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                        break;
                    case General.Action.StopTheClient:
                        content = "Error!! The connection will be closed." + Settings.EndOfMessage;
                        Send(handler, content);
                        state.sb.Clear();
                        //Shutdown the connection
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        break;

                }

                
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }
    }
}
