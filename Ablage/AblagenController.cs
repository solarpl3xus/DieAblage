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
    enum MessageType
    {

        Unknown,
        ServerShutdown,
        AcceptSend,
        IncomingFileTransfer,
        OnlineNotification,
        OfflineNotification
    }

    internal class AblagenController
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Thread receiverThread;


        private TcpClient hostControlClient;
        private NetworkStream hostControlStream;
        private int bufferSize = 256;

        private TcpClient hostDataClient;

        private List<string> pendingFile;

        public MainForm Form { get; internal set; }

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


        private void StartListenerThread()
        {
            receiverThread = new Thread(new ThreadStart(Receive));
            receiverThread.Start();
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
                SendControlMessage($"<{fileName.Substring(fileName.LastIndexOf('\\') + 1)}");
            }

            ).Start();
        }


        private void SendControlMessage(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(message);

            hostControlStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }


        private void Receive()
        {
            try
            {
                while (true)
                {
                    string message = string.Empty;
                    MessageType messageType = ReceiveControlMessage(out message);

                    switch (messageType)
                    {
                        case MessageType.ServerShutdown:
                            break;
                        case MessageType.AcceptSend:
                            HandleFileUpload(message);
                            break;
                        case MessageType.IncomingFileTransfer:
                            HandleIncomingFileTransfer(message);
                            break;
                        case MessageType.OnlineNotification:
                            HandleOnlineNotification(message);
                            break;
                        case MessageType.OfflineNotification:
                            break;
                        case MessageType.Unknown:
                            logger.Debug("Unknow message type, discarding");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Fatal(e);
                Console.WriteLine(e);
                Environment.Exit(4919);
                throw;
            }
        }


        private MessageType ReceiveControlMessage(out string message)
        {
            message = string.Empty;
            MessageType messageType;

            byte[] rawMessage = new byte[bufferSize];
            int bytesRead;

            bytesRead = hostControlStream.Read(rawMessage, 0, bufferSize);
            message = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead).Substring(0, bytesRead);
            logger.Info($"> {message}");
            if (bytesRead == 0)
            {
                logger.Debug("Host shutdown");
                messageType = MessageType.ServerShutdown;
            }
            if (message == "OK")
            {
                messageType = MessageType.AcceptSend;
            }
            else if (message.StartsWith(">"))
            {
                messageType = MessageType.IncomingFileTransfer;
            }
            else if (message.StartsWith("+"))
            {
                messageType = MessageType.OnlineNotification;
            }
            else if (message.StartsWith("-"))
            {
                messageType = MessageType.OfflineNotification;
            }
            else
            {
                messageType = MessageType.Unknown;
            }

            return messageType;
        }



        private void HandleFileUpload(string message)
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


        private void HandleIncomingFileTransfer(string message)
        {
            string fileName = message.Substring(1);
            ReceiveFile(fileName);
        }

        private void ReceiveFile(string fileName)
        {
            string completePath = AdjustFilePathIfAlreadyExists(fileName);
            using (var output = File.Create(completePath))
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

        private string AdjustFilePathIfAlreadyExists(string fileName)
        {
            string completeFilePath = $"{AblagenConfiguration.OutputPath}{fileName}";
            int index = 0;
            while (File.Exists(completeFilePath))
            {
                index++;
                string alternateFileName = CreateAlternateFileName(fileName, index);
                completeFilePath = $"{AblagenConfiguration.OutputPath}{alternateFileName}";
            }
            return completeFilePath;
        }

        private string CreateAlternateFileName(string fileName, int nameIndex)
        {
            string alternateFileName = $"{fileName}({nameIndex})";
            if (fileName.Contains('.'))
            {
                int indexOFExtension = fileName.LastIndexOf('.');
                alternateFileName = $"{fileName.Substring(0, indexOFExtension)}({nameIndex}){fileName.Substring(indexOFExtension)}";
            }

            return alternateFileName;
        }


        private void HandleOnlineNotification(string message)
        {
            string balloonMessage = $"{message.Substring(1)} has come online";
            Form.ShowBalloonMessage(balloonMessage);
        }
    }
}