using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AblageServer
{
    public partial class ServerForm : Form
    {
        private AblageServerController ablageServerController;

        public ServerForm(AblageServerController ablageServerController)
        {
            InitializeComponent();
            this.ablageServerController = ablageServerController;
            ablageServerController.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ablageServerController.ShutdownConnections();
            base.OnClosing(e);
        }
    }
}
