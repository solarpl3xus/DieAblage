using AblageClient;
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
using AblageClient.Controls;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using CommunicationBase;

namespace AblageClient
{
    public partial class ClientForm : Window
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private AblagenController controller;
        private bool started = false;
        private UIElement previousLastElement;
        private GlobalKeyListener globalKeyListener;
        private SoundPlayer simpleSound;



        public ClientForm()
        {
            InitializeComponent();

            controller = new AblagenController(this);
            globalKeyListener = new GlobalKeyListener(this);

            hostConnectedLabel.Visibility = Visibility.Collapsed;
            new Thread(() =>
            {
                controller.Start();
            }).Start();

            chatViewer.ScrollChanged += OnChatViewerScrollChanged;

            simpleSound = new SoundPlayer(@"c:\Windows\Media\chimes.wav");

            /*/
            ImageSource source = new BitmapImage(new Uri(@"D:\Bilder\be very quiet.jpg"));
            Image image = new Image();
            image.Source = source;
            //image.Stretch = Stretch.Uniform;
            image.StretchDirection = StretchDirection.DownOnly;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            // image.Width = source.Width;


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

            ChatText ct = new ChatText("Yung Lean", "Motorola");
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(ct);
            
            
            ChatFile ct = new Controls.ChatFile("Yung Lean", "Motorola", DateTime.Now);
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(ct);


            ChatText ct = new Controls.ChatText("Yung Lean", @"Motorola http://google.com", DateTime.Now);
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(ct);

            ct = new Controls.ChatText("Yung Lean", @"www.google.de", DateTime.Now);
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(ct);

            ct = new Controls.ChatText("Yung Lean", @"www.google.de otto", DateTime.Now);
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(ct);

            ct = new Controls.ChatText("Yung Lean", @"otto www.google.de otto", DateTime.Now);
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(ct);

            ChatImage ct = new ChatImage("you", "img");
            ct.HorizontalAlignment = HorizontalAlignment.Left;
            ct.Source = new BitmapImage(new Uri(@"D:\Bilder\RainbowScienceBARS.png"));
            panel.Children.Add(ct);
            
             /**/
        }


        private void OnChatViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UIElement lastElement = null;

            foreach (UIElement element in panel.Children)
            {
                lastElement = element;
            }

            if (lastElement != null && previousLastElement != lastElement)
            {
                double theHeight = 0;

                Type elementType = lastElement.GetType();

                if (elementType == typeof(Image))
                {
                    Image theImage = (Image)lastElement;

                    theHeight = theImage.ActualHeight;
                }
                else if (elementType == typeof(UserControl))
                {
                    theHeight = ((UserControl)lastElement).Height;
                }

                chatViewer.ScrollToVerticalOffset(chatViewer.ScrollableHeight);
            }

            previousLastElement = lastElement;
        }

        internal void Paste(bool ignoreActive = false)
        {
            if (IsActive || ignoreActive)
            {
                controller.HandlePaste();
            }
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


        private void OnDropFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                controller.SendFilesToServer(files);
            }
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


        public void ReportDownloadProgess(int progress, string completePath)
        {
            Dispatcher.Invoke(() =>
            {
                List<UIElement> elementList = new List<UIElement>();
                foreach (UIElement element in panel.Children)
                {
                    elementList.Add(element);
                }
                UIElement downloadingElement = elementList.Where(c => c.GetType() == typeof(ChatFile) && ((ChatFile)c).FilePath == completePath || c.GetType() == typeof(ChatImage) && ((ChatImage)c).ImageName == completePath).LastOrDefault();
                if (downloadingElement != null)
                {
                    Type elementType = downloadingElement.GetType();

                    if (elementType == typeof(ChatFile))
                    {
                        ((ChatFile)downloadingElement).SetProgress(progress);
                    }
                    else if(elementType == typeof(ChatImage))
                    {
                        ((ChatImage)downloadingElement).SetProgress(progress);
                    }
                }

                if (progress >= 100)
                {
                    simpleSound.Play();
                    Flasher.FlashWindow(this);
                }
            });
        }

        public void ReportUploadProgess(int progress, string fileName)
        {
            Dispatcher.Invoke(() =>
            {
                List<UIElement> elementList = new List<UIElement>();
                foreach (UIElement element in panel.Children)
                {
                    elementList.Add(element);
                }
                ChatFile pendingChatFile = elementList.Where(c => c.GetType() == typeof(ChatFile) && ((ChatFile)c).FilePath == fileName).Select(c => (ChatFile)c).LastOrDefault();

                pendingChatFile?.SetProgress(progress);
            });
        }


        public void ShowBalloonMessage(string balloonMessage)
        {
            throw new NotImplementedException();
        }


        public void HandleFileDownloadStarted(string sender, string completePath)
        {
            if (AblagenConfiguration.IsImage(completePath))
            {
                Dispatcher.Invoke(() =>
                {
                    ChatImage ct = new ChatImage(sender, completePath); ;
                    ct.HorizontalAlignment = HorizontalAlignment.Left;
                    panel.Children.Add(ct);                    
                });
            }
            else
            {
                AddFileToChatStream(sender, completePath);
            }

        }

        public void HandleFileDownloadCompleted(string sender, string completePath)
        {
            if (AblagenConfiguration.IsImage(completePath))
            {
                Dispatcher.Invoke(() =>
                {
                    List<UIElement> elementList = new List<UIElement>();
                    foreach (UIElement element in panel.Children)
                    {
                        elementList.Add(element);
                    }
                    ChatImage chatImage = elementList.Where(c => c.GetType() == typeof(ChatImage) && ((ChatImage)c).ImageName == completePath).Select(c => (ChatImage)c).LastOrDefault();

                    if (chatImage != null)
                    {
                        chatImage.Source = new BitmapImage(new Uri(completePath)); 
                    }
                    else
                    {
                        logger.Warn($"Chat image for path {completePath} could not be found");
                    }
                    
                });
            }            
        }


        private void OnSendTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(sendTextBox.Text))
                {
                    Telegram telegram = new Telegram(Constants.TelegramTypes.ChatMessage);
                    telegram[Constants.TelegramFields.Text] = sendTextBox.Text;
                    controller.SendTelegram(telegram);

                    AddChatMessageToChatStream(AblagenConfiguration.ClientName, sendTextBox.Text);

                    sendTextBox.Text = string.Empty;
                }
            }
            else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                controller.HandlePaste();
            }
        }


        public void AddFileToChatStream(string sender, string completePath)
        {
            Dispatcher.Invoke(() =>
            {
                ChatFile chatFile = new ChatFile(sender, completePath);
                chatFile.HorizontalAlignment = HorizontalAlignment.Left;

                AddUiElementToStream(sender, chatFile);
            });
        }

        public void AddChatMessageToChatStream(string sender, string chatMessage)
        {
            Dispatcher.Invoke(() =>
            {
                ChatText chatText = new ChatText(sender, chatMessage);
                chatText.HorizontalAlignment = HorizontalAlignment.Left;
                AddUiElementToStream(sender, chatText);
            });
        }

        public void AddImageToChatStream(string sender, Image image)
        {
            Dispatcher.Invoke(() =>
            {
                image.StretchDirection = StretchDirection.DownOnly;
                image.HorizontalAlignment = HorizontalAlignment.Left;
                if (sender == AblagenConfiguration.ClientName)
                {
                    image.Margin = new Thickness(40, 10, 0, 0);
                }
                else
                {
                    simpleSound.Play();
                    Flasher.FlashWindow(this);
                }
                panel.Children.Add(image);
            });
        }


        private void AddUiElementToStream(string sender, System.Windows.Controls.Control image)
        {
            if (sender == AblagenConfiguration.ClientName)
            {
                image.Margin = new Thickness(40, 0, 0, 0);
            }
            else
            {
                simpleSound.Play();
                Flasher.FlashWindow(this);
            }
            panel.Children.Add(image);
        }

        protected override void OnActivated(EventArgs e)
        {
            Flasher.StopFlashingWindow(this);
            base.OnActivated(e);
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
