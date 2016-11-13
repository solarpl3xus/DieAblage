using System;
using System.Configuration;
using System.IO;

namespace Ablage
{
    internal static class AblagenConfiguration
    {
        public static string ClientName { get; private set; }
        public static int HostDataPort { get; private set; }
        public static string HostIp { get; private set; }
        public static int HostControlPort { get; private set; }
        public static string OutputPath { get; private set; }

        internal static void SetupConfiguration()
        {
            HostIp = ConfigurationManager.AppSettings["HostIp"];
            HostControlPort = int.Parse(ConfigurationManager.AppSettings["HostControlPort"]);
            HostDataPort = int.Parse(ConfigurationManager.AppSettings["HostDataPort"]);
            ClientName = ConfigurationManager.AppSettings["Name"];
            OutputPath = ConfigurationManager.AppSettings["OutputPath"];

            SetupOutputPath();
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
        
    }
}