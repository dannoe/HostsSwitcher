using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows.Forms;

namespace Barbar.HostsSwitcher
{
    static class Program
    {
        [SupportedOSPlatform("windows")]
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if the app should start minimized (e.g., launched from autostart)
            bool startMinimized = false;
            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (string.Equals(arg, "/minimized", StringComparison.OrdinalIgnoreCase))
                    {
                        startMinimized = true;
                        break;
                    }
                }
            }

            bool createdNew = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(true, "HostsSwitcher", out createdNew);
            }
            catch
            {
            }

            if (mutex == null || !createdNew)
            {
                MessageBox.Show("Another instance of HostsSwitcher is already running.", "Cannot start HostsSwitcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.Run(new FormMain(startMinimized));
            }
            finally
            {
                mutex.Close();
            }
        }
    }
}