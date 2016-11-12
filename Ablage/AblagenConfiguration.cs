using System;
using System.Configuration;

namespace Ablage
{
    internal static class AblagenConfiguration
    {
        public static string ClientName { get; private set; }
        public static int HostDataPort { get; private set; }
        public static string HostIp { get; private set; }
        public static int HostControlPort { get; private set; }

        internal static void ReadConfiguration()
        {
            HostIp = ConfigurationManager.AppSettings["HostIp"];
            HostControlPort = int.Parse(ConfigurationManager.AppSettings["HostControlPort"]);
            HostDataPort = int.Parse(ConfigurationManager.AppSettings["HostDataPort"]);
            ClientName = ConfigurationManager.AppSettings["Name"];
        }
    }
}