using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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


        public static Image ConvertDrawingImageToControlsImage(System.Drawing.Image drawingImage)
        {
            Image controlsImage = new Image();
            BitmapImage bitmapImage = ConvertDrawingImageToBitmapImage(drawingImage);
            controlsImage.Source = bitmapImage;
            return controlsImage;
        }

        private static BitmapImage ConvertDrawingImageToBitmapImage(System.Drawing.Image img)
        {
            MemoryStream memoryStream = new MemoryStream();
            img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            return bitmapImage;

        }
    }
}
