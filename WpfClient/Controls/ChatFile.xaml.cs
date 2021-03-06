﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AblageClient.Controls
{
    /// <summary>
    /// Interaction logic for ChatFile.xaml
    /// </summary>
    public partial class ChatFile : UserControl
    {
        public string FilePath { get; private set; }
        private bool downLoadComplete = false;

        public ChatFile(string author, string filePath, DateTime timestamp)
        {
            InitializeComponent();

            FilePath = filePath;

            textBlock.Text = Helper.GetFileNameFromPath(filePath);
            authorLabel.Content = author;
            timeStampLabel.Content = timestamp.ToShortTimeString();

            MouseDown += OnMouseDown;
        }

        public ChatFile(string author, string text) : this(author, text, DateTime.Now)
        {

        }



        public void SetProgress(int progress)
        {
            if (progress < 100)
            {
                progressBar.Visibility = Visibility.Visible;
            }
            else
            {
                progressBar.Visibility = Visibility.Collapsed;
                downLoadComplete = true;
            }
            progressBar.Value = progress;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(FilePath) && downLoadComplete)
            {
                System.Diagnostics.Process.Start(FilePath);
            }
        }

    }
}
