﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ablage
{
    partial class MainForm : Form
    {
        private AblagenController controller;

        public MainForm(AblagenController ablagenController)
        {
            InitializeComponent();
            controller = ablagenController;

            
        }

        private void OnSendFileButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {                
                controller.SendFileToServer(openFileDialog.FileName);
            }
        }
    }
}
