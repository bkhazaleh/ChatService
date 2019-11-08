using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace clientTests
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public void TestClient()
        {
            Settings settings = new Settings
            {
                Port = 5950,
                EndOfMessage = "<EOF>"
            };
            Thread serverThread = new Thread(Server.StartListening);
            serverThread.Start(settings);

            Thread.Sleep(1100);
            client.AsynchronousClient.StartClient(true);

            Thread.Sleep(1100);
        }
    }
}
