using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationBase
{
    public class TelegramParser
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        Dictionary<string, string[]> telegramDefinitions = new Dictionary<string, string[]>()
        {
            { Constants.TelegramTypes.SignIn, new string[]{ Constants.TelegramFields.Name } },

            {Constants.TelegramTypes.WatchdogReply, new string[]{ } },
            {Constants.TelegramTypes.WatchdogRequest, new string[]{ } },
            
            {Constants.TelegramTypes.OnlineNotification, new string[]{ Constants.TelegramFields.Name } },
            {Constants.TelegramTypes.OfflineNotification, new string[]{ Constants.TelegramFields.Name } },

            { Constants.TelegramTypes.FileTransferRequest, new string[]{ Constants.TelegramFields.FileName } },
            { Constants.TelegramTypes.ByteArrayTransferRequest, new string[]{ Constants.TelegramFields.FileName } },

            {Constants.TelegramTypes.FileSendAccept, new string[]{ } },
            {Constants.TelegramTypes.ByteSendAccept, new string[]{ } },

            {Constants.TelegramTypes.IncomingFileTransfer, new string[]{ Constants.TelegramFields.FileName, Constants.TelegramFields.FileSize, Constants.TelegramFields.Sender } },

            {Constants.TelegramTypes.ChatMessage , new string[]{ Constants.TelegramFields.Sender, Constants.TelegramFields.Text} },


        };


        public string ParseTelegram(Telegram telegram)
        {
            string message = string.Empty;

            if (telegramDefinitions.ContainsKey(telegram.TelegramType))
            {
                message += telegram.TelegramType;

                string[] fields = telegramDefinitions[telegram.TelegramType];
                for (int i = 0; i < fields.Length; i++)
                {
                    string fieldName = fields[i];
                    string fieldValue = telegram[fieldName];
                    message += $"|{fieldValue}";
                }
            }
            else
            {
                logger.Warn("Telegram unknown");
            }

            return message;
        }


        public Telegram ParseMessage(string message)
        {
            logger.Info($"> {message}");

            string[] messageFields = message.Split('|');

            string telegramType = messageFields[0];

            Telegram telegram = new Telegram();
            if (telegramDefinitions.ContainsKey(telegramType))
            {
                telegram.TelegramType = telegramType;
                string[] fields = telegramDefinitions[telegramType];

                for (int i = 0; i < fields.Length; i++)
                {
                    telegram[fields[i]] = messageFields[i + 1];
                }
            }
            else
            {
                logger.Warn("Telegram unknown");
                telegram.TelegramType = Constants.TelegramTypes.Unknown;
            }
            /*
            if (bytesRead == 0)
            {
                logger.Debug("Host shutdown");
                messageType = MessageType.ServerShutdown;
            }
         */
            return telegram;
        }
    }
}
