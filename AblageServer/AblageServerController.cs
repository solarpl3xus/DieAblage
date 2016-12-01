﻿using CommunicationBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
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
        private bool echoMode;

        public AblageServerController(bool echoMode = false)
        {
            controlPort = int.Parse(ConfigurationManager.AppSettings["ControlPort"]);
            dataPort = int.Parse(ConfigurationManager.AppSettings["DataPort"]);
            this.echoMode = echoMode;
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

        internal void DistributeFileToClients(string filePath)
        {
            List<AblagenClient> recipientClients = GetOtherOnlineClients(null);

            DistributionRequestArgs distributionRequestArgs = new DistributionRequestArgs("Server", $"{filePath.Substring(filePath.LastIndexOf('\\') + 1)}", File.ReadAllBytes(filePath));
            for (int i = 0; i < recipientClients.Count; i++)
            {
                recipientClients[i].HandleDistributionRequest(distributionRequestArgs);
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
                        ablagenClient.ChatMessageReceive += HandleChatMessageReceive;
                        onlineClients[ipAddress] = ablagenClient;


                    }
                }
                catch (SocketException)
                {
                    tryAgain = true;
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception e)
                {
                    logger.Fatal("Error during server initialization", e);
                    throw e;
                }
            }
        }

        private void HandleChatMessageReceive(AblagenClient sendingClient, ChatMessageEventArgs e)
        {
            List<AblagenClient> recipients = GetOtherOnlineClients(sendingClient);
            for (int i = 0; i < recipients.Count; i++)
            {
                Telegram telegram = new Telegram(Constants.TelegramTypes.ChatMessage);
                telegram[Constants.TelegramFields.Sender] = sendingClient.Name;
                telegram[Constants.TelegramFields.Text] = e.Message;

                recipients[i].SendTelegram(telegram);
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
                catch (ThreadAbortException)
                {

                }
                catch (Exception e)
                {
                    logger.Fatal("Error during server initialization", e);
                    throw e;
                }
            }
        }


        private void BroadcastMessage(AblagenClient sendingClient, Telegram telegram)
        {
            List<AblagenClient> recipientClients = GetOtherOnlineClients(sendingClient);

            for (int i = 0; i < recipientClients.Count; i++)
            {
                int index = i;
                recipientClients[i].SendTelegram(telegram);
            }
        }

        private void HandleSignIn(AblagenClient sendingClient, EventArgs e)
        {
            Telegram telegram = new Telegram(Constants.TelegramTypes.OnlineNotification);
            telegram[Constants.TelegramFields.Name] = sendingClient.Name;
            BroadcastMessage(sendingClient, telegram);
        }


        private void HandleDistributeRequest(AblagenClient sendingClient, DistributionRequestArgs distributionRequestArgs)
        {
            List<AblagenClient> recipientClients = GetOtherOnlineClients(sendingClient);

            for (int i = 0; i < recipientClients.Count; i++)
            {
                recipientClients[i].HandleDistributionRequest(distributionRequestArgs);
            }
        }

        private List<AblagenClient> GetOtherOnlineClients(AblagenClient sendingClient)
        {
            return onlineClients.Values.Where(ac => (echoMode || ac != sendingClient)).ToList();
        }

        private void HandleClientDisconnect(AblagenClient sendingClient, EventArgs e)
        {
            var item = onlineClients.First(kvp => kvp.Value == sendingClient);
            onlineClients.Remove(item.Key);
            logger.Debug($"{sendingClient.Name} removed");


            Telegram telegram = new Telegram(Constants.TelegramTypes.OfflineNotification);
            telegram[Constants.TelegramFields.Name] = sendingClient.Name;
            BroadcastMessage(sendingClient, telegram);
        }




        public void ShutdownConnections()
        {
            tcpControlListener.Stop();
            controlListenThread.Abort();

            tcpDataListener.Stop();
            dataListenThread.Abort();

            List<AblagenClient> clientsToShutdown = GetOtherOnlineClients(null);
            for (int i = 0; i < clientsToShutdown.Count; i++)
            {
                try
                {
                    clientsToShutdown[i].StopCommunication();
                }
                catch (Exception e)
                {
                    logger.Error("Exception during client shutdown",  e);
                }
            }
        }
    }
}
