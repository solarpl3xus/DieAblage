using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AblageClient
{
    public static class Helper
    {
        public static string GetFileExtension(string filePath)
        {
            string extension = string.Empty;
            if (filePath.Contains('.'))
            {
                int indexOfExtension = filePath.LastIndexOf('.');
                extension = filePath.Substring(indexOfExtension + 1);
            }
            return extension;
        }

        public static string GetFileNameFromPath(string filePath)
        {
            string fileName = string.Empty;
            if (filePath.Contains('\\'))
            {
                int indexOfSlash = filePath.LastIndexOf('\\');
                fileName = filePath.Substring(indexOfSlash + 1);
            }
            return fileName;
        }

    }
}
