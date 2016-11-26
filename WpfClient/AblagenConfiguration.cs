using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace AblageClient
{
    public static class AblagenConfiguration
    {
        public static string ClientName { get; private set; }
        public static int HostDataPort { get; private set; }
        public static string HostIp { get; private set; }
        public static int HostControlPort { get; private set; }
        public static string OutputPath { get; private set; }

        const string nameFileName = "name";
        private static string[] fileExtensionsToOpen;

        internal static void SetupConfiguration()
        {
            HostIp = ConfigurationManager.AppSettings["HostIp"];
            HostControlPort = int.Parse(ConfigurationManager.AppSettings["HostControlPort"]);
            HostDataPort = int.Parse(ConfigurationManager.AppSettings["HostDataPort"]);
            OutputPath = ConfigurationManager.AppSettings["OutputPath"];
            fileExtensionsToOpen = ConfigurationManager.AppSettings["FileExtensionsToOpen"].Split(';').Select(k => k.ToUpper()).ToArray();
            SetupOutputPath();
            TryGetName();
        }

        private static void SetupOutputPath()
        {
            string path = OutputPath;
            OutputPath = AddSlashToPathIfNeeded(OutputPath);
            CreateFolderIfNotExists(OutputPath);
        }



        private static string AddSlashToPathIfNeeded(string outputPath)
        {
            string pathToCheck = outputPath;
            if (pathToCheck[pathToCheck.Length - 1] != '\\')
            {
                pathToCheck += "\\";
            }

            return pathToCheck;
        }

        private static void CreateFolderIfNotExists(string outputPath)
        {
            string[] folderHieararchy = outputPath.Split('\\');
            string path = string.Empty;
            for (int i = 0; i < folderHieararchy.Length - 1; i++)
            {
                path += folderHieararchy[i] + "\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        private static void TryGetName()
        {
            if (File.Exists(nameFileName))
            {
                ClientName = File.ReadAllLines(nameFileName).FirstOrDefault();
            }
        }

        internal static void SaveName(string name)
        {
            ClientName = name;
            File.Create(nameFileName).Close();
            File.WriteAllText(nameFileName, ClientName);
        }

        public static bool IsImage(string filePath)
        {
            string extension = Helper.GetFileExtension(filePath);

            if (fileExtensionsToOpen.Contains(extension.ToUpper()))
            {
                return true;
            }

            return false;
        }


    }
}