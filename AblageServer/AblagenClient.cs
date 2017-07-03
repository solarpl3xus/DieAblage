using CommunicationBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AblageServer
{    
    public class AblagenClient
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void DistributeRequestHandler(AblagenClient sendingClient, DistributionRequestArgs e);
        public event DistributeRequestHandler DistributeRequest;

        public delegate void DisconnectHandler(AblagenClient sendingClient, EventArgs e);
        public event DisconnectHandler Disconnect;

        public delegate void SignInHandler(AblagenClient sendingClient, EventArgs e);
        public event SignInHandler SignIn;

        public delegate void ChatMessageReceiveHandler(AblagenClient sendingClient, ChatMessageEventArgs e);
        public event ChatMessageReceiveHandler ChatMessageReceive;


        private int bufferSize = 256;

        private TcpClient controlClient;
        Thread controlCommunicationThread;
        private NetworkStream controlStream;

        private List<string> pendingUploads;
        List<byte[]> pendingDownloads;
        private TelegramParser telegramParser;

        public AblagenClient(TcpClient client)
        {
            controlClient = client;
            pendingUploads = new List<string>();
            pendingDownloads = new List<byte[]>();

            telegramParser = new TelegramParser();
        }

        public string Name { get; private set; }

        public string IpAddress
        {
            get
            {
                return ((IPEndPoint)controlClient.Client.RemoteEndPoint).Address.ToString(); 
            }
        }

        public string Identifier
        {
            get
            {
                return $"{IpAddress}-{Name}";
            }
        }

        internal void StartCommunication()
        {
            controlCommunicationThread = new Thread(HandleControlCommunication);
            controlCommunicationThread.Start();
        }

        internal void StopCommunication()
        {
            controlCommunicationThread.Abort();
        }

        private void HandleControlCommunication()
        {
            controlStream = controlClient.GetStream();

            Name = string.Empty;
        
            try
            {
                bool clientConnected = true;
                while (clientConnected)
                {                
                    Telegram signInTelegram = ReceiveControlTelegram();
                    if (signInTelegram.TelegramType == Constants.TelegramTypes.SignIn)
                    {
                        Name = signInTelegram[Constants.TelegramFields.Name];
                        logger.Info($"{Name} signed in");
                        SignIn?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        clientConnected = false;
                        logger.Debug(Name + " says: client disconnected");
                    }


                    while (clientConnected)
                    {
                        Telegram telegram = ReceiveControlTelegram();

                        switch (telegram.TelegramType)
                        {
                            case Constants.TelegramTypes.Disconnect:
                                clientConnected = false;
                                break;
                            case Constants.TelegramTypes.ByteArrayTransferRequest:
                                HandleByteUploadRequest(telegram);
                                break;
                            case Constants.TelegramTypes.FileTransferRequest:
                                HandleFileUploadRequest(telegram);
                                break;
                            case Constants.TelegramTypes.WatchdogRequest:
                                HandleWatchdogRequest(telegram);
                                break;
                            case Constants.TelegramTypes.ChatMessage:
                                DistributeChatMessage(telegram);
                                break;
                            case Constants.TelegramTypes.Unknown:
                                logger.Debug("Unknown Message received, discarding");
                                break;
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
                if (controlClient != null)
                {
                    controlClient.Close();
                }

                Disconnect?.Invoke(this, new EventArgs());
            }
        }

        private void DistributeChatMessage(Telegram telegram)
        {
            string chatMessage = telegram[Constants.TelegramFields.Text];
            ChatMessageReceive?.Invoke(this, new ChatMessageEventArgs(chatMessage));
        }

        private Telegram ReceiveControlTelegram()
        {
            byte[] rawMessage = new byte[bufferSize];
            int bytesRead;

            bytesRead = controlStream.Read(rawMessage, 0, bufferSize);
            string message = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead);
            logger.Info($"{Name} > {message}");

            Telegram telegram;
            if (bytesRead == 0)
            {
                logger.Debug(Name + " says: client disconnected");
                telegram = new Telegram(Constants.TelegramTypes.Disconnect);
            }
            else
            {
                telegram = telegramParser.ParseMessage(message);
            }

            return telegram;
        }


        public void SendTelegram(Telegram telegram)
        {
            string message = telegramParser.ParseTelegram(telegram);
            SendControlMessage(message);
        }

        private void SendControlMessage(string message)
        {
            controlStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }

        private void HandleWatchdogRequest(Telegram telegram)
        {
            Telegram sendTelegram = new Telegram(Constants.TelegramTypes.WatchdogReply);
            SendTelegram(sendTelegram);
        }
        

        private void HandleByteUploadRequest(Telegram telegram)
        {
            string fileName = telegram[Constants.TelegramFields.FileName];

            pendingUploads.Add(fileName);

            logger.Info($"Request to receive {fileName} from {Name}");

            Telegram sendTelegram = new Telegram(Constants.TelegramTypes.ByteSendAccept);
            sendTelegram[Constants.TelegramFields.FileName] = fileName;
            SendTelegram(sendTelegram);
        }


        private void HandleFileUploadRequest(Telegram telegram)
        {
            string fileName = telegram[Constants.TelegramFields.FileName];

            pendingUploads.Add(fileName);

            logger.Info($"Request to receive {fileName} from {Name}");

            Telegram sendTelegram = new Telegram(Constants.TelegramTypes.FileSendAccept);
            SendTelegram(sendTelegram);            
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

       

        internal void HandleIncomingDataConnection(TcpClient dataclient)
        {
            new Thread(() =>
            {
                if (pendingUploads.Any())
                {
                    string fileName = pendingUploads.First();

                    NetworkStream dataStream = dataclient.GetStream();

                    dataStream.Write(Encoding.ASCII.GetBytes("GO"), 0, "GO".Length);

                    var buffer = new byte[1024];

                    int bytesRead = 0;
                    List<byte[]> bufferBlocks = new List<byte[]>();
                    while ((bytesRead = dataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bufferBlocks.Add(SubArray(buffer, 0, bytesRead));
                        buffer = new byte[1024];
                    }

                    byte[] bufferedFile = new byte[bufferBlocks.Sum(a => a.Length)];
                    int offset = 0;

                    for (int i = 0; i < bufferBlocks.Count; i++)
                    {
                        Buffer.BlockCopy(bufferBlocks[i], 0, bufferedFile, offset, bufferBlocks[i].Length);
                        offset += bufferBlocks[i].Length;
                    }
                    pendingUploads.Remove(fileName);
                    dataclient.Close();


                    if (DistributeRequest != null)
                    {
                        DistributionRequestArgs distributeRequest = new DistributionRequestArgs(Name, fileName, bufferedFile);
                        DistributeRequest(this, distributeRequest);
                    }

                }
                else
                {
                    if (pendingDownloads.Any())
                    {
                        byte[] fileBytes = pendingDownloads.First();

                        dataclient.GetStream().Write(fileBytes, 0, fileBytes.Length);
                        logger.Info($"Sent file to client {dataclient.Client.LocalEndPoint.ToString()}");
                        dataclient.GetStream().Close();
                        dataclient.Close();

                        pendingDownloads.RemoveAt(0);
                    }
                }
            }).Start();
        }

        internal void HandleDistributionRequest(DistributionRequestArgs distributionRequestArgs)
        {
            new Thread(() =>
            {
                try
                {
                    pendingDownloads.Add(distributionRequestArgs.FileBytes);
                    Telegram telegram = new Telegram(Constants.TelegramTypes.IncomingFileTransfer);
                    telegram[Constants.TelegramFields.FileName] = distributionRequestArgs.FileName;
                    telegram[Constants.TelegramFields.FileSize] = distributionRequestArgs.FileBytes.Length.ToString();
                    telegram[Constants.TelegramFields.Sender] = distributionRequestArgs.Sender;

                    SendTelegram(telegram);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }).Start();
        }
    }


}
