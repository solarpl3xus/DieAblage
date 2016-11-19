using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Ablage
{
    enum MessageType
    {

        Unknown,
        ServerShutdown,
        AcceptFileSend,
        IncomingFileTransfer,
        OnlineNotification,
        OfflineNotification,
        AcceptByteSend,
        WatchdogReply
    }

    internal class AblagenController
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Thread receiverThread;

        private bool running = true;

        private TcpClient hostControlClient;
        private NetworkStream hostControlStream;
        private int bufferSize = 256;

        private TcpClient hostDataClient;

        private List<string> pendingFile;
        private List<byte[]> pendingBytes;
        private Thread watchdogThread;
        private bool connected;

        public MainForm Form { get; internal set; }

        public AblagenController()
        {
            AblagenConfiguration.SetupConfiguration();
            pendingFile = new List<string>();
            pendingBytes = new List<byte[]>();
        }

        public AblagenController(MainForm mainForm) : this()
        {
            Form = mainForm;

            if (string.IsNullOrEmpty(AblagenConfiguration.ClientName))
            {
                string name = Form.PromptForName();
                AblagenConfiguration.SaveName(name);
            }
        }


        public void Start()
        {
            ConnectToServer();
            StartListenerThread();
        }

        private void ConnectToServer()
        {
            connected = false;
            bool retry = false;
            int logEntries = 0;

            while (!connected)
            {
                if (retry)
                {
                    Thread.Sleep(5000);
                }
                try
                {
                    hostControlClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostControlPort);
                    connected = true;
                }
                catch (Exception e)
                {
                    if (logEntries < 5)
                    {
                        logger.Error("Could not connect to server", e);
                    }
                    logEntries++;
                    retry = true;
                }
            }

            hostControlStream = hostControlClient.GetStream();

            hostControlStream.Write(Encoding.ASCII.GetBytes(AblagenConfiguration.ClientName), 0, AblagenConfiguration.ClientName.Length);

            logger.Debug($"logged in as {AblagenConfiguration.ClientName}");
        }


        private void StartListenerThread()
        {
            receiverThread = new Thread(new ThreadStart(ReceiveControlMessages));
            receiverThread.Start();

            watchdogThread = new Thread(new ThreadStart(SendWatchdog));
            watchdogThread.Start();
        }

        private void SendWatchdog()
        {
            Semaphore semaphore = new Semaphore(0, 1);
            while (true)
            {
                semaphore.WaitOne(30000);
                if (connected)
                {
                    SendControlMessage("PING"); 
                }
            }
        }

        internal void Disconnect()
        {
            hostControlStream.Close();
            hostControlClient.Close();
        }

        internal void HandlePaste()
        {
            if (Clipboard.ContainsFileDropList())
            {
                string[] filePaths = Clipboard.GetFileDropList().Cast<string>().ToArray();
                for (int i = 0; i < filePaths.Length; i++)
                {
                    SendFileToServer(filePaths[i]);
                }
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] byteArray = ms.ToArray();
                SendByteArrayToServer($"Screenshot{DateTime.Now.Ticks}.png", byteArray);
            }
        }

        private void SendByteArrayToServer(string fileName, byte[] byteArray)
        {
            new Thread(() =>
            {
                logger.Info($"Request send byte array {fileName} to server");

                pendingBytes.Add(byteArray);
                SendControlMessage($"<!{fileName}");
            }

            ).Start();
        }

        internal void SendFileToServer(string filePath)
        {
            new Thread(() =>
            {
                logger.Info($"Request send {filePath} to server");

                pendingFile.Add(filePath);
                SendControlMessage($"<{filePath.Substring(filePath.LastIndexOf('\\') + 1)}");
            }

            ).Start();
        }


        private void SendControlMessage(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(message);
            logger.Info($"<{message}");
            hostControlStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }


        private void ReceiveControlMessages()
        {
            try
            {
                bool serverOnline = true;
                do
                {
                    string message = string.Empty;
                    MessageType messageType = ReceiveControlMessage(out message);

                    switch (messageType)
                    {
                        case MessageType.ServerShutdown:
                            serverOnline = false;
                            break;
                        case MessageType.AcceptFileSend:
                            HandleFileUpload(message);
                            break;
                        case MessageType.AcceptByteSend:
                            HandleByteUpload(message);
                            break;
                        case MessageType.IncomingFileTransfer:
                            HandleIncomingFileTransfer(message);
                            break;
                        case MessageType.OnlineNotification:
                            HandleOnlineNotification(message);
                            break;
                        case MessageType.OfflineNotification:
                            HandleOfflineNotification(message);
                            break;
                        case MessageType.Unknown:
                            logger.Debug("Unknow message type, discarding");
                            break;
                        default:
                            break;
                    }
                } while (serverOnline);
            }
            catch (Exception e)
            {
                //2016 - 11 - 13 20:15:23,106[11] FATAL Ablage.AblagenController[(null)] - System.IO.IOException: Von der Übertragungsverbindung können keine Daten gelesen werden: Eine vorhandene Verbindung wurde vom Remotehost geschlossen. --->System.Net.Sockets.SocketException: Eine vorhandene Verbindung wurde vom Remotehost geschlossen
                if (running)
                {
                    logger.Error("Exception during message processing", e);
                }
            }
            finally
            {
                Disconnect();
                if (running)
                {
                    new Thread(Start).Start();
                }
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
            else if (message.StartsWith("PONG"))
            {
                messageType = MessageType.WatchdogReply;
            }
            else if (message == "OK")
            {
                messageType = MessageType.AcceptFileSend;
            }
            else if (message.StartsWith("OK!"))
            {
                messageType = MessageType.AcceptByteSend;
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


        private void HandleByteUpload(string message)
        {
            logger.Info($"Server confirmed request to send file");


            if (hostDataClient == null || !hostDataClient.Connected)
            {
                hostDataClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostDataPort);
            }

            byte[] bytes = pendingBytes.First();
            hostDataClient.GetStream().Write(bytes, 0, bytes.Length);

            logger.Info($"Sent bytes to Server");

            hostDataClient.GetStream().Close();
            hostDataClient.Close();
            pendingBytes.RemoveAt(0);            
        }


        private void HandleIncomingFileTransfer(string message)
        {
            string fileName = message.Substring(1);
            ReceiveFile(fileName);
        }

        private void ReceiveFile(string fileName)
        {
            string completePath = AdjustFilePathIfAlreadyExists(fileName);

            try
            {
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
                    while ((bytesRead = hostDataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }

                    hostDataClient.Close();

                    if (AblagenConfiguration.OpenFileAutomatically(completePath))
                    {
                        System.Diagnostics.Process.Start(completePath);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
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
            string clientName = message.Substring(1);
            Form.RegisterOnlineClient(clientName);
        }


        private void HandleOfflineNotification(string message)
        {
            string clientName = message.Substring(1);
            Form.DeregisterOnlineClient(clientName);
        }

        internal void Shutdown()
        {
            running = false;
            receiverThread.Abort();
            Disconnect();
        }
    }
}