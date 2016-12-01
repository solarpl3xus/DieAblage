using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationBase
{
    public class Telegram
    {
        private Dictionary<string, string> fields = new Dictionary<string, string>();
        private string byteArrayTransferRequest;

        public string TelegramType { get; internal set; }

        public Telegram()
        {

        }

        public Telegram(string telegramType)
        {
            TelegramType = telegramType;
        }

        public string this[string fieldName, bool defaultEmtpy = true]
        {
            get
            {
                string returnValue = string.Empty;

                if (defaultEmtpy)
                {
                    if (fields.ContainsKey(fieldName))
                    {
                        returnValue = fields[fieldName];
                    }
                    return returnValue;
                }
                else
                {
                    return fields[fieldName];
                }
            }
            set
            {
                fields[fieldName] = value;
            }
        }

    }
}
