using System;
using System.Runtime.Versioning;
using Microsoft.Win32.TaskScheduler;

namespace Barbar.HostsSwitcher.Utilities
{
    [SupportedOSPlatform("windows")]
    public static class AutostartManager
    {
        private const string TaskName = "HostsSwitcher_Autostart";

        /// <summary>
        /// Checks if autostart is currently enabled by querying the Task Scheduler.
        /// </summary>
        /// <returns>True if the scheduled task exists, false otherwise.</returns>
        public static bool IsEnabled()
        {
            try
            {
                return TaskService.Instance.GetTask(TaskName) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enables autostart by creating a scheduled task in Task Scheduler.
        /// </summary>
        /// <param name="exePath">Full path to the application executable.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool Enable(string exePath)
        {
            try
            {
                using var ts = TaskService.Instance;
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Starts Hosts Switcher at logon";
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;

                td.Triggers.Add(new LogonTrigger());
                td.Actions.Add(new ExecAction(exePath, "/minimized"));

                ts.RootFolder.RegisterTaskDefinition(TaskName, td, TaskCreation.CreateOrUpdate,
                    null, null, TaskLogonType.InteractiveToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disables autostart by deleting the scheduled task from Task Scheduler.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool Disable()
        {
            try
            {
                TaskService.Instance.RootFolder.DeleteTask(TaskName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
