using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AblageClient
{
    public interface IClientForm
    {
        string PromptForName();

        void ShowBalloonMessage(string balloonMessage);

        void RegisterOnlineClient(string clientName);

        void DeregisterOnlineClient(string clientName);

        void DisplayIsConnected(bool connected);

        void ReportUploadProgess(int progress);

        void ReportDownloadProgess(int progress);
        void HandleFileDownloadCompleted(string completePath);
    }
}
