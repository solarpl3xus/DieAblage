using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AblageServer
{
    enum MessageType
    {
        Disconnect,
        Upload,
        Unknown
    }

    public class AblagenClient
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void DistributeRequestHandler(AblagenClient sendingClient, DistributionRequestArgs e);
        public event DistributeRequestHandler DistributeRequest;

        private int bufferSize = 256;

        private TcpClient controlClient;
        Thread controlCommunicationThread;
        private NetworkStream controlStream;

        private List<string> pendingUploads;
        List<byte[]> pendingDownloads;

        public AblagenClient(TcpClient client)
        {
            controlClient = client;
            pendingUploads = new List<string>();
            pendingDownloads = new List<byte[]>();
        }

        public string Name { get; private set; }

        internal void StartCommunication()
        {
            controlCommunicationThread = new Thread(HandleControlCommunication);
            controlCommunicationThread.Start();
        }

        private void HandleControlCommunication()
        {
            controlStream = controlClient.GetStream();

            Name = string.Empty;

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
                        logger.Debug(Name + " says: client disconnected");
                        break;
                    }
                    Name = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead);
                    logger.Info($"{Name} signed in");

                    bool clientConnected = true;
                    while (clientConnected)
                    {
                        bytesRead = 0;

                        string message = string.Empty;
                        MessageType messageType = ReceiveControlMessage(out message);


                        switch (messageType)
                        {
                            case MessageType.Disconnect:
                                clientConnected = false;
                                break;
                            case MessageType.Upload:
                                HandleUploadRequest(message);
                                break;
                            case MessageType.Unknown:
                                break;
                            default:
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
            }
        }

        private MessageType ReceiveControlMessage(out string message)
        {
            message = string.Empty;
            MessageType messageType;

            byte[] rawMessage = new byte[bufferSize];
            int bytesRead;



            bytesRead = controlStream.Read(rawMessage, 0, bufferSize);
            message = Encoding.ASCII.GetString(rawMessage).Substring(0, bytesRead);
            logger.Info($"> {message}");
            if (bytesRead == 0)
            {
                logger.Debug(Name + " says: client disconnected");
                messageType = MessageType.Disconnect;
            }
            else if (message.StartsWith("<"))
            {
                messageType = MessageType.Upload;
            }
            else
            {
                messageType = MessageType.Unknown;
            }

            return messageType;
        }

        private void HandleUploadRequest(string message)
        {
            string fileName = message.Substring(1);

            pendingUploads.Add(fileName);

            logger.Info($"Request to receive {fileName} from {Name}");
            controlStream.Write(Encoding.ASCII.GetBytes("OK"), 0, "OK".Length);
            logger.Info("Confirmed request");
        }

        internal void HandleIncomingDataConnection(TcpClient dataclient)
        {
            new Thread(() =>
            {
                if (pendingUploads.Any())
                {
                    string fileName = pendingUploads.First();

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
                    pendingUploads.Remove(fileName);
                    dataclient.Close();


                    if (DistributeRequest != null)
                    {
                        DistributionRequestArgs distributeRequest = new DistributionRequestArgs(fileName, bufferedFile);
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
                    controlStream.Write(Encoding.ASCII.GetBytes(">" + distributionRequestArgs.FileName), 0, (">" + distributionRequestArgs.FileName).Length);
                }
                catch (Exception e)
                {
                    e.ToString();
                    throw;
                }
            }).Start();
        }
    }


}
