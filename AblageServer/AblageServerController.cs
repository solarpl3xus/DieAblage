using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AblageServer
{
    public class AblageServerController
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Thread controlListenThread;
        private int controlPort;
        private TcpListener tcpControlListener;

        private Thread dataListenThread;
        private int dataPort;
        private TcpListener tcpDataListener;

        private Dictionary<string, AblagenClient> onlineClients = new Dictionary<string, AblagenClient>();

        public AblageServerController()
        {
            controlPort = int.Parse(ConfigurationManager.AppSettings["ControlPort"]);
            dataPort = int.Parse(ConfigurationManager.AppSettings["DataPort"]);
        }

        public void Start()
        {
            try
            {
                tcpControlListener = new TcpListener(IPAddress.Any, controlPort);
                controlListenThread = new Thread(new ThreadStart(ListenForClientsControl));
                controlListenThread.IsBackground = true;
                controlListenThread.Start();

                tcpDataListener = new TcpListener(IPAddress.Any, dataPort);
                dataListenThread = new Thread(new ThreadStart(ListenForClientsData));
                dataListenThread.IsBackground = true;
                dataListenThread.Start();
            }
            catch (Exception e)
            {
                logger.Fatal("Error during server initialization", e);
                throw e;
            }
        }

        private void ListenForClientsControl()
        {
            bool tryAgain = false;
            while (true)
            {
                if (tryAgain)
                {
                    Thread.Sleep(1000);
                }

                tryAgain = false;
                try
                {
                    tcpControlListener.Start();
                    logger.Debug($"Listening for incoming control connections at port {controlPort}");
                    while (true)
                    {
                        TcpClient client = tcpControlListener.AcceptTcpClient();

                        string ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        logger.Debug($"Client connected {ipAddress}");

                        if (onlineClients.ContainsKey(ipAddress))
                        {
                            logger.Warn("Already exists");
                        }

                        AblagenClient ablagenClient = new AblagenClient(client);
                        ablagenClient.StartCommunication();
                        ablagenClient.DistributeRequest += HandleDistributeRequest;
                        ablagenClient.Disconnect += HandleClientDisconnect;
                        ablagenClient.SignIn += HandleSignIn;
                        onlineClients[ipAddress] = ablagenClient;

                        
                    }
                }
                catch (SocketException)
                {
                    tryAgain = true;
                }
                catch (Exception e)
                {
                    logger.Fatal("Error during server initialization", e);
                    throw e;
                }
            }
        }


        private void ListenForClientsData()
        {
            bool tryAgain = false;
            while (true)
            {
                if (tryAgain)
                {
                    Thread.Sleep(1000);
                }

                tryAgain = false;
                try
                {
                    tcpDataListener.Start();
                    logger.Debug($"Listening at port {dataPort}");
                    while (true)
                    {
                        TcpClient dataclient = tcpDataListener.AcceptTcpClient();

                        string ipAddress = ((IPEndPoint)dataclient.Client.RemoteEndPoint).Address.ToString();
                        logger.Debug($"Data client connected {ipAddress}");

                        if (!onlineClients.ContainsKey(ipAddress))
                        {
                            logger.Error("No control client exists");
                        }
                        else
                        {
                            onlineClients[ipAddress].HandleIncomingDataConnection(dataclient);
                        }
                    }
                }
                catch (SocketException)
                {
                    tryAgain = true;
                }
                catch (Exception e)
                {
                    logger.Fatal("Error during server initialization", e);
                    throw e;
                }
            }
        }


        private void BroadcastMessage(AblagenClient sendingClient, string message)
        {
            List<AblagenClient> recipientClients = GetOtherOnlineClients(sendingClient);

            for (int i = 0; i < recipientClients.Count; i++)
            {
                int index = i;
                recipientClients[i].SendControlMessage(message);
            }
        }

        private void HandleSignIn(AblagenClient sendingClient, EventArgs e)
        {
            BroadcastMessage(sendingClient, $"+{sendingClient.Name}");

            List<string> onlineClientNames = GetOtherOnlineClients(sendingClient).Select(k => k.Name).ToList();
            for (int i = 0; i < onlineClientNames.Count; i++)
            {
                sendingClient.SendControlMessage($"+{onlineClientNames[i]}");
            }
        }


        private void HandleDistributeRequest(AblagenClient sendingClient, DistributionRequestArgs distributionRequestArgs)
        {
            List<AblagenClient> recipientClients = GetOtherOnlineClients(sendingClient);


            for (int i = 0; i < recipientClients.Count; i++)
            {

                //pendingServerDownloads.Add(new Tuple<string, byte[]>(tcpClients[i].Client.LocalEndPoint.ToString().Replace(controlPort.ToString(), dataPort.ToString()), bufferedFile));
                int index = i;
                recipientClients[i].HandleDistributionRequest(distributionRequestArgs);

            }
        }

        private List<AblagenClient> GetOtherOnlineClients(AblagenClient sendingClient)
        {
            return onlineClients.Values.Where(ac => ac != sendingClient).ToList();
        }

        private void HandleClientDisconnect(AblagenClient sendingClient, EventArgs e)
        {
            var item = onlineClients.First(kvp => kvp.Value == sendingClient);
            onlineClients.Remove(item.Key);
            logger.Debug($"{sendingClient.Name} removed");

            BroadcastMessage(sendingClient, $"-{sendingClient.Name}");
        }




        internal void ShutdownConnections()
        {
            // throw new NotImplementedException();
        }
    }
}
