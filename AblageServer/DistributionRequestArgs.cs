using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AblageServer
{
    public class DistributionRequestArgs : EventArgs
    {
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }

        public DistributionRequestArgs(string fileName, byte[] bufferedFile)
        {
            FileName = fileName;
            FileBytes = bufferedFile;
        }

        
    }
}
