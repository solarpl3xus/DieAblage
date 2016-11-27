using System;
using System.Collections.Generic;
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
    /// Interaction logic for ChatImage.xaml
    /// </summary>
    public partial class ChatImage : UserControl
    {
        public ImageSource Source
        {
            get { return image.Source; }
            set
            {
                image.Source = value;
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        public string ImageName { get; private set; }

        public ChatImage(string author, string imageName, DateTime timestamp)
        {
            InitializeComponent();

            ImageName = imageName;
            
            authorLabel.Content = author;
            timeStampLabel.Content = timestamp.ToShortTimeString();
            
        }

        public ChatImage(string author, string text) : this(author, text, DateTime.Now)
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
            }
            progressBar.Value = progress;
        }
    }
}
