using System;
using System.Collections.Generic;
using System.Drawing;
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
        void DisplayChatMessage(string sender, string chatMessage);
        void AddImageToChatStream(Image image);
    }
}
