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
using System.Threading;

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
            onlineClientsBox.DataSource = onlineClients;
            
            controller = ablagenController;
            controller.Form = this;
            controller.Start();
        }

        private void OnSendFileButtonClick(object sender, EventArgs e)
        {            
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

        internal void RegisterOnlineClient(string clientName)
        {
            List<string> nList = new List<string>();

             Invoke((MethodInvoker)(() =>
             {
                 nList.AddRange(onlineClients);
                 nList.Add(clientName);

                 onlineClientsBox.DataSource = nList;
                 onlineClients = nList;

             }));
        }

        internal void DeregisterOnlineClient(string clientName)
        {
            List<string> nList = new List<string>();

            Invoke((MethodInvoker)(() =>
            {
                nList.AddRange(onlineClients);
                nList.Remove(clientName);

                onlineClientsBox.DataSource = nList;
                onlineClients = nList;

            }));
        }
    }
}
