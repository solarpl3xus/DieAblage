using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AblageServer
{
    public class ChatMessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public ChatMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
