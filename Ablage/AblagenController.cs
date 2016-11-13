using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;

namespace Ablage
{

    internal class AblagenController
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Thread listenerThread;

        private Thread controlMessageThread;

        private TcpClient hostControlClient;
        private NetworkStream hostControlStream;
        private int bufferSize = 256;

        private TcpClient hostDataClient;

        private List<string> pendingFile;

        public AblagenController()
        {
            AblagenConfiguration.SetupConfiguration();

            pendingFile = new List<string>();

            ConnectToServer();
            StartListenerThread();
        }

        private void ConnectToServer()
        {            
            hostControlClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostControlPort);

            hostControlStream = hostControlClient.GetStream();

            hostControlStream.Write(Encoding.ASCII.GetBytes(AblagenConfiguration.ClientName), 0, AblagenConfiguration.ClientName.Length);

            logger.Debug($"logged in as {AblagenConfiguration.ClientName}");
        }

        internal void Disconnect()
        {
            hostControlStream.Close();
            hostControlClient.Close();
        }

        internal void SendFileToServer(string fileName)
        {
            new Thread(() =>
            {
                logger.Info($"Request send {fileName} to server");

                pendingFile.Add(fileName);
                SendMessage(hostControlClient, $"<{fileName.Substring(fileName.LastIndexOf('\\') + 1)}");
            }

            ).Start();
        }

        internal void SendFileToOnlineClients(string fileName)
        {
            string[] ips = GetOnlineClients();

            for (int i = 0; i < ips.Length; i++)
            {
                int index = i;
                new Thread(() => SendFile(ips[index], fileName)).Start();
                //Thread clientThread = new Thread(new ParameterizedThreadStart(SendFil111e));
                /*clientThreads.Add(clientThread);
                clientThread.Start(chatClient);
                listenerThread = new Thread(new ThreadStart(ListenAndAccept));
                listenerThread.Start();*/
            }
        }

        //        internal void SendFile(ob

        internal void SendFile(string ipAddress, string fileName)
        {
            logger.Info($"1Sending {fileName} to {ipAddress}");
            //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("2a02:908:1582:20e0:d4f5:c8f6:667e:8a4b"), 11000);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), AblagenConfiguration.HostDataPort);

            logger.Info($"2Sending {fileName} to {ipAddress}");

            // Create a TCP socket.

            Socket sendClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sendClient.Connect(ipEndPoint);

            /* SendMessage(sendClient, fileName.Substring(fileName.LastIndexOf('\\') + 1));

             string message = ReceiveMessage(sendClient);*/

            logger.Info($"Sending {fileName} to {ipAddress}");

            sendClient.SendFile(fileName);

            sendClient.Shutdown(SocketShutdown.Both);
            sendClient.Close();

            logger.Info($"Sent {fileName} to {ipAddress}");
        }

        private string[] GetOnlineClients()
        {
            string[] ips = new string[0];
            if (hostControlClient.Connected)
            {
                SendMessage(hostControlClient, $"!{AblagenConfiguration.ClientName}");
                string onlineClients = ReceiveMessage(hostControlClient);
                ips = onlineClients.Split(',').Select(t => t.Substring(0, t.LastIndexOf(':'))).ToArray();
            }
            return ips;
        }

        private void SendMessage(TcpClient tcpClient, string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(message);

            NetworkStream stream = tcpClient.GetStream();
            stream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }

        private string ReceiveMessage(TcpClient tcpClient)
        {
            NetworkStream netStream = tcpClient.GetStream();
            byte[] rawMessage = new byte[bufferSize];
            int bytesRead = netStream.Read(rawMessage, 0, bufferSize);
            if (bytesRead == 0)
            {
                logger.Debug("Says: client disconnected");
            }

            string message = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead).Trim('\0');

            return message;
        }



        internal void StartListenerThread()
        {
            listenerThread = new Thread(new ThreadStart(ListenAndAccept));
            listenerThread.Start();
        }

        private void ListenAndAccept()
        {
            try
            {
                while (true)
                {
                    int bytesRead;
                    var buffer = new byte[1024];
                    ASCIIEncoding encoder = new ASCIIEncoding();

                    bytesRead = hostControlStream.Read(buffer, 0, buffer.Length);
                    string message = encoder.GetString(buffer).Trim('\0');

                    if (message == "OK")
                    {
                        logger.Info($"Server confirmed request to send file");

                        string fileName = pendingFile.First();

                        if (hostDataClient == null || !hostDataClient.Connected)
                        {
                            hostDataClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostDataPort);
                        }

                        byte[] fileBytes = File.ReadAllBytes(fileName);
                        hostDataClient.GetStream().Write(fileBytes, 0, fileBytes.Length);

                        logger.Info($"Sent {fileName} to Server");

                        hostDataClient.GetStream().Close();
                        hostDataClient.Close();
                        pendingFile.Remove(fileName);

                        hostControlStream.Write(Encoding.ASCII.GetBytes("DISTRIBUTE"), 0, "DISTRIBUTE".Length);
                    }
                    else if (message.StartsWith(">"))
                    {
                        string fileName = message.Substring(1);
                        ReceiveFile(fileName);
                    }
                   

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(4919);
                throw;
            }
        }

        private void ReceiveFile(string fileName)
        {
            using (var output = File.Create(fileName))
            {
                Console.WriteLine("Client connected. Starting to receive the file");

                if (hostDataClient == null || !hostDataClient.Connected)
                {
                    hostDataClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostDataPort);
                }

                NetworkStream hostDataStream = hostDataClient.GetStream();

                int bytesRead;
                var buffer = new byte[1024];
                // read the file in chunks of 1KB
                while ((bytesRead = hostDataStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }

                hostDataClient.Close();
            }
        }


    }
}