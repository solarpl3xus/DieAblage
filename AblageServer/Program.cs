using AblageServer.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AblageServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args?.Length > 0)
            {
                HandleParameterizedStart(args);
            }
            else
            {
                if (Environment.UserInteractive)
                {
                    GuiStart();
                }
                else
                {
                    ServiceStart();
                }
            }
        }

        private static void HandleParameterizedStart(string[] args)
        {
            if (args[0] == "-install")
            {
                AblagenServiceInstaller.Install(false, args);
            }
            else if (args[0] == "-uninstall")
            {
                AblagenServiceInstaller.Install(true, args);
            }
        }

        private static void GuiStart()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AblageServerController ablageServerController = new AblageServerController();
            Application.Run(new ServerForm(ablageServerController));
        }

        private static void ServiceStart()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                new Service.ServiceController()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
