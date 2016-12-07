using AblageServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AblageServer.Service
{
    public partial class ServiceController : ServiceBase
    {

        private AblageServerController ablageServerController;
        public ServiceController()
        {
            InitializeComponent();
            CanStop = true;
            ablageServerController = new AblageServerController();
        }

        protected override void OnStart(string[] args)
        {
            ablageServerController.Start();   
        }

        protected override void OnStop()
        {
            ablageServerController.ShutdownConnections();
        }

        protected override void OnShutdown()
        {
            ablageServerController.ShutdownConnections();
            base.OnShutdown();
        }
    }
}
