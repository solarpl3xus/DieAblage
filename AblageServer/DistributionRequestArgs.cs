using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AblageServer
{
    public class DistributionRequestArgs : EventArgs
    {
        public string Sender { get; set; }
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }

        public DistributionRequestArgs(string sender,string fileName, byte[] bufferedFile)
        {
            Sender = sender;
            FileName = fileName;
            FileBytes = bufferedFile;
        }

        
    }
}
