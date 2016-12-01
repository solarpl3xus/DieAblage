using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationBase
{
    public static class Constants
    {
        public static class TelegramTypes
        {
            public const string WatchdogReply = "PONG";
            public const string FileSendAccept = "OK";
            public const string ByteSendAccept = "OK!";
            public const string IncomingFileTransfer = ">";
            public const string FileTransferRequest = "<";
            public const string ByteArrayTransferRequest = "<!";

            public const string OnlineNotification = "+";
            public const string OfflineNotification = "-";
            public const string ChatMessage = "!";

            public const string Unknown = "???";
            public const string WatchdogRequest = "PING";
            public const string SignIn = "*";
            public const string HostShutdown = "Hostshutdown";
            public const string Disconnect = "Disconnect";
        }

        public static class TelegramFields
        {
            public const string FileName = "FileName";
            public const string Text = "Text";
            public const string Name = "Name";
            public const string FileSize = "FileSize";
            public const string Sender = "Sender";
        }
    }
}
