using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ablage
{
    static class Program
    {
        // log received files
        // register at dyndns
        // encryption

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AblagenController ablagenController = new AblagenController();
            Application.Run(new MainForm(ablagenController));
        }
    }
}
