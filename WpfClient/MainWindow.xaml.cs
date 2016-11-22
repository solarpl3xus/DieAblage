using Ablage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.ComponentModel;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, AblageClient.IClientForm
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private AblagenController controller;
        private bool started;

        public MainWindow()
        {
            InitializeComponent();

            controller = new AblagenController(this);

            hostConnectedLabel.Visibility = Visibility.Collapsed;
            new Thread(() =>
            {
                controller.Start();
            }).Start();


            /*/
            ImageSource source = new BitmapImage(new Uri(@"D:\Bilder\be very quiet.jpg"));
            Image image = new Image();
            image.Source = source;
            //image.Stretch = Stretch.Uniform;
            image.StretchDirection = StretchDirection.DownOnly;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            // image.Width = source.Width;
            panel.Children.Add(image);


            source = new BitmapImage(new Uri(@"D:\Bilder\RainbowScienceBARS.png"));
            image = new Image();
            //image.Height = 200;
            image.Source = source;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(image);

            TextBlock text = new TextBlock();
            text.Text = @"https://www.youtube.com/watch?v=UWcBtHdPSKk";
            text.MouseDown += Text_MouseDown;
            panel.Children.Add(text);
            /**/

        }


        public string PromptForName()
        {
            var dialog = new StringInputDialog();
            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(dialog.ResponseText))
                {
                    logger.Fatal("No name entered, closing");
                    Environment.Exit(4919);
                }
                return dialog.ResponseText;
            }
            else
            {
                logger.Fatal("No name entered, closing");
                Environment.Exit(4919);
                return string.Empty;
            }
        }


        protected override void OnContentRendered(EventArgs e)
        {
            started = true;
            base.OnContentRendered(e);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                controller.HandlePaste();
            }
            base.OnKeyDown(e);
        }


        private void OnSendFileButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                controller.SendFileToServer(openFileDialog.FileName);
            }
        }


        public void DisplayIsConnected(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                if (connected)
                {
                    hostConnectedLabel.Visibility = Visibility.Collapsed;
                    clientListBox.Visibility = Visibility.Visible;
                }
                else
                {
                    hostConnectedLabel.Visibility = Visibility.Visible;
                    clientListBox.Visibility = Visibility.Collapsed;
                }
            });
        }


        public void RegisterOnlineClient(string clientName)
        {
            Dispatcher.Invoke(() =>
            {
                clientListBox.Items.Add(clientName);
            });
        }

        public void DeregisterOnlineClient(string clientName)
        {
            Dispatcher.Invoke(() =>
            {
                clientListBox.Items.Remove(clientName);
            });
        }


        public void ReportDownloadProgess(int progress)
        {
            Dispatcher.Invoke(() =>
            {
                downloadBar.Value = progress;
            });
        }

        public void ReportUploadProgess(int progress)
        {
            Dispatcher.Invoke(() =>
            {
                uploadBar.Value = progress;
            });
        }


        public void ShowBalloonMessage(string balloonMessage)
        {
            throw new NotImplementedException();
        }


        private void Text_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(((TextBlock)sender).Text);
        }

        public void HandleFileDownloadCompleted(string completePath)
        {
            if (AblagenConfiguration.IsImage(completePath))
            {
                Dispatcher.Invoke(() =>
                {
                    ImageSource source = new BitmapImage(new Uri(completePath));
                    Image image = new Image();
                    image.Source = source;
                    image.StretchDirection = StretchDirection.DownOnly;
                    image.HorizontalAlignment = HorizontalAlignment.Left;
                    panel.Children.Add(image);
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    TextBlock text = new TextBlock();
                    text.Text = completePath;
                    text.MouseDown += Text_MouseDown;
                    panel.Children.Add(text);
                });
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (controller != null)
            {
                try
                {
                    controller.Shutdown();
                }
                catch (Exception ex)
                {
                    logger.Error("Exception during shutdown,", ex);
                }
            }
            Environment.Exit(4919);
            base.OnClosing(e);
        }
       
    }
}
