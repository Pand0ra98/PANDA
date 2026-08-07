using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("PANDA Uninstaller")]
[assembly: AssemblyDescription("Deinstallationsprogramm für PANDA")]
[assembly: AssemblyProduct("PANDA")]
[assembly: AssemblyCompany("PANDA")]
[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.0")]

namespace PandaUninstall
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            bool silent = args.Length > 0 && string.Equals(args[0], "--silent", StringComparison.OrdinalIgnoreCase);
            string installDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string ownPath = Assembly.GetExecutingAssembly().Location;

            if (!silent)
            {
                DialogResult answer = MessageBox.Show("Soll PANDA vollständig von diesem Benutzerkonto entfernt werden?", "PANDA deinstallieren", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes)
                    return;
            }

            try
            {
                DeleteIfExists(Path.Combine(installDirectory, "PANDA.exe"));
                DeleteIfExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PANDA.lnk"));

                string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "PANDA");
                DeleteIfExists(Path.Combine(startMenuFolder, "PANDA.lnk"));
                DeleteIfExists(Path.Combine(startMenuFolder, "PANDA deinstallieren.lnk"));
                if (Directory.Exists(startMenuFolder) && Directory.GetFileSystemEntries(startMenuFolder).Length == 0)
                    Directory.Delete(startMenuFolder, false);

                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\PANDA", false);

                if (!silent)
                    MessageBox.Show("PANDA wurde erfolgreich deinstalliert.", "Deinstallation abgeschlossen", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ScheduleSelfRemoval(ownPath, installDirectory);
            }
            catch (Exception exception)
            {
                if (!silent)
                    MessageBox.Show("PANDA konnte nicht vollständig entfernt werden.\r\n\r\n" + exception.Message + "\r\n\r\nSchließe PANDA und versuche es erneut.", "Deinstallationsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.ExitCode = 2;
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void ScheduleSelfRemoval(string ownPath, string installDirectory)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), "panda-uninstall-" + Guid.NewGuid().ToString("N") + ".cmd");
            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine("ping 127.0.0.1 -n 2 > nul");
            script.AppendLine("del /f /q \"" + ownPath + "\" > nul 2>&1");
            script.AppendLine("rmdir \"" + installDirectory.TrimEnd(Path.DirectorySeparatorChar) + "\" > nul 2>&1");
            script.AppendLine("del /f /q \"%~f0\" > nul 2>&1");
            File.WriteAllText(scriptPath, script.ToString(), Encoding.ASCII);
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c call \"" + scriptPath + "\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}
