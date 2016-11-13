using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Ablage
{
    partial class MainForm : Form
    {
        private AblagenController controller;

        private List<string> onlineClients;

        public MainForm(AblagenController ablagenController)
        {
            onlineClients = new List<string>();

            InitializeComponent();
            controller = ablagenController;

            controller.Form = this;            
        }

        private void OnSendFileButtonClick(object sender, EventArgs e)
        {
            //ShowBalloonMessage("BANG");

            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                controller.SendFileToServer(openFileDialog.FileName);
            }
        }

        internal void ShowBalloonMessage(string balloonMessage)
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;


            notifyIcon.BalloonTipTitle = "Die Ablage";
            notifyIcon.BalloonTipText = balloonMessage;

            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.ShowBalloonTip(30000);
        }
    }
}
