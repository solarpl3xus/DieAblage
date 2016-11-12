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
        private int bufferSize = 256;



        private Dictionary<string, string> onlineIps;
        private Dictionary<string, TcpClient> onlineControlClients;

        private List<Tuple<string, string>> pendingServerUploads;
        private List<Tuple<string, byte[]>> pendingServerDownloads;

        private Dictionary<string, AblagenClient> onlineClients = new Dictionary<string, AblagenClient>();

        public AblageServerController()
        {
            controlPort = int.Parse(ConfigurationManager.AppSettings["ControlPort"]);
            dataPort = int.Parse(ConfigurationManager.AppSettings["DataPort"]);

            onlineIps = new Dictionary<string, string>();
            onlineControlClients = new Dictionary<string, TcpClient>();

            pendingServerUploads = new List<Tuple<string, string>>();
            pendingServerDownloads = new List<Tuple<string, byte[]>>();
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

                        string ipAddress = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                        logger.Debug($"Client connected {ipAddress}");

                        if (onlineClients.ContainsKey(ipAddress))
                        {
                            logger.Error("Already exists");
                        }
                        else
                        {
                            AblagenClient ablagenClient = new AblagenClient(client);
                            ablagenClient.StartCommunication();
                            ablagenClient.DistributeRequest += HandleDistributeRequest;
                            onlineClients[ipAddress] = ablagenClient;
                        }

                     /*   Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                        clientThread.Start(client);*/
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

        private void HandleDistributeRequest(AblagenClient sendingClient, DistributionRequestArgs distributionRequestArgs)
        {
            List<AblagenClient> recipientClients = onlineClients.Values.Where(ac => ac != sendingClient).ToList();
            for (int i = 0; i < recipientClients.Count; i++)
            {

                //pendingServerDownloads.Add(new Tuple<string, byte[]>(tcpClients[i].Client.LocalEndPoint.ToString().Replace(controlPort.ToString(), dataPort.ToString()), bufferedFile));
                int index = i;
                recipientClients[i].HandleDistributionRequest(distributionRequestArgs);

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
                        //blocks until a client has connected to the server
                        TcpClient dataclient = tcpDataListener.AcceptTcpClient();

                        string ipAddress = ((IPEndPoint)dataclient.Client.LocalEndPoint).Address.ToString();
                        logger.Debug($"Data client connected {ipAddress}");

                        if (!onlineClients.ContainsKey(ipAddress))
                        {
                            logger.Error("No control client exists");
                        }
                        else
                        {
                            onlineClients[ipAddress].HandleIncomingDataConnection(dataclient);

                            /*AblagenClient ablagenClient = new AblagenClient(client);
                            ablagenClient.StartCommunication();
                            onlineClients[ipAddress] = ablagenClient;*/
                        }

                        Tuple<string, string> pendingServerUpload = pendingServerUploads.Where(k => k.Item1 == dataclient.Client.LocalEndPoint.ToString()).FirstOrDefault();
                        if (pendingServerUpload != null)
                        {
                            string fileName = pendingServerUpload.Item2;

                            NetworkStream dataStream = dataclient.GetStream();

                            var buffer = new byte[1024];

                            int bytesRead = 0;
                            List<byte[]> bufferBlocks = new List<byte[]>();
                            while ((bytesRead = dataStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bufferBlocks.Add(buffer);
                                buffer = new byte[1024];
                            }

                            byte[] bufferedFile = new byte[bufferBlocks.Sum(a => a.Length)];
                            int offset = 0;

                            for (int i = 0; i < bufferBlocks.Count; i++)
                            {
                                Buffer.BlockCopy(bufferBlocks[i], 0, bufferedFile, offset, bufferBlocks[i].Length);
                                offset += bufferBlocks[i].Length;
                            }
                            pendingServerUploads.Remove(pendingServerUpload);
                            dataclient.Close();
                            List<TcpClient> tcpClients = onlineControlClients.Values.ToList();
                            for (int i = 0; i < tcpClients.Count; i++)
                            {
                                pendingServerDownloads.Add(new Tuple<string, byte[]>(tcpClients[i].Client.LocalEndPoint.ToString().Replace(controlPort.ToString(), dataPort.ToString()), bufferedFile));
                                int index = i;
                                new Thread(() =>
                                {
                                    try
                                    {
                                        NetworkStream controlStream = tcpClients[index].GetStream();
                                        controlStream.Write(Encoding.ASCII.GetBytes(">" + fileName), 0, (">" + fileName).Length);
                                    }
                                    catch (Exception e)
                                    {
                                        e.ToString();
                                        throw;
                                    }
                                }).Start();
                            }
                        }
                        else
                        {
                            Tuple<string, byte[]> pendingServerDownload = pendingServerDownloads.Where(k => k.Item1 == dataclient.Client.LocalEndPoint.ToString()).FirstOrDefault();
                            if (pendingServerDownload != null)
                            {
                                byte[] fileBytes = pendingServerDownload.Item2;
                                dataclient.GetStream().Write(fileBytes, 0, fileBytes.Length);
                                logger.Info($"Sent file to client {dataclient.Client.LocalEndPoint.ToString()}");
                                dataclient.GetStream().Close();
                                dataclient.Close();

                                pendingServerDownloads.Remove(pendingServerDownload);
                            }
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



        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream controlStream = tcpClient.GetStream();

            string name = string.Empty;

            byte[] rawMessage = new byte[bufferSize];
            int bytesRead;
            try
            {
                while (true)
                {
                    bytesRead = 0;
                    bytesRead = controlStream.Read(rawMessage, 0, bufferSize);
                    if (bytesRead == 0)
                    {
                        logger.Debug(name + " says: client disconnected");
                        break;
                    }
                    name = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead);
                    onlineIps[name] = tcpClient.Client.LocalEndPoint.ToString();
                    onlineControlClients[name] = tcpClient;
                    logger.Info($"{name} signed in");


                    //message has successfully been received
                    //   message = Regex.Match(Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead), @"\d+").Value;

                    while (true)
                    {
                        bytesRead = 0;


                        bytesRead = controlStream.Read(rawMessage, 0, bufferSize);
                        if (bytesRead == 0)
                        {
                            logger.Debug(name + " says: client disconnected");
                            break;
                        }

                        string message = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead);
                        logger.Info($"> {message}");
                        if (message.StartsWith("?"))
                        {
                            string clientToLookFor = message.Substring(1);

                            logger.Debug($"{name} wants the address of {clientToLookFor}");
                            string ipAddress = "?";
                            if (onlineIps.ContainsKey(clientToLookFor))
                            {
                                ipAddress = onlineIps[clientToLookFor];
                                logger.Debug($"which is {ipAddress}");
                            }
                            else
                            {
                                logger.Warn("which is not online");
                            }

                            controlStream.Write(Encoding.ASCII.GetBytes(ipAddress), 0, ipAddress.TrimEnd().Length);
                        }
                        if (message.StartsWith("!"))
                        {
                            string requestingClient = message.Substring(1);
                            string ips = string.Join(",", onlineIps.Keys.Where(k => k != requestingClient).Select(k => onlineIps[k]).ToArray());
                            controlStream.Write(Encoding.ASCII.GetBytes(ips), 0, ips.Length);
                        }
                        if (message.StartsWith("<"))
                        {
                            string fileName = message.Substring(1);

                            pendingServerUploads.Add(new Tuple<string, string>(tcpClient.Client.LocalEndPoint.ToString().Replace(controlPort.ToString(), dataPort.ToString()), fileName));

                            logger.Info($"Request to receive {fileName} from {tcpClient.Client.LocalEndPoint.ToString()}");
                            controlStream.Write(Encoding.ASCII.GetBytes("OK"), 0, "OK".Length);
                            logger.Info("Confirmed request");
                        }
                        else if (message == "ping")
                        {
                            controlStream.Write(Encoding.ASCII.GetBytes("pong"), 0, "pong".Length);
                        }


                    }

                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            finally
            {
                if (controlStream != null)
                {
                    controlStream.Close();
                }
                if (tcpClient != null)
                {
                    tcpClient.Close();
                }
            }


        }


        internal void ShutdownConnections()
        {
            // throw new NotImplementedException();
        }
    }
}
