using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Circle_Tracker
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!EnsureSingleInstance())
            {
                MessageBox.Show("Another instance of circle tracker is already running.", "Error");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        static bool EnsureSingleInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();

            var runningProcess = (from process in Process.GetProcesses()
                                  where
                                    process.Id != currentProcess.Id &&
                                    process.ProcessName.Equals(
                                      currentProcess.ProcessName,
                                      StringComparison.Ordinal)
                                  select process).FirstOrDefault();

            return (runningProcess != null) ? false : true;
        }
    }
}
