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

namespace WpfApplication1.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ChatText : UserControl
    {
        public ChatText(string author, string text, DateTime timestamp)
        {
            InitializeComponent();
            textBlock.Text = text;
            authorLabel.Content = author;
            timeStampLabel.Content = timestamp.ToShortTimeString();
        }

        public ChatText(string author, string text) : this(author, text, DateTime.Now)
        {

        }
    }
}
