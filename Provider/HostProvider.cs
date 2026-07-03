using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Barbar.HostsSwitcher.Provider
{
    internal class HostProvider : IHostProvider
    {
        private readonly List<string> _excludeList = new List<string>()
        {
            "lmhosts.sam",
            "networks",
            "protocol",
            "services"
        };

        private readonly string _directory = Path.Combine(Path.Combine(Environment.SystemDirectory, "Drivers"), "etc");

        public string[] GetHostFiles()
        {
            List<string> result = new List<string>();
            foreach (var file in Directory.GetFiles(_directory))
            {
                var fileName = Path.GetFileName(file);
                if (_excludeList.Contains(fileName.ToLowerInvariant()))
                    continue;

                result.Add(fileName);
            }

            return result.ToArray();
        }

        public void ReplaceHosts(string fileName)
        {
            if (string.Compare(fileName, "hosts", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return;
            }

            File.Copy(Path.Combine(_directory, fileName), Path.Combine(_directory, "hosts"), true);
        }

        public void CopyHosts(string sourceFileName, string targetFileName)
        {
            File.Copy(Path.Combine(_directory, sourceFileName), Path.Combine(_directory, targetFileName), true);
        }

        public void DeleteHosts(string fileName)
        {
            File.Delete(Path.Combine(_directory, fileName));
        }

        public void LaunchEditor(string fileName)
        {
            Process.Start("notepad", @"""" + Path.Combine(_directory, fileName) + @"""");
        }

        public void OpenFolder()
        {
            Process.Start("explorer", @"""" + _directory + @"""");
        }

        public string GetHostsDirectory()
        {
            return _directory;
        }
    }
}