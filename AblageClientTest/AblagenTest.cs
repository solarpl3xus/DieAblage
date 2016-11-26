using System;
using NUnit.Framework;
using AblageClient;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

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



            ablagenController.Shutdown();
        }

        

        private void CreateFileWithSequence()
        {
            List<string> countArray = new List<string>();

            int length = 1000000;
            for (int i = 0; i < length; i++)
            {
                countArray.Add(i.ToString().PadLeft(8, '0'));
            }

            File.WriteAllLines(AblagenConfiguration.OutputPath + length.ToString(), countArray.ToArray());
        }

        [Test]
        public void AlternateFileName()
        {
            AblagenController ablagenController = new AblagenController();

            List<string> testFileNames = new List<string>()
            {
                "noextension",

                "extenstion.dat",

                "noextensiondup",
                "noextensiondup(1)",

                "extensiondup.dat",
            };

            for (int i = 1; i < 5; i++)
            {
                testFileNames.Add($"extensiondup({i}).dat");
            }

            for (int i = 0; i < testFileNames.Count; i++)
            {
                File.Create($"{AblagenConfiguration.OutputPath}{testFileNames[i]}").Close();
            }

            MethodInfo adjustFilePathIfAlreadyExists = ablagenController.GetType().GetMethod("AdjustFilePathIfAlreadyExists", BindingFlags.NonPublic | BindingFlags.Instance);

            string result = (string)adjustFilePathIfAlreadyExists.Invoke(ablagenController, new object[] { "newfile" });
            Assert.AreEqual($"{AblagenConfiguration.OutputPath}{"newfile"}", result);

            
            result = (string)adjustFilePathIfAlreadyExists.Invoke(ablagenController, new object[] { "noextension" });
            Assert.AreEqual($"{AblagenConfiguration.OutputPath}{"noextension(1)"}", result);

            result = (string)adjustFilePathIfAlreadyExists.Invoke(ablagenController, new object[] { "extenstion.dat" });
            Assert.AreEqual($"{AblagenConfiguration.OutputPath}{"extenstion(1).dat"}", result);

            result = (string)adjustFilePathIfAlreadyExists.Invoke(ablagenController, new object[] { "noextensiondup" });
            Assert.AreEqual($"{AblagenConfiguration.OutputPath}{"noextensiondup(2)"}", result);

            result = (string)adjustFilePathIfAlreadyExists.Invoke(ablagenController, new object[] { "extensiondup.dat" });
            Assert.AreEqual($"{AblagenConfiguration.OutputPath}{"extensiondup(5).dat"}", result);

            for (int i = 0; i < testFileNames.Count; i++)
            {
                File.Delete($"{AblagenConfiguration.OutputPath}{testFileNames[i]}");
            }

            ablagenController.Shutdown();            
            //AdjustFilePathIfAlreadyExists
        }


    }
}