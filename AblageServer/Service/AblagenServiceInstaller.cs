﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AblageServer.Service
{
    [RunInstaller(true)]
    public sealed class ServiceInstallerProcess : ServiceProcessInstaller
    {
        public ServiceInstallerProcess()
        {
            this.Account = ServiceAccount.LocalService;
        }
    }

    [RunInstaller(true)]
    public sealed class AblagenServiceInstaller : ServiceInstaller
    {
        public AblagenServiceInstaller()
        {
            Description = "Server for Die Ablage";
            DisplayName = "Ablagen Server";
            ServiceName = "Ablagen Server";
            StartType = ServiceStartMode.Manual;
        }


        public static void Install(bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing");
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                            MessageBox.Show("Service uninstalled");
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                            MessageBox.Show("Service installed");
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during service install {ex.Message}");
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
