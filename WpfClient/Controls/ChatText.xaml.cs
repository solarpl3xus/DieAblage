using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ChatText : UserControl
    {
        private readonly Regex RE_URL = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)", RegexOptions.Multiline);

        public ChatText()
        {
            InitializeComponent();
        }



        public ChatText(string author, string text, DateTime timestamp) : this()
        {
            Paragraph para = new Paragraph();
            textBlock.Document = new FlowDocument();
            textBlock.Document.Blocks.Add(para);

            if (RE_URL.IsMatch(text))
            {
                List<Match> hyperLinkMatches = new List<Match>();
                foreach (Match match in (RE_URL.Matches(text)))
                {
                    hyperLinkMatches.Add(match);
                }


                int last_pos = 0;
                foreach (Match match in hyperLinkMatches)
                {
                    if (match.Index != last_pos)
                    {
                        var raw_text = text.Substring(last_pos, match.Index - last_pos);
                        para.Inlines.Add(new Run(raw_text));
                    }

                    string hyperLink = match.Value.StartsWith("www.", StringComparison.CurrentCultureIgnoreCase) ? $"http://{match.Value}" : match.Value;

                    var link = new Hyperlink(new Run(match.Value))
                    {
                        NavigateUri = new Uri(hyperLink)
                    };
                    link.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());

                    para.Inlines.Add(link);


                    last_pos = match.Index + match.Length;
                    if (match.Index == hyperLinkMatches[hyperLinkMatches.Count - 1].Index)
                    {
                        var raw_text = text.Substring(last_pos);
                        para.Inlines.Add(new Run(raw_text));
                    }
                }

            }
            else
            {
                para.Inlines.Add(new Run(text));
            }
            authorLabel.Content = author;
            timeStampLabel.Content = timestamp.ToShortTimeString();
        }

        public ChatText(string author, string text) : this(author, text, DateTime.Now)
        {

        }
    }
}
