using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("PANDA Setup")]
[assembly: AssemblyDescription("Installer für PANDA")]
[assembly: AssemblyProduct("PANDA")]
[assembly: AssemblyCompany("PANDA")]
[assembly: AssemblyVersion("1.9.0.0")]
[assembly: AssemblyFileVersion("1.9.0.0")]

namespace PandaSetup
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 1 && string.Equals(args[0], "--verify", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ResourcePayload.Read("PANDA.Application.exe");
                    ResourcePayload.Read("PANDA.Uninstaller.exe");
                    Environment.ExitCode = 0;
                }
                catch
                {
                    Environment.ExitCode = 2;
                }
                return;
            }

            if (args.Length == 2 && string.Equals(args[0], "--screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var form = new SetupForm())
                {
                    form.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    form.Close();
                }
                return;
            }

            Application.Run(new SetupForm());
        }
    }

    internal static class ResourcePayload
    {
        public static byte[] Read(string name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new InvalidDataException("Installationsdatei fehlt: " + name);
                var bytes = new byte[stream.Length];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    int read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read == 0) break;
                    offset += read;
                }
                if (offset != bytes.Length || bytes.Length < 1024)
                    throw new InvalidDataException("Installationsdatei ist unvollständig: " + name);
                return bytes;
            }
        }
    }

    internal sealed class SetupForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly TextBox installPath = new TextBox();
        private readonly CheckBox desktopShortcut = new CheckBox();
        private readonly CheckBox startMenuShortcut = new CheckBox();
        private readonly CheckBox launchAfterInstall = new CheckBox();
        private readonly Button installButton = new Button();
        private readonly Label statusLabel = new Label();

        public SetupForm()
        {
            Text = "PANDA Setup";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 520);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(30, 24, 30, 22),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            Controls.Add(root);

            var heading = new Panel { Dock = DockStyle.Fill };
            heading.Controls.Add(new Label
            {
                Text = "PANDA installieren",
                Font = new Font("Segoe UI Semibold", 22F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            heading.Controls.Add(new Label
            {
                Text = "Pseudonymisierung alphanumerischer Nutzdaten durch Alphabetverschiebung",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 48)
            });
            heading.Controls.Add(new Label
            {
                Text = "Version 1.9.0  •  Installation für den aktuellen Windows-Benutzer",
                ForeColor = Blue,
                AutoSize = true,
                Location = new Point(2, 73)
            });
            root.Controls.Add(heading, 0, 0);

            var location = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.White,
                Padding = new Padding(18, 14, 18, 12),
                Margin = new Padding(0, 0, 0, 14)
            };
            location.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            location.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            location.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            location.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            location.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            var locationTitle = new Label
            {
                Text = "INSTALLATIONSORDNER",
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            location.Controls.Add(locationTitle, 0, 0);
            location.SetColumnSpan(locationTitle, 2);
            installPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PANDA");
            installPath.Dock = DockStyle.Fill;
            installPath.ForeColor = Navy;
            location.Controls.Add(installPath, 0, 1);
            var browse = new Button
            {
                Text = "Durchsuchen …",
                Anchor = AnchorStyles.None,
                Size = new Size(102, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Navy,
                Margin = new Padding(8, 0, 0, 0)
            };
            browse.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
            browse.Click += delegate { BrowseForFolder(); };
            location.Controls.Add(browse, 1, 1);
            var note = new Label
            {
                Text = "Keine Administratorrechte erforderlich.",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            location.Controls.Add(note, 0, 2);
            location.SetColumnSpan(note, 2);
            root.Controls.Add(location, 0, 1);

            var options = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18, 14, 18, 12) };
            options.Controls.Add(new Label
            {
                Text = "OPTIONEN",
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(18, 16)
            });
            desktopShortcut.Text = "Desktop-Verknüpfung erstellen";
            desktopShortcut.Checked = true;
            desktopShortcut.AutoSize = true;
            desktopShortcut.ForeColor = Navy;
            desktopShortcut.Location = new Point(20, 52);
            startMenuShortcut.Text = "Eintrag im Startmenü erstellen";
            startMenuShortcut.Checked = true;
            startMenuShortcut.AutoSize = true;
            startMenuShortcut.ForeColor = Navy;
            startMenuShortcut.Location = new Point(20, 82);
            launchAfterInstall.Text = "PANDA nach der Installation starten";
            launchAfterInstall.Checked = true;
            launchAfterInstall.AutoSize = true;
            launchAfterInstall.ForeColor = Navy;
            launchAfterInstall.Location = new Point(20, 112);
            options.Controls.Add(desktopShortcut);
            options.Controls.Add(startMenuShortcut);
            options.Controls.Add(launchAfterInstall);
            root.Controls.Add(options, 0, 2);

            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 18, 0, 0) };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            statusLabel.Text = "Bereit zur Installation.";
            statusLabel.ForeColor = Muted;
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            footer.Controls.Add(statusLabel, 0, 0);
            installButton.Text = "Jetzt installieren";
            installButton.Dock = DockStyle.Fill;
            installButton.Margin = new Padding(0);
            installButton.FlatStyle = FlatStyle.Flat;
            installButton.FlatAppearance.BorderSize = 0;
            installButton.BackColor = Blue;
            installButton.ForeColor = Color.White;
            installButton.Cursor = Cursors.Hand;
            installButton.Click += delegate { Install(); };
            footer.Controls.Add(installButton, 1, 0);
            root.Controls.Add(footer, 0, 3);
            AcceptButton = installButton;
        }

        private void BrowseForFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Installationsordner für PANDA auswählen";
                dialog.SelectedPath = installPath.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    installPath.Text = Path.Combine(dialog.SelectedPath, "PANDA");
            }
        }

        private void Install()
        {
            string target;
            try
            {
                target = Path.GetFullPath(installPath.Text.Trim());
                if (string.Equals(target, Path.GetPathRoot(target), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Bitte wähle keinen Laufwerks-Stammordner aus.");

                installButton.Enabled = false;
                statusLabel.Text = "PANDA wird installiert …";
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                Directory.CreateDirectory(target);
                string applicationPath = Path.Combine(target, "PANDA.exe");
                string uninstallerPath = Path.Combine(target, "PANDA-Uninstall.exe");
                File.WriteAllBytes(applicationPath, ResourcePayload.Read("PANDA.Application.exe"));
                File.WriteAllBytes(uninstallerPath, ResourcePayload.Read("PANDA.Uninstaller.exe"));

                string desktopLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PANDA.lnk");
                if (desktopShortcut.Checked)
                    Shortcut.Create(desktopLink, applicationPath, target, "PANDA starten");
                else if (File.Exists(desktopLink))
                    File.Delete(desktopLink);

                string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "PANDA");
                if (startMenuShortcut.Checked)
                {
                    Directory.CreateDirectory(startMenuFolder);
                    Shortcut.Create(Path.Combine(startMenuFolder, "PANDA.lnk"), applicationPath, target, "PANDA starten");
                    Shortcut.Create(Path.Combine(startMenuFolder, "PANDA deinstallieren.lnk"), uninstallerPath, target, "PANDA deinstallieren");
                }
                else if (Directory.Exists(startMenuFolder))
                {
                    string oldApplicationLink = Path.Combine(startMenuFolder, "PANDA.lnk");
                    string oldUninstallLink = Path.Combine(startMenuFolder, "PANDA deinstallieren.lnk");
                    if (File.Exists(oldApplicationLink)) File.Delete(oldApplicationLink);
                    if (File.Exists(oldUninstallLink)) File.Delete(oldUninstallLink);
                    if (Directory.GetFileSystemEntries(startMenuFolder).Length == 0) Directory.Delete(startMenuFolder, false);
                }

                RegisterUninstaller(target, applicationPath, uninstallerPath);
                statusLabel.Text = "Installation erfolgreich abgeschlossen.";
                Cursor = Cursors.Default;
                MessageBox.Show(this, "PANDA wurde erfolgreich installiert.\r\n\r\nDie Deinstallation ist über Windows › Installierte Apps ‹ oder das Startmenü möglich.", "PANDA installiert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (launchAfterInstall.Checked)
                    Process.Start(applicationPath);
                Close();
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                installButton.Enabled = true;
                statusLabel.Text = "Installation fehlgeschlagen.";
                MessageBox.Show(this, "PANDA konnte nicht installiert werden.\r\n\r\n" + exception.Message + "\r\n\r\nFalls PANDA bereits läuft, schließe das Programm und versuche es erneut.", "Installationsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void RegisterUninstaller(string target, string applicationPath, string uninstallerPath)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\PANDA"))
            {
                key.SetValue("DisplayName", "PANDA");
                key.SetValue("DisplayVersion", "1.9.0");
                key.SetValue("DisplayIcon", applicationPath);
                key.SetValue("Publisher", "PANDA");
                key.SetValue("InstallLocation", target);
                key.SetValue("UninstallString", "\"" + uninstallerPath + "\"");
                key.SetValue("QuietUninstallString", "\"" + uninstallerPath + "\" --silent");
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                key.SetValue("EstimatedSize", 100, RegistryValueKind.DWord);
            }
        }
    }

    internal static class Shortcut
    {
        public static void Create(string shortcutPath, string targetPath, string workingDirectory, string description)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
                throw new InvalidOperationException("Windows-Verknüpfungen werden auf diesem System nicht unterstützt.");
            object shell = Activator.CreateInstance(shellType);
            object shortcut = null;
            try
            {
                shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDirectory });
                shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { description });
                shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath + ",0" });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            finally
            {
                if (shortcut != null && Marshal.IsComObject(shortcut)) Marshal.FinalReleaseComObject(shortcut);
                if (shell != null && Marshal.IsComObject(shell)) Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
