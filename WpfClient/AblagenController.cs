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
using CommunicationBase;
using AblageClient;

namespace AblageClient
{
    public class AblagenController
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
        private TelegramParser telegramParser;

        public ClientForm Form { get; set; }

        public AblagenController()
        {
            AblagenConfiguration.SetupConfiguration();
            pendingFile = new List<string>();
            pendingBytes = new List<byte[]>();
        }

        public AblagenController(ClientForm mainForm) : this()
        {
            Form = mainForm;
            telegramParser = new TelegramParser();

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

                Form.DisplayIsConnected(connected);
            }

            hostControlStream = hostControlClient.GetStream();

            Telegram signInTelegram = new Telegram(Constants.TelegramTypes.SignIn);
            signInTelegram[Constants.TelegramFields.Name] = AblagenConfiguration.ClientName;
            SendTelegram(signInTelegram);

            logger.Debug($"logged in as {AblagenConfiguration.ClientName}");
        }


        private void StartListenerThread()
        {
            receiverThread = new Thread(new ThreadStart(ReceiveControlMessages));
            receiverThread.Start();

            if (watchdogThread == null || !watchdogThread.IsAlive)
            {
                watchdogThread = new Thread(new ThreadStart(SendWatchdog));
                watchdogThread.Start();
            }
        }

        private void SendWatchdog()
        {
            Semaphore semaphore = new Semaphore(0, 1);
            while (true)
            {
                semaphore.WaitOne(30000);
                if (connected)
                {
                    SendTelegram(new Telegram(Constants.TelegramTypes.WatchdogRequest));
                }
            }
        }

        internal void Disconnect()
        {
            hostControlStream?.Close();
            hostControlClient?.Close();
        }

        public void HandlePaste()
        {
            if (Clipboard.ContainsFileDropList())
            {
                string[] filePaths = Clipboard.GetFileDropList().Cast<string>().ToArray();
                SendFilesToServer(filePaths);
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] byteArray = ms.ToArray();
                SendByteArrayToServer($"Screenshot{DateTime.Now.Ticks}.png", byteArray);


                System.Windows.Controls.Image chatStreamImage = Helper.ConvertDrawingImageToControlsImage(image);
                Form.AddImageToChatStream(AblagenConfiguration.ClientName, chatStreamImage);
            }
        }

        public void SendFilesToServer(string[] filePaths)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                SendFileToServer(filePaths[i]);
                Form.AddFileToChatStream(AblagenConfiguration.ClientName, filePaths[i]);
            }
        }

        private void SendByteArrayToServer(string fileName, byte[] byteArray)
        {
            new Thread(() =>
            {
                logger.Info($"Request send byte array {fileName} to server");

                pendingBytes.Add(byteArray);

                Telegram telegram = new Telegram(Constants.TelegramTypes.ByteArrayTransferRequest);                
                telegram[Constants.TelegramFields.FileName] = fileName;
                SendTelegram(telegram);
            }

            ).Start();
        }

        public void SendFileToServer(string filePath)
        {
            new Thread(() =>
            {
                logger.Info($"Request send {filePath} to server");

                pendingFile.Add(filePath);

                Telegram fileTransferRequest = new Telegram(Constants.TelegramTypes.FileTransferRequest);
                fileTransferRequest[Constants.TelegramFields.FileName] = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                SendTelegram(fileTransferRequest);
            }

            ).Start();
        }


        public void SendTelegram(Telegram telegram)
        {
            string message = telegramParser.ParseTelegram(telegram);
            SendControlMessage(message);
        }

        private void SendControlMessage(string message)
        {            
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

                    //MessageType messageType = ReceiveControlMessage(out message);

                    Telegram telegram = ReceiveControlMessage();

                    switch (telegram.TelegramType)
                    {
                        case Constants.TelegramTypes.HostShutdown:
                            serverOnline = false;
                            break;
                        case Constants.TelegramTypes.FileSendAccept:
                            HandleFileUpload(telegram);
                            break;
                        case Constants.TelegramTypes.ByteSendAccept:
                            HandleByteUpload(telegram);
                            break;
                        case Constants.TelegramTypes.IncomingFileTransfer:
                            HandleIncomingFileTransfer(telegram);
                            break;
                        case Constants.TelegramTypes.OnlineNotification:
                            HandleOnlineNotification(telegram);
                            break;
                        case Constants.TelegramTypes.OfflineNotification:
                            HandleOfflineNotification(telegram);
                            break;
                        case Constants.TelegramTypes.ChatMessage:
                            ReceiveChatMessage(telegram);
                            break;
                        case Constants.TelegramTypes.Unknown:
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

        private void ReceiveChatMessage(Telegram telegram)
        {
            string sender = telegram[Constants.TelegramFields.Sender];
            string chatMessage = telegram[Constants.TelegramFields.Text];
            Form.AddChatMessageToChatStream(sender, chatMessage);
        }

        private Telegram ReceiveControlMessage()
        {
            byte[] rawMessage = new byte[bufferSize];
            int bytesRead;

            bytesRead = hostControlStream.Read(rawMessage, 0, bufferSize);
            string message = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead).Substring(0, bytesRead);
            logger.Info($"> {message}");

            Telegram telegram;
            if (bytesRead == 0)
            {
                telegram = new Telegram(Constants.TelegramTypes.HostShutdown);
            }
            else
            {
                telegram = telegramParser.ParseMessage(message);
            }

            return telegram;
        }


        private void HandleFileUpload(Telegram telegram)
        {
            logger.Info($"Server confirmed request to send file");

            string fileName = pendingFile.First();
            byte[] fileBytes = File.ReadAllBytes(fileName);
            UploadBytes(fileBytes, fileName);

            logger.Info($"Sent {fileName} to Server");

            pendingFile.Remove(fileName);
        }

        private void UploadBytes(byte[] fileBytes, string fileName)
        {
            if (hostDataClient == null || !hostDataClient.Connected)
            {
                hostDataClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostDataPort);
            }

            logger.Info($"<{AblagenConfiguration.ClientName}");
            hostDataClient.GetStream().Write(Encoding.ASCII.GetBytes(AblagenConfiguration.ClientName), 0, AblagenConfiguration.ClientName.Length);

            byte[] rawMessage = new byte[bufferSize];
            int bytesRead = hostDataClient.GetStream().Read(rawMessage, 0, bufferSize);

            if (bytesRead > 0)
            {
                fileBytes = Encryption.Encrypt(fileBytes, "kackbratze");
                for (int offset = 0; offset < fileBytes.Length; offset += 1024)
                {
                    int size = Math.Min(1024, fileBytes.Length - offset);
                    hostDataClient.GetStream().Write(fileBytes, offset, size);
                    Form.ReportUploadProgess((int)((long)offset * 100 / fileBytes.Length), fileName);
                }
                Form.ReportUploadProgess(100, fileName);
            }

            hostDataClient.GetStream().Close();
            hostDataClient.Close();
        }

        private void HandleByteUpload(Telegram telegram)
        {
            logger.Info($"Server confirmed request to send file");

            byte[] bytes = pendingBytes.First();

            UploadBytes(bytes, "Image");

            logger.Info($"Sent bytes to Server");

            pendingBytes.RemoveAt(0);
        }


        private void HandleIncomingFileTransfer(Telegram telegram)
        {
            string fileName = telegram[Constants.TelegramFields.FileName];
            int size = int.Parse(telegram[Constants.TelegramFields.FileSize]);
            string sender = telegram[Constants.TelegramFields.Sender];

            ReceiveFile(sender, fileName, size);
        }

        private void ReceiveFile(string sender, string fileName, int size)
        {
            string completePath = AdjustFilePathIfAlreadyExists(fileName);

            try
            {
                using (var output = File.Create(completePath))
                {
                    logger.Info("Client connected. Starting to receive the file");

                    if (hostDataClient == null || !hostDataClient.Connected)
                    {
                        hostDataClient = new TcpClient(AblagenConfiguration.HostIp, AblagenConfiguration.HostDataPort);
                    }


                    Form.HandleFileDownloadStarted(sender, completePath);

                    NetworkStream hostDataStream = hostDataClient.GetStream();

                    hostDataStream.Write(Encoding.ASCII.GetBytes(AblagenConfiguration.ClientName), 0, AblagenConfiguration.ClientName.Length);

                    int totalByteRead = 0;
                    int bytesRead;
                    var buffer = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        while ((bytesRead = hostDataStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, bytesRead);
                            totalByteRead += bytesRead;

                            Form.ReportDownloadProgess((int)((long)totalByteRead * 100 / size), completePath);
                        }
                        byte[] byteCache = Encryption.Decrypt(ms.ToArray(), "kackbratze");
                        output.Write(byteCache, 0, byteCache.Length);
                        Form.ReportDownloadProgess(100, completePath);
                    }
                    hostDataClient.Close();
                }
                Form.HandleFileDownloadCompleted(sender, completePath);
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


        private void HandleOnlineNotification(Telegram telegram)
        {
            string clientName = telegram[Constants.TelegramFields.Name];
            Form.RegisterOnlineClient(clientName);
        }


        private void HandleOfflineNotification(Telegram telegram)
        {
            string clientName = telegram[Constants.TelegramFields.Name];
            Form.DeregisterOnlineClient(clientName);
        }

        public void Shutdown()
        {
            running = false;
            receiverThread?.Abort();
            watchdogThread?.Abort();
            Disconnect();
        }
    }
}