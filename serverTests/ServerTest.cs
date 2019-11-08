using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace serverTests
{
    [TestClass]
    public class ServerTest
    {
        [TestMethod]
        public void ListenTest()
        {
            //start the server
            Thread serverThread = new Thread(server.SocketListener.StartListening);
            serverThread.Start(true);

            //
            Thread.Sleep(1100);
        }

        [TestMethod]
        public void ListenOneClientConnection()
        {
            //start the server
            Thread serverThread = new Thread(server.SocketListener.StartListening);
            serverThread.Start(true);

            //
            Thread.Sleep(1000);

            Settings settings = new Settings
            {
                Host = "bayans-imac.local",
                Port = 5950,
                EndOfMessage = "<EOF>",
                NumberOfMessagesToBeSend = 0,
                MilliSecondsBetweenMessagesTobeSend = 0
            };

            Client.StartClient(settings);
            Thread.Sleep(1000);
        }


        [TestMethod]
        public void ListenTenClientConnection()
        {
            //start the server
            Thread serverThread = new Thread(server.SocketListener.StartListening);
            serverThread.Start(true);

            //
            Thread.Sleep(1000);

            Settings settings = new Settings
            {
                Host = "bayans-imac.local",
                Port = 5950,
                EndOfMessage = "<EOF>",
                NumberOfMessagesToBeSend = 1,
                MilliSecondsBetweenMessagesTobeSend = 0
            };

            for (int clientIndex = 0; clientIndex < 10; clientIndex++)
            {
                Thread clientThread = new Thread(Client.StartClient);
                clientThread.Start(settings);
            }
            Thread.Sleep(1000);

        }

        [TestMethod]
        public void ServerReceivingMultiMessagesInLessThanASecond()
        {
            //start the server
            Thread serverThread = new Thread(server.SocketListener.StartListening);
            serverThread.Start(true);

            //
            Thread.Sleep(1000);

            Settings settings = new Settings
            {
                Host = "bayans-imac.local",
                Port = 5950,
                EndOfMessage = "<EOF>",
                NumberOfMessagesToBeSend = 3,
                MilliSecondsBetweenMessagesTobeSend = 1
            };

            Client.StartClient(settings);
        }

        [TestMethod]
        public void ServerReceivingMultiMessagesInMoreThanASecond()
        {
            //start the server
            Thread serverThread = new Thread(server.SocketListener.StartListening);
            serverThread.Start(true);

            //
            Thread.Sleep(1000);

            Settings settings = new Settings
            {
                Host = "bayans-imac.local",
                Port = 5950,
                EndOfMessage = "<EOF>",
                NumberOfMessagesToBeSend = 3,
                MilliSecondsBetweenMessagesTobeSend = 1001
            };

            Client.StartClient(settings);
        }

    }
}
