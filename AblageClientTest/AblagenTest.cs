using System;
using NUnit.Framework;
using Ablage;
using System.IO;

namespace AblageClientTest
{
    [TestFixture]
    public class AblagenTest
    {
        const string file1 = @"C:\Projects\DieAblage\AblageClientTest\AblagenTest.cs";
        const string file2 = @"C:\Projects\DieAblage\AblageClientTest\packages.config";

        [SetUp]
        public void Setup()
        {
            Directory.SetCurrentDirectory(@"C:\Projects\DieAblage\AblageClientTest\\bin\\Debug");
            CleanOutputFolder();
        }

        private void CleanOutputFolder()
        {
            DeleteIfExists(file1);
            DeleteIfExists(file2);
        }

        private void DeleteIfExists(string file)
        {
            string fileName = file.Substring(file.LastIndexOf('\\') + 1);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        [Test]
        public void FileTransfer()
        {
            AblagenController ablagenController = new AblagenController();
            ablagenController.SendFileToServer(file1);

            System.Threading.Thread.Sleep(2000);

            string fileName = file1.Substring(file1.LastIndexOf('\\') + 1);
            Assert.True(File.Exists(fileName), $"{fileName} not copied");
            

            ablagenController.SendFileToServer(file2);
            System.Threading.Thread.Sleep(2000);
            fileName = file2.Substring(file2.LastIndexOf('\\') + 1);
            Assert.True(File.Exists(fileName), $"{fileName} not copied");
            


            ablagenController.Disconnect();
        }
    }
}