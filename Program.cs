using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("PANDA")]
[assembly: AssemblyDescription("Pseudonymisierung alphanumerischer Nutzdaten durch Alphabetverschiebung")]
[assembly: AssemblyProduct("PANDA")]
[assembly: AssemblyCompany("PANDA")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]

namespace Panda
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 2 && string.Equals(args[0], "--screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var form = new MainForm(false, "Metro"))
                {
                    form.Size = new Size(1320, 820);
                    form.Show();
                    form.LoadPreviewData();
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
            if (args.Length == 2 && string.Equals(args[0], "--metro-empty-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var form = new MainForm(false, "Metro"))
                {
                    form.Size = new Size(1320, 820);
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
            if (args.Length == 2 && string.Equals(args[0], "--simple-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var form = new MainForm(false, "Metro"))
                {
                    form.Size = new Size(1320, 820);
                    form.Show();
                    form.LoadPreviewData();
                    form.SetAdvancedShiftModeForTest(false);
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
            if (args.Length == 2 && string.Equals(args[0], "--classic-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var form = new MainForm(false, "Classic"))
                {
                    form.Size = new Size(1320, 820);
                    form.Show();
                    form.LoadPreviewData();
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
            if (args.Length == 3 && string.Equals(args[0], "--wizard-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var wizard = new ImportWizardForm(args[1]))
                {
                    wizard.Show();
                    Application.DoEvents();
                    wizard.UncheckLastColumnForPreview();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(wizard.Width, wizard.Height))
                    {
                        wizard.DrawToBitmap(bitmap, new Rectangle(Point.Empty, wizard.Size));
                        bitmap.Save(args[2], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    wizard.Close();
                }
                return;
            }
            if (args.Length == 2 && string.Equals(args[0], "--settings-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                var previewSettings = new AppSettings { DefaultShiftSequence = "3-5-8-2", ConfirmBeforeShift = true, AskForUpdateCheckOnStart = true, InterfaceStyle = "Metro" };
                using (var settingsForm = new SettingsForm(previewSettings))
                {
                    settingsForm.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(settingsForm.Width, settingsForm.Height))
                    {
                        settingsForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, settingsForm.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    settingsForm.Close();
                }
                return;
            }
            if (args.Length == 2 && string.Equals(args[0], "--quick-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var quickForm = new QuickConversionForm("3-5-8-2"))
                {
                    quickForm.LoadPreviewData();
                    quickForm.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(quickForm.Width, quickForm.Height))
                    {
                        quickForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, quickForm.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    quickForm.Close();
                }
                return;
            }
            if (args.Length == 2 && string.Equals(args[0], "--templates-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                var previewTemplates = new List<SelectionTemplate>
                {
                    new SelectionTemplate("Kontaktdaten", new[] { "Vorname", "Büro" })
                };
                using (var templatesForm = new SelectionTemplatesForm(
                    new[] { "Kundennummer", "Vorname", "Nachname", "Büro", "Ort" },
                    previewTemplates,
                    new[] { 1, 3 },
                    false))
                {
                    templatesForm.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(templatesForm.Width, templatesForm.Height))
                    {
                        templatesForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, templatesForm.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    templatesForm.Close();
                }
                return;
            }
            if (args.Length == 2 && string.Equals(args[0], "--export-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                var exportRows = new List<IList<string>>();
                for (int row = 1; row <= 20; row++)
                    exportRows.Add(new List<string> { (1000 + row).ToString(), "Vorname " + row, "Büro " + ((row % 4) + 1) });
                using (var exportForm = new ExportOptionsForm(
                    new[] { "Kundennummer", "Vorname", "Büro" },
                    exportRows,
                    new[] { 4, 11 },
                    new[] { 4, 11 }))
                {
                    exportForm.SelectCustomModeForPreview();
                    exportForm.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(exportForm.Width, exportForm.Height))
                    {
                        exportForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, exportForm.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    exportForm.Close();
                }
                return;
            }
            if (args.Length == 2 && string.Equals(args[0], "--filter-screenshot", StringComparison.OrdinalIgnoreCase))
            {
                var filterRows = new List<IList<string>>
                {
                    new List<string> { "1001", "Anna", "Meyer", "Berlin" },
                    new List<string> { "1002", "Jonas", "Schmidt", "Hamburg" },
                    new List<string> { "1003", "Zoe", "Fischer", "München" },
                    new List<string> { "1004", "Lena", "Wagner", "Köln" },
                    new List<string> { "1005", "Mia", "Koch", "Mainz" }
                };
                var previewFilter = new RowFilter(3, new[] { "Hamburg" }, "M*");
                using (var filterForm = new RowFilterForm(
                    new[] { "Kundennummer", "Vorname", "Nachname", "Ort" },
                    filterRows,
                    previewFilter))
                {
                    filterForm.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(filterForm.Width, filterForm.Height))
                    {
                        filterForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, filterForm.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                    filterForm.Close();
                }
                return;
            }
            Application.Run(new MainForm());
        }
    }

    internal sealed class CsvDocument
    {
        public List<string> Headers = new List<string>();
        public List<List<string>> Rows = new List<List<string>>();
        public char Delimiter;
        public bool FirstRowIsHeader;
    }

    internal static class CsvCodec
    {
        public static CsvDocument Load(string path, bool firstRowIsHeader)
        {
            string text;
            using (var reader = new StreamReader(path, Encoding.UTF8, true))
                text = reader.ReadToEnd();

            char delimiter = DetectDelimiter(text);
            var records = Parse(text, delimiter);
            var document = new CsvDocument
            {
                Delimiter = delimiter,
                FirstRowIsHeader = firstRowIsHeader
            };

            int columnCount = records.Count == 0 ? 0 : records.Max(row => row.Count);
            if (firstRowIsHeader && records.Count > 0)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    string value = column < records[0].Count ? records[0][column] : string.Empty;
                    document.Headers.Add(string.IsNullOrWhiteSpace(value) ? "Spalte " + (column + 1) : value);
                }
                records.RemoveAt(0);
            }
            else
            {
                for (int column = 0; column < columnCount; column++)
                    document.Headers.Add("Spalte " + (column + 1));
            }

            foreach (var record in records)
            {
                while (record.Count < columnCount)
                    record.Add(string.Empty);
                document.Rows.Add(record);
            }

            return document;
        }

        public static void Save(string path, CsvDocument document, IList<IList<string>> rows)
        {
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                if (document.FirstRowIsHeader)
                    WriteRecord(writer, document.Headers, document.Delimiter);

                foreach (var row in rows)
                    WriteRecord(writer, row, document.Delimiter);
            }
        }

        public static CsvDocument SelectColumns(CsvDocument source, IList<int> selectedColumns)
        {
            var result = new CsvDocument
            {
                Delimiter = source.Delimiter,
                FirstRowIsHeader = source.FirstRowIsHeader
            };

            foreach (int column in selectedColumns)
            {
                if (column < 0 || column >= source.Headers.Count)
                    throw new ArgumentOutOfRangeException("selectedColumns");
                result.Headers.Add(source.Headers[column]);
            }

            foreach (var sourceRow in source.Rows)
            {
                var row = new List<string>();
                foreach (int column in selectedColumns)
                    row.Add(column < sourceRow.Count ? sourceRow[column] : string.Empty);
                result.Rows.Add(row);
            }
            return result;
        }

        private static void WriteRecord(TextWriter writer, IEnumerable<string> values, char delimiter)
        {
            writer.WriteLine(string.Join(delimiter.ToString(), values.Select(value => Escape(value ?? string.Empty, delimiter))));
        }

        private static string Escape(string value, char delimiter)
        {
            if (value.IndexOfAny(new[] { delimiter, '"', '\r', '\n' }) < 0)
                return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        internal static char DetectDelimiter(string text)
        {
            string firstRecord = GetFirstLogicalRecord(text);
            char[] candidates = { ';', ',', '\t' };
            char best = ';';
            int bestCount = -1;
            foreach (char candidate in candidates)
            {
                int count = CountOutsideQuotes(firstRecord, candidate);
                if (count > bestCount)
                {
                    bestCount = count;
                    best = candidate;
                }
            }
            return best;
        }

        private static string GetFirstLogicalRecord(string text)
        {
            var builder = new StringBuilder();
            bool quoted = false;
            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                if (current == '"')
                {
                    if (quoted && index + 1 < text.Length && text[index + 1] == '"')
                    {
                        builder.Append("\"\"");
                        index++;
                        continue;
                    }
                    quoted = !quoted;
                }
                if (!quoted && (current == '\r' || current == '\n'))
                    break;
                builder.Append(current);
            }
            return builder.ToString();
        }

        private static int CountOutsideQuotes(string text, char delimiter)
        {
            bool quoted = false;
            int count = 0;
            for (int index = 0; index < text.Length; index++)
            {
                if (text[index] == '"')
                {
                    if (quoted && index + 1 < text.Length && text[index + 1] == '"')
                    {
                        index++;
                        continue;
                    }
                    quoted = !quoted;
                }
                else if (!quoted && text[index] == delimiter)
                {
                    count++;
                }
            }
            return count;
        }

        internal static List<List<string>> Parse(string text, char delimiter)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                if (quoted)
                {
                    if (current == '"')
                    {
                        if (index + 1 < text.Length && text[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                        }
                        else
                        {
                            quoted = false;
                        }
                    }
                    else
                    {
                        field.Append(current);
                    }
                }
                else if (current == '"' && field.Length == 0)
                {
                    quoted = true;
                }
                else if (current == delimiter)
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (current == '\r' || current == '\n')
                {
                    if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                        index++;
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    field.Append(current);
                }
            }

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row);
            }
            return rows;
        }
    }

    internal static class LetterShifter
    {
        public static string Shift(string value, int amount)
        {
            return Shift(value, new[] { amount });
        }

        public static string Shift(string value, IList<int> amounts)
        {
            if (string.IsNullOrEmpty(value) || amounts == null || amounts.Count == 0)
                return value;

            var result = new StringBuilder(value.Length);
            int letterIndex = 0;
            foreach (char character in value)
            {
                int amount = amounts[letterIndex % amounts.Count];
                if (character >= 'A' && character <= 'Z')
                {
                    result.Append(ShiftInRange(character, 'A', 26, amount));
                    letterIndex++;
                }
                else if (character >= 'a' && character <= 'z')
                {
                    result.Append(ShiftInRange(character, 'a', 26, amount));
                    letterIndex++;
                }
                else
                    result.Append(character);
            }
            return result.ToString();
        }

        private static char ShiftInRange(char value, char start, int length, int amount)
        {
            int shifted = ((value - start + amount) % length + length) % length;
            return (char)(start + shifted);
        }
    }

    internal static class ShiftSequence
    {
        internal static bool TryParse(string text, out List<int> values, out string errorMessage)
        {
            values = new List<int>();
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                errorMessage = "Bitte gib mindestens einen Zählwert ein, zum Beispiel 3-5-8-2.";
                return false;
            }
            if (text.Length > 200)
            {
                errorMessage = "Die Zählfolge ist zu lang.";
                return false;
            }
            string[] parts = text.Split(new[] { '-', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts.Length > 32)
            {
                errorMessage = "Die Zählfolge muss aus 1 bis 32 Zahlen bestehen.";
                return false;
            }
            foreach (string part in parts)
            {
                int value;
                if (!int.TryParse(part, out value) || value < 1 || value > 25)
                {
                    errorMessage = "Jeder Zählwert muss eine ganze Zahl zwischen 1 und 25 sein.";
                    values.Clear();
                    return false;
                }
                values.Add(value);
            }
            return true;
        }

        internal static string Normalize(string text, string fallback)
        {
            List<int> values;
            string error;
            if (!TryParse(text, out values, out error))
            {
                if (!TryParse(fallback, out values, out error))
                    values = new List<int> { 1 };
            }
            return Format(values);
        }

        internal static string Format(IEnumerable<int> values)
        {
            return string.Join("-", (values ?? Enumerable.Empty<int>()).Select(value => Math.Abs(value).ToString()).ToArray());
        }

        internal static List<int> ToFiveValues(IEnumerable<int> values)
        {
            var source = (values ?? Enumerable.Empty<int>()).Select(value => Math.Max(1, Math.Min(25, Math.Abs(value)))).Take(5).ToList();
            while (source.Count < 5)
                source.Add(1);
            return source;
        }

        internal static string NormalizeForMainMode(string text)
        {
            List<int> values;
            string errorMessage;
            if (!TryParse(text, out values, out errorMessage))
                return "1";
            return values.Count == 1 ? Format(values) : Format(ToFiveValues(values));
        }
    }

    internal sealed class AppSettings
    {
        public string DefaultShiftSequence = "1";
        public string InterfaceStyle = "Metro";
        public bool ConfirmBeforeShift = true;
        public bool CheckForUpdates = true;
        public bool AskForUpdateCheckOnStart;
        public long LastUpdateCheckUtcTicks;
        public string LatestKnownVersion = string.Empty;
        public string LastNotifiedVersion = string.Empty;

        private static string SettingsPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PANDA", "settings.ini");
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    return Parse(File.ReadAllLines(SettingsPath));
            }
            catch
            {
            }
            return new AppSettings();
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllLines(SettingsPath, Serialize(), new UTF8Encoding(false));
        }

        internal static AppSettings Parse(IEnumerable<string> lines)
        {
            var settings = new AppSettings();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;
                int separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;
                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();
                if (string.Equals(key, "DefaultShiftSequence", StringComparison.OrdinalIgnoreCase))
                {
                    settings.DefaultShiftSequence = ShiftSequence.Normalize(value, settings.DefaultShiftSequence);
                }
                else if (string.Equals(key, "DefaultShift", StringComparison.OrdinalIgnoreCase))
                {
                    int parsed;
                    if (int.TryParse(value, out parsed))
                        settings.DefaultShiftSequence = Math.Max(1, Math.Min(25, parsed)).ToString();
                }
                else if (string.Equals(key, "InterfaceStyle", StringComparison.OrdinalIgnoreCase))
                {
                    settings.InterfaceStyle = string.Equals(value, "Classic", StringComparison.OrdinalIgnoreCase) ? "Classic" : "Metro";
                }
                else if (string.Equals(key, "ConfirmBeforeShift", StringComparison.OrdinalIgnoreCase))
                {
                    bool parsed;
                    if (bool.TryParse(value, out parsed))
                        settings.ConfirmBeforeShift = parsed;
                }
                else if (string.Equals(key, "CheckForUpdates", StringComparison.OrdinalIgnoreCase))
                {
                    bool parsed;
                    if (bool.TryParse(value, out parsed))
                        settings.CheckForUpdates = parsed;
                }
                else if (string.Equals(key, "AskForUpdateCheckOnStart", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(key, "CheckForUpdatesEveryStart", StringComparison.OrdinalIgnoreCase))
                {
                    bool parsed;
                    if (bool.TryParse(value, out parsed))
                        settings.AskForUpdateCheckOnStart = parsed;
                }
                else if (string.Equals(key, "LastUpdateCheckUtcTicks", StringComparison.OrdinalIgnoreCase))
                {
                    long parsed;
                    if (long.TryParse(value, out parsed) && parsed >= 0)
                        settings.LastUpdateCheckUtcTicks = parsed;
                }
                else if (string.Equals(key, "LatestKnownVersion", StringComparison.OrdinalIgnoreCase))
                {
                    Version parsed;
                    if (UpdateChecker.TryParseVersionText(value, out parsed))
                        settings.LatestKnownVersion = UpdateChecker.DisplayVersion(parsed);
                }
                else if (string.Equals(key, "LastNotifiedVersion", StringComparison.OrdinalIgnoreCase))
                {
                    Version parsed;
                    if (UpdateChecker.TryParseVersionText(value, out parsed))
                        settings.LastNotifiedVersion = UpdateChecker.DisplayVersion(parsed);
                }
            }
            return settings;
        }

        internal string[] Serialize()
        {
            return new[]
            {
                "DefaultShiftSequence=" + DefaultShiftSequence,
                "InterfaceStyle=" + InterfaceStyle,
                "ConfirmBeforeShift=" + ConfirmBeforeShift,
                "CheckForUpdates=" + CheckForUpdates,
                "AskForUpdateCheckOnStart=" + AskForUpdateCheckOnStart,
                "LastUpdateCheckUtcTicks=" + LastUpdateCheckUtcTicks,
                "LatestKnownVersion=" + LatestKnownVersion,
                "LastNotifiedVersion=" + LastNotifiedVersion
            };
        }
    }

    [DataContract]
    internal sealed class GitHubReleaseResponse
    {
        [DataMember(Name = "tag_name")]
        public string TagName { get; set; }
    }

    internal sealed class UpdateCheckResult
    {
        public Version LatestVersion;
        public string ErrorMessage;
        public bool Success { get { return LatestVersion != null && string.IsNullOrEmpty(ErrorMessage); } }
    }

    internal static class UpdateChecker
    {
        internal const string ReleasePageUrl = "https://github.com/Pand0ra98/PANDA/releases/latest";
        private const string ApiUrl = "https://api.github.com/repos/Pand0ra98/PANDA/releases/latest";
        private const int MaximumResponseBytes = 65536;

        internal static UpdateCheckResult CheckLatestRelease()
        {
            try
            {
                string json = DownloadReleaseMetadata();
                return new UpdateCheckResult { LatestVersion = ParseLatestVersion(json) };
            }
            catch (Exception exception)
            {
                return new UpdateCheckResult { ErrorMessage = exception.Message };
            }
        }

        internal static Version ParseLatestVersion(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || Encoding.UTF8.GetByteCount(json) > MaximumResponseBytes)
                throw new InvalidDataException("Die Updateantwort ist leer oder zu groß.");
            var serializer = new DataContractJsonSerializer(typeof(GitHubReleaseResponse));
            GitHubReleaseResponse release;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json), false))
                release = serializer.ReadObject(stream) as GitHubReleaseResponse;
            Version version;
            if (release == null || !TryParseVersionText(release.TagName, out version))
                throw new InvalidDataException("GitHub hat keine gültige PANDA-Versionsnummer geliefert.");
            return version;
        }

        internal static bool TryParseVersionText(string text, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(text) || text.Length > 32)
                return false;
            string value = text.Trim();
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(1);
            string[] parts = value.Split('.');
            if (parts.Length < 2 || parts.Length > 4 || parts.Any(part => part.Length == 0 || part.Any(character => character < '0' || character > '9')))
                return false;
            Version parsed;
            if (!Version.TryParse(value, out parsed) || parsed.Major < 0 || parsed.Minor < 0)
                return false;
            version = NormalizeVersion(parsed);
            return true;
        }

        internal static bool IsNewer(Version candidate, Version current)
        {
            return NormalizeVersion(candidate).CompareTo(NormalizeVersion(current)) > 0;
        }

        internal static string DisplayVersion(Version version)
        {
            Version normalized = NormalizeVersion(version);
            string value = normalized.Major + "." + normalized.Minor + "." + normalized.Build;
            if (normalized.Revision > 0)
                value += "." + normalized.Revision;
            return value;
        }

        private static Version NormalizeVersion(Version version)
        {
            if (version == null)
                return new Version(0, 0, 0, 0);
            return new Version(version.Major, version.Minor, Math.Max(0, version.Build), Math.Max(0, version.Revision));
        }

        private static string DownloadReleaseMetadata()
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "GET";
            request.Accept = "application/vnd.github+json";
            request.UserAgent = "PANDA-UpdateCheck/2.0";
            request.Headers["X-GitHub-Api-Version"] = "2022-11-28";
            request.AllowAutoRedirect = false;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK
                    || response.ResponseUri.Scheme != Uri.UriSchemeHttps
                    || !string.Equals(response.ResponseUri.Host, "api.github.com", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Die GitHub-Updateantwort stammt nicht von der erwarteten Adresse.");
                if (response.ContentLength > MaximumResponseBytes)
                    throw new InvalidDataException("Die GitHub-Updateantwort ist zu groß.");
                using (Stream input = response.GetResponseStream())
                using (var output = new MemoryStream())
                {
                    var buffer = new byte[4096];
                    int total = 0;
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        total += read;
                        if (total > MaximumResponseBytes)
                            throw new InvalidDataException("Die GitHub-Updateantwort ist zu groß.");
                        output.Write(buffer, 0, read);
                    }
                    return new UTF8Encoding(false, true).GetString(output.ToArray());
                }
            }
        }
    }

    internal sealed class SettingsForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly TextBox defaultShiftSequenceTextBox = new TextBox();
        private readonly ComboBox interfaceStyleComboBox = new ComboBox();
        private readonly CheckBox confirmationCheckBox = new CheckBox();
        private readonly CheckBox updateCheckBox = new CheckBox();
        private readonly CheckBox askForUpdateCheckBox = new CheckBox();

        public string DefaultShiftSequence { get; private set; }
        public string InterfaceStyle { get { return interfaceStyleComboBox.SelectedIndex == 1 ? "Classic" : "Metro"; } }
        public bool ConfirmBeforeShift { get { return confirmationCheckBox.Checked; } }
        public bool CheckForUpdates { get { return updateCheckBox.Checked; } }
        public bool AskForUpdateCheckOnStart { get { return askForUpdateCheckBox.Checked; } }

        public SettingsForm(AppSettings settings)
        {
            Text = "PANDA – Einstellungen";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(600, 560);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout(settings);
        }

        private void BuildLayout(AppSettings settings)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24, 20, 24, 18),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            Controls.Add(root);

            var heading = new Panel { Dock = DockStyle.Fill };
            heading.Controls.Add(new Label
            {
                Text = "Einstellungen",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            heading.Controls.Add(new Label
            {
                Text = "Lege die Standardwerte für zukünftige Umwandlungen fest.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 40)
            });
            root.Controls.Add(heading, 0, 0);

            var card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11,
                BackColor = Color.White,
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(0, 0, 0, 12)
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            var shiftLabel = new Label
            {
                Text = "Standard-Zählfolge",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            card.Controls.Add(shiftLabel, 0, 0);
            card.SetColumnSpan(shiftLabel, 2);
            card.Controls.Add(new Label
            {
                Text = "Ein Wert für Einfach oder genau fünf Werte für Erweitert; zulässig sind 1 bis 25.",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 1);
            defaultShiftSequenceTextBox.Text = ShiftSequence.NormalizeForMainMode(settings.DefaultShiftSequence);
            defaultShiftSequenceTextBox.TextAlign = HorizontalAlignment.Center;
            defaultShiftSequenceTextBox.MaxLength = 200;
            defaultShiftSequenceTextBox.Dock = DockStyle.Fill;
            defaultShiftSequenceTextBox.Margin = new Padding(8, 5, 0, 5);
            card.Controls.Add(defaultShiftSequenceTextBox, 1, 1);
            confirmationCheckBox.Text = "Vor jeder Umwandlung eine Bestätigung mit der gewählten Zählfolge anzeigen";
            confirmationCheckBox.Checked = settings.ConfirmBeforeShift;
            confirmationCheckBox.AutoSize = true;
            confirmationCheckBox.ForeColor = Navy;
            confirmationCheckBox.Anchor = AnchorStyles.Left;
            card.SetColumnSpan(confirmationCheckBox, 2);
            card.Controls.Add(confirmationCheckBox, 0, 3);
            updateCheckBox.Text = "Automatische Updateprüfung aktivieren (höchstens einmal täglich)";
            updateCheckBox.Checked = settings.CheckForUpdates;
            updateCheckBox.AutoSize = true;
            updateCheckBox.ForeColor = Navy;
            updateCheckBox.Anchor = AnchorStyles.Left;
            card.SetColumnSpan(updateCheckBox, 2);
            card.Controls.Add(updateCheckBox, 0, 5);
            askForUpdateCheckBox.Text = "Stattdessen bei jedem Programmstart vorher nachfragen";
            askForUpdateCheckBox.Checked = settings.AskForUpdateCheckOnStart;
            askForUpdateCheckBox.AutoSize = true;
            askForUpdateCheckBox.ForeColor = Navy;
            askForUpdateCheckBox.Anchor = AnchorStyles.Left;
            askForUpdateCheckBox.Margin = new Padding(22, 3, 3, 3);
            card.SetColumnSpan(askForUpdateCheckBox, 2);
            card.Controls.Add(askForUpdateCheckBox, 0, 6);
            updateCheckBox.CheckedChanged += delegate { RefreshUpdateFrequencyState(); };
            RefreshUpdateFrequencyState();
            var designLabel = new Label
            {
                Text = "Oberflächendesign",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            card.SetColumnSpan(designLabel, 2);
            card.Controls.Add(designLabel, 0, 8);
            interfaceStyleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            interfaceStyleComboBox.Items.AddRange(new object[] { "Metro (neu)", "Klassisch (Backup)" });
            interfaceStyleComboBox.SelectedIndex = string.Equals(settings.InterfaceStyle, "Classic", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            interfaceStyleComboBox.Dock = DockStyle.Fill;
            interfaceStyleComboBox.Margin = new Padding(0, 4, 0, 4);
            card.SetColumnSpan(interfaceStyleComboBox, 2);
            card.Controls.Add(interfaceStyleComboBox, 0, 9);
            var designNote = new Label
            {
                Text = "Das gewählte Design wird nach dem Speichern sofort angewendet.",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            card.SetColumnSpan(designNote, 2);
            card.Controls.Add(designNote, 0, 10);
            root.Controls.Add(card, 0, 1);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 0)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            var cancelButton = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Navy,
                Margin = new Padding(0)
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
            var saveButton = new Button
            {
                Text = "Speichern",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Blue,
                ForeColor = Color.White,
                Margin = new Padding(0)
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += delegate { SaveSettings(); };
            footer.Controls.Add(cancelButton, 1, 0);
            footer.Controls.Add(saveButton, 3, 0);
            root.Controls.Add(footer, 0, 2);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private void RefreshUpdateFrequencyState()
        {
            askForUpdateCheckBox.Enabled = updateCheckBox.Checked;
        }

        private void SaveSettings()
        {
            List<int> values;
            string errorMessage;
            if (!ShiftSequence.TryParse(defaultShiftSequenceTextBox.Text, out values, out errorMessage))
            {
                MessageBox.Show(this, errorMessage, "Ungültige Zählfolge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                defaultShiftSequenceTextBox.Focus();
                defaultShiftSequenceTextBox.SelectAll();
                return;
            }
            if (values.Count != 1 && values.Count != 5)
            {
                MessageBox.Show(this,
                    "Für den einfachen Modus ist genau ein Wert nötig. Für den erweiterten Modus sind genau fünf Werte nötig.",
                    "Ungültige Anzahl von Zählwerten",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                defaultShiftSequenceTextBox.Focus();
                defaultShiftSequenceTextBox.SelectAll();
                return;
            }
            DefaultShiftSequence = ShiftSequence.Format(values);
            defaultShiftSequenceTextBox.Text = DefaultShiftSequence;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal enum ExportRowMode
    {
        All,
        First,
        Changed,
        Custom
    }

    internal static class ExportRowSelector
    {
        internal static List<int> SelectRows(int rowCount, ExportRowMode mode, int firstCount, IEnumerable<int> changedRows, IEnumerable<int> customRows)
        {
            if (rowCount <= 0)
                return new List<int>();
            if (mode == ExportRowMode.All)
                return Enumerable.Range(0, rowCount).ToList();
            if (mode == ExportRowMode.First)
                return Enumerable.Range(0, Math.Max(0, Math.Min(firstCount, rowCount))).ToList();
            return Normalize(mode == ExportRowMode.Changed ? changedRows : customRows, rowCount);
        }

        internal static List<int> FindChangedRows(IList<IList<string>> originalRows, IList<IList<string>> currentRows)
        {
            var changed = new List<int>();
            int rowCount = Math.Max(originalRows.Count, currentRows.Count);
            for (int row = 0; row < rowCount; row++)
            {
                if (row >= originalRows.Count || row >= currentRows.Count
                    || !originalRows[row].SequenceEqual(currentRows[row], StringComparer.Ordinal))
                    changed.Add(row);
            }
            return changed;
        }

        private static List<int> Normalize(IEnumerable<int> rows, int rowCount)
        {
            return (rows ?? Enumerable.Empty<int>())
                .Where(row => row >= 0 && row < rowCount)
                .Distinct()
                .OrderBy(row => row)
                .ToList();
        }
    }

    internal sealed class ExportOptionsForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly Color Green = Color.FromArgb(29, 132, 88);
        private readonly IList<IList<string>> rows;
        private readonly HashSet<int> changedRows;
        private readonly HashSet<int> initialRows;
        private readonly RadioButton allRadio = new RadioButton();
        private readonly RadioButton firstRadio = new RadioButton();
        private readonly RadioButton changedRadio = new RadioButton();
        private readonly RadioButton customRadio = new RadioButton();
        private readonly NumericUpDown firstCountNumeric = new NumericUpDown();
        private readonly CheckedListBox rowList = new CheckedListBox();
        private readonly Button selectAllButton = new Button();
        private readonly Button selectNoneButton = new Button();
        private readonly Button selectChangedButton = new Button();
        private readonly Button continueButton = new Button();
        private readonly Label summaryLabel = new Label();

        public List<int> SelectedRowIndices { get; private set; }

        public ExportOptionsForm(IList<string> headers, IList<IList<string>> rows, IEnumerable<int> changedRows, IEnumerable<int> initiallySelectedRows)
        {
            this.rows = rows;
            this.changedRows = new HashSet<int>(changedRows ?? Enumerable.Empty<int>());
            this.initialRows = new HashSet<int>(initiallySelectedRows ?? Enumerable.Empty<int>());
            Text = "PANDA – CSV-Export";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(680, 600);
            ClientSize = new Size(720, 650);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout(headers);
            allRadio.Checked = true;
            SetCheckedRows(this.initialRows);
            UpdateState();
        }

        private void BuildLayout(IList<string> headers)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(24, 20, 24, 18),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 164));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            Controls.Add(root);

            var heading = new Panel { Dock = DockStyle.Fill };
            heading.Controls.Add(new Label
            {
                Text = "Zeilen für den Export",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            heading.Controls.Add(new Label
            {
                Text = "Lege fest, welche Zeilen in die neue CSV-Datei geschrieben werden.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 40)
            });
            root.Controls.Add(heading, 0, 0);

            var optionsCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White,
                Padding = new Padding(16, 10, 16, 10),
                Margin = new Padding(0, 0, 0, 12)
            };
            for (int row = 0; row < 4; row++)
                optionsCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            ConfigureRadio(allRadio, "Alle Zeilen (" + rows.Count + ")");
            optionsCard.Controls.Add(allRadio, 0, 0);

            var firstPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };
            ConfigureRadio(firstRadio, "Nur die ersten");
            firstRadio.Width = 128;
            firstPanel.Controls.Add(firstRadio);
            firstCountNumeric.Minimum = 1;
            firstCountNumeric.Maximum = Math.Max(1, rows.Count);
            firstCountNumeric.Value = Math.Max(1, Math.Min(10, rows.Count));
            firstCountNumeric.Width = 76;
            firstCountNumeric.TextAlign = HorizontalAlignment.Center;
            firstCountNumeric.Margin = new Padding(4, 4, 7, 3);
            firstCountNumeric.ValueChanged += delegate { UpdateState(); };
            firstPanel.Controls.Add(firstCountNumeric);
            firstPanel.Controls.Add(new Label
            {
                Text = "Zeilen",
                ForeColor = Navy,
                AutoSize = true,
                Margin = new Padding(0, 7, 0, 0)
            });
            optionsCard.Controls.Add(firstPanel, 0, 1);

            ConfigureRadio(changedRadio, "Nur veränderte Zeilen (" + this.changedRows.Count + ")");
            changedRadio.Enabled = this.changedRows.Count > 0;
            optionsCard.Controls.Add(changedRadio, 0, 2);
            ConfigureRadio(customRadio, "Eigene Zeilenauswahl über die Checkboxen unten");
            optionsCard.Controls.Add(customRadio, 0, 3);
            root.Controls.Add(optionsCard, 0, 1);

            var selectionCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0)
            };
            selectionCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            selectionCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            selectionCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            selectionCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            selectionCard.Controls.Add(new Label
            {
                Text = "Freie Zeilenauswahl",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 0);

            var selectionButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };
            selectAllButton.Text = "Alle markieren";
            selectNoneButton.Text = "Keine markieren";
            selectChangedButton.Text = "Veränderte markieren";
            foreach (Button button in new[] { selectAllButton, selectNoneButton, selectChangedButton })
            {
                StyleSecondaryButton(button);
                button.Dock = DockStyle.None;
                button.Width = button == selectChangedButton ? 160 : 130;
                button.Height = 32;
                button.Margin = new Padding(0, 3, 10, 3);
                selectionButtons.Controls.Add(button);
            }
            selectAllButton.Click += delegate { customRadio.Checked = true; SetCheckedRows(Enumerable.Range(0, rows.Count)); UpdateState(); };
            selectNoneButton.Click += delegate { customRadio.Checked = true; SetCheckedRows(new int[0]); UpdateState(); };
            selectChangedButton.Click += delegate { customRadio.Checked = true; SetCheckedRows(this.changedRows); UpdateState(); };
            selectionCard.Controls.Add(selectionButtons, 0, 1);

            rowList.CheckOnClick = true;
            rowList.Dock = DockStyle.Fill;
            rowList.BorderStyle = BorderStyle.FixedSingle;
            rowList.BackColor = Color.White;
            rowList.ForeColor = Navy;
            rowList.IntegralHeight = false;
            rowList.ItemCheck += delegate
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateState));
            };
            for (int row = 0; row < rows.Count; row++)
                rowList.Items.Add(BuildRowCaption(headers, row));
            selectionCard.Controls.Add(rowList, 0, 2);
            selectionCard.Controls.Add(new Label
            {
                Text = "Diese Checkboxen gelten nur für den Export und ändern keine Markierung im Hauptfenster.",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 3);
            root.Controls.Add(selectionCard, 0, 2);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            summaryLabel.ForeColor = Muted;
            summaryLabel.AutoEllipsis = true;
            summaryLabel.Dock = DockStyle.Fill;
            summaryLabel.TextAlign = ContentAlignment.MiddleLeft;
            footer.Controls.Add(summaryLabel, 0, 0);
            var cancelButton = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel };
            StyleSecondaryButton(cancelButton);
            footer.Controls.Add(cancelButton, 1, 0);
            continueButton.Text = "Weiter";
            StylePrimaryButton(continueButton);
            continueButton.Click += delegate { ConfirmSelection(); };
            footer.Controls.Add(continueButton, 3, 0);
            root.Controls.Add(footer, 0, 3);
            AcceptButton = continueButton;
            CancelButton = cancelButton;
        }

        private void ConfigureRadio(RadioButton radio, string text)
        {
            radio.Text = text;
            radio.ForeColor = Navy;
            radio.AutoSize = true;
            radio.Anchor = AnchorStyles.Left;
            radio.Margin = new Padding(0, 6, 0, 4);
            radio.CheckedChanged += delegate { UpdateState(); };
        }

        private string BuildRowCaption(IList<string> headers, int rowIndex)
        {
            var values = rows[rowIndex].Take(3).Select(value => value ?? string.Empty).ToArray();
            string preview = string.Join("  |  ", values);
            if (preview.Length > 100)
                preview = preview.Substring(0, 97) + "...";
            string changed = changedRows.Contains(rowIndex) ? "  •  verändert" : string.Empty;
            return "Zeile " + (rowIndex + 1) + changed + "  —  " + preview;
        }

        private void StylePrimaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Blue;
            button.ForeColor = Color.White;
            button.Margin = new Padding(0);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
        }

        private void StyleSecondaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Navy;
            button.Margin = new Padding(0);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
        }

        private void SetCheckedRows(IEnumerable<int> selectedRows)
        {
            var selected = new HashSet<int>(selectedRows ?? Enumerable.Empty<int>());
            for (int row = 0; row < rowList.Items.Count; row++)
                rowList.SetItemChecked(row, selected.Contains(row));
        }

        private List<int> GetCheckedRows()
        {
            var selected = new List<int>();
            for (int row = 0; row < rowList.Items.Count; row++)
                if (rowList.GetItemChecked(row)) selected.Add(row);
            return selected;
        }

        private ExportRowMode CurrentMode
        {
            get
            {
                if (firstRadio.Checked) return ExportRowMode.First;
                if (changedRadio.Checked) return ExportRowMode.Changed;
                if (customRadio.Checked) return ExportRowMode.Custom;
                return ExportRowMode.All;
            }
        }

        private List<int> GetCurrentSelection()
        {
            return ExportRowSelector.SelectRows(rows.Count, CurrentMode, (int)firstCountNumeric.Value, changedRows, GetCheckedRows());
        }

        private void UpdateState()
        {
            bool custom = customRadio.Checked;
            firstCountNumeric.Enabled = firstRadio.Checked;
            rowList.Enabled = custom;
            selectAllButton.Enabled = custom;
            selectNoneButton.Enabled = custom;
            selectChangedButton.Enabled = custom && changedRows.Count > 0;
            int count = GetCurrentSelection().Count;
            summaryLabel.Text = count + " von " + rows.Count + " Zeilen werden exportiert.";
            summaryLabel.ForeColor = count > 0 ? Green : Color.FromArgb(190, 60, 55);
            continueButton.Enabled = count > 0;
        }

        private void ConfirmSelection()
        {
            List<int> selected = GetCurrentSelection();
            if (selected.Count == 0)
            {
                MessageBox.Show(this, "Bitte wähle mindestens eine Zeile für den Export aus.", "Keine Zeilen ausgewählt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedRowIndices = selected;
            DialogResult = DialogResult.OK;
            Close();
        }

        internal void SelectCustomModeForPreview()
        {
            customRadio.Checked = true;
            UpdateState();
        }
    }

    internal sealed class SelectionTemplate
    {
        public string Name;
        public List<string> Columns;

        public SelectionTemplate(string name, IEnumerable<string> columns)
        {
            Name = name ?? string.Empty;
            Columns = columns == null ? new List<string>() : columns.ToList();
        }

        public SelectionTemplate Clone()
        {
            return new SelectionTemplate(Name, Columns);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal static class SelectionTemplateStore
    {
        private static string TemplatesPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PANDA", "selection-templates.dat");
            }
        }

        public static List<SelectionTemplate> Load()
        {
            var templates = new List<SelectionTemplate>();
            try
            {
                if (!File.Exists(TemplatesPath))
                    return templates;
                foreach (string line in File.ReadAllLines(TemplatesPath, Encoding.UTF8))
                {
                    SelectionTemplate template;
                    if (!TryParseLine(line, out template))
                        continue;
                    int existing = templates.FindIndex(item => string.Equals(item.Name, template.Name, StringComparison.OrdinalIgnoreCase));
                    if (existing >= 0)
                        templates[existing] = template;
                    else
                        templates.Add(template);
                }
            }
            catch
            {
            }
            return templates.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public static void Save(IEnumerable<SelectionTemplate> templates)
        {
            string directory = Path.GetDirectoryName(TemplatesPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string[] lines = templates
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Name) && item.Columns.Count > 0)
                .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(SerializeLine)
                .ToArray();
            File.WriteAllLines(TemplatesPath, lines, new UTF8Encoding(false));
        }

        internal static string SerializeLine(SelectionTemplate template)
        {
            var parts = new List<string> { Encode(template.Name.Trim()) };
            parts.AddRange(template.Columns
                .Where(column => !string.IsNullOrWhiteSpace(column))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(column => Encode(column.Trim())));
            return string.Join("|", parts.ToArray());
        }

        internal static bool TryParseLine(string line, out SelectionTemplate template)
        {
            template = null;
            if (string.IsNullOrWhiteSpace(line))
                return false;
            try
            {
                string[] parts = line.Split('|');
                if (parts.Length < 2)
                    return false;
                string name = Decode(parts[0]).Trim();
                var columns = parts.Skip(1)
                    .Select(Decode)
                    .Where(column => !string.IsNullOrWhiteSpace(column))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (name.Length == 0 || columns.Count == 0)
                    return false;
                template = new SelectionTemplate(name, columns);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        internal static List<int> FindColumnIndices(IList<string> headers, IEnumerable<string> templateColumns)
        {
            var wanted = new HashSet<string>(templateColumns ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var indices = new List<int>();
            for (int index = 0; index < headers.Count; index++)
                if (wanted.Contains(headers[index]))
                    indices.Add(index);
            return indices;
        }

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        private static string Decode(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }

    internal sealed class SelectionTemplatesForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly IList<string> headers;
        private readonly List<SelectionTemplate> templates;
        private readonly HashSet<int> initialSelectedColumns;
        private readonly bool persistChanges;
        private readonly ComboBox templateComboBox = new ComboBox();
        private readonly TextBox nameTextBox = new TextBox();
        private readonly CheckedListBox columnList = new CheckedListBox();
        private readonly Button applyButton = new Button();
        private readonly Button deleteButton = new Button();
        private readonly Label editorStatusLabel = new Label();

        public SelectionTemplate TemplateToApply { get; private set; }

        public SelectionTemplatesForm(IList<string> headers, List<SelectionTemplate> templates, IEnumerable<int> selectedColumns)
            : this(headers, templates, selectedColumns, true)
        {
        }

        internal SelectionTemplatesForm(IList<string> headers, List<SelectionTemplate> templates, IEnumerable<int> selectedColumns, bool persistChanges)
        {
            this.headers = headers;
            this.templates = templates;
            this.initialSelectedColumns = new HashSet<int>(selectedColumns ?? Enumerable.Empty<int>());
            this.persistChanges = persistChanges;
            Text = "PANDA – Auswahlvorlagen";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(610, 560);
            ClientSize = new Size(650, 610);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout();
            RefreshTemplateList(null);
            RestoreInitialSelection();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(24, 20, 24, 18),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            Controls.Add(root);

            var heading = new Panel { Dock = DockStyle.Fill };
            heading.Controls.Add(new Label
            {
                Text = "Auswahlvorlagen",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            heading.Controls.Add(new Label
            {
                Text = "Speichere häufig benötigte Spaltenkombinationen und wende sie mit einem Klick an.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 40)
            });
            root.Controls.Add(heading, 0, 0);

            var existingCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 12)
            };
            existingCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            existingCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            existingCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            existingCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
            existingCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            existingCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            var existingLabel = new Label
            {
                Text = "Gespeicherte Vorlage",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            existingCard.SetColumnSpan(existingLabel, 4);
            existingCard.Controls.Add(existingLabel, 0, 0);
            templateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            templateComboBox.Dock = DockStyle.Fill;
            templateComboBox.Margin = new Padding(0, 3, 10, 3);
            templateComboBox.SelectedIndexChanged += delegate { LoadSelectedTemplate(); };
            existingCard.Controls.Add(templateComboBox, 0, 1);
            applyButton.Text = "Anwenden";
            StylePrimaryButton(applyButton);
            applyButton.Click += delegate { ApplySelectedTemplate(); };
            existingCard.Controls.Add(applyButton, 1, 1);
            deleteButton.Text = "Löschen";
            StyleSecondaryButton(deleteButton);
            deleteButton.Click += delegate { DeleteSelectedTemplate(); };
            existingCard.Controls.Add(deleteButton, 3, 1);
            root.Controls.Add(existingCard, 0, 1);

            var editorCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                BackColor = Color.White,
                Padding = new Padding(16, 14, 16, 14),
                Margin = new Padding(0)
            };
            editorCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            editorCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            editorCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142));
            editorCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            editorCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            editorCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            editorCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            editorCard.Controls.Add(new Label
            {
                Text = "Vorlagenname",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 0);
            nameTextBox.Dock = DockStyle.Fill;
            nameTextBox.Margin = new Padding(0, 6, 12, 6);
            editorCard.Controls.Add(nameTextBox, 1, 0);
            var saveButton = new Button { Text = "Vorlage speichern" };
            StylePrimaryButton(saveButton);
            saveButton.Click += delegate { SaveTemplate(); };
            editorCard.Controls.Add(saveButton, 2, 0);
            var columnsLabel = new Label
            {
                Text = "Spalten für die Vorlage auswählen",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            editorCard.SetColumnSpan(columnsLabel, 3);
            editorCard.Controls.Add(columnsLabel, 0, 1);
            columnList.CheckOnClick = true;
            columnList.Dock = DockStyle.Fill;
            columnList.BorderStyle = BorderStyle.FixedSingle;
            columnList.BackColor = Color.White;
            columnList.ForeColor = Navy;
            columnList.ItemCheck += delegate
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateEditorStatus));
            };
            foreach (string header in headers)
                columnList.Items.Add(header);
            editorCard.SetColumnSpan(columnList, 3);
            editorCard.Controls.Add(columnList, 0, 2);
            editorStatusLabel.ForeColor = Muted;
            editorStatusLabel.AutoEllipsis = true;
            editorStatusLabel.Dock = DockStyle.Fill;
            editorStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            editorCard.SetColumnSpan(editorStatusLabel, 3);
            editorCard.Controls.Add(editorStatusLabel, 0, 3);
            root.Controls.Add(editorCard, 0, 2);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            var closeButton = new Button
            {
                Text = "Schließen",
                DialogResult = DialogResult.Cancel
            };
            StyleSecondaryButton(closeButton);
            footer.Controls.Add(closeButton, 1, 0);
            root.Controls.Add(footer, 0, 3);
            CancelButton = closeButton;
        }

        private void StylePrimaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Blue;
            button.ForeColor = Color.White;
            button.Margin = new Padding(0, 3, 0, 3);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
        }

        private void StyleSecondaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Navy;
            button.Margin = new Padding(0, 3, 0, 3);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
        }

        private void RefreshTemplateList(string selectedName)
        {
            templateComboBox.BeginUpdate();
            templateComboBox.Items.Clear();
            foreach (SelectionTemplate template in templates.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
                templateComboBox.Items.Add(template);
            templateComboBox.EndUpdate();
            templateComboBox.SelectedIndex = -1;
            if (!string.IsNullOrEmpty(selectedName))
            {
                for (int index = 0; index < templateComboBox.Items.Count; index++)
                {
                    var item = (SelectionTemplate)templateComboBox.Items[index];
                    if (string.Equals(item.Name, selectedName, StringComparison.OrdinalIgnoreCase))
                    {
                        templateComboBox.SelectedIndex = index;
                        break;
                    }
                }
            }
            UpdateTemplateButtons();
        }

        private void RestoreInitialSelection()
        {
            nameTextBox.Clear();
            for (int index = 0; index < columnList.Items.Count; index++)
                columnList.SetItemChecked(index, initialSelectedColumns.Contains(index));
            UpdateEditorStatus();
        }

        private void LoadSelectedTemplate()
        {
            var template = templateComboBox.SelectedItem as SelectionTemplate;
            UpdateTemplateButtons();
            if (template == null)
                return;
            nameTextBox.Text = template.Name;
            var selected = new HashSet<int>(SelectionTemplateStore.FindColumnIndices(headers, template.Columns));
            for (int index = 0; index < columnList.Items.Count; index++)
                columnList.SetItemChecked(index, selected.Contains(index));
            UpdateEditorStatus();
        }

        private void UpdateTemplateButtons()
        {
            bool hasSelection = templateComboBox.SelectedItem is SelectionTemplate;
            applyButton.Enabled = hasSelection;
            deleteButton.Enabled = hasSelection;
        }

        private void UpdateEditorStatus()
        {
            int count = 0;
            for (int index = 0; index < columnList.Items.Count; index++)
                if (columnList.GetItemChecked(index)) count++;
            editorStatusLabel.Text = count == 1 ? "1 Spalte ausgewählt" : count + " Spalten ausgewählt";
        }

        private List<string> GetCheckedColumns()
        {
            var columns = new List<string>();
            for (int index = 0; index < columnList.Items.Count; index++)
                if (columnList.GetItemChecked(index)) columns.Add(headers[index]);
            return columns.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void SaveTemplate()
        {
            string name = nameTextBox.Text.Trim();
            List<string> columns = GetCheckedColumns();
            if (name.Length == 0)
            {
                MessageBox.Show(this, "Bitte gib einen Namen für die Vorlage ein.", "Name fehlt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                nameTextBox.Focus();
                return;
            }
            if (columns.Count == 0)
            {
                MessageBox.Show(this, "Bitte wähle mindestens eine Spalte für die Vorlage aus.", "Keine Spalten", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int existingIndex = templates.FindIndex(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                DialogResult overwrite = MessageBox.Show(this, "Die Vorlage „" + templates[existingIndex].Name + "“ existiert bereits. Möchtest du sie überschreiben?", "Vorlage überschreiben", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (overwrite != DialogResult.Yes)
                    return;
            }

            var updated = templates.Select(item => item.Clone()).ToList();
            var newTemplate = new SelectionTemplate(name, columns);
            if (existingIndex >= 0)
                updated[existingIndex] = newTemplate;
            else
                updated.Add(newTemplate);
            try
            {
                if (persistChanges)
                    SelectionTemplateStore.Save(updated);
                templates.Clear();
                templates.AddRange(updated);
                RefreshTemplateList(name);
                editorStatusLabel.Text = "Vorlage „" + name + "“ gespeichert – " + columns.Count + " Spalten.";
                editorStatusLabel.ForeColor = Color.FromArgb(29, 132, 88);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Die Vorlage konnte nicht gespeichert werden.\r\n\r\n" + exception.Message, "Speichern fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedTemplate()
        {
            var selected = templateComboBox.SelectedItem as SelectionTemplate;
            if (selected == null)
                return;
            DialogResult confirmation = MessageBox.Show(this, "Möchtest du die Vorlage „" + selected.Name + "“ wirklich löschen?", "Vorlage löschen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirmation != DialogResult.Yes)
                return;
            var updated = templates.Where(item => !string.Equals(item.Name, selected.Name, StringComparison.OrdinalIgnoreCase)).Select(item => item.Clone()).ToList();
            try
            {
                if (persistChanges)
                    SelectionTemplateStore.Save(updated);
                templates.Clear();
                templates.AddRange(updated);
                RefreshTemplateList(null);
                RestoreInitialSelection();
                editorStatusLabel.Text = "Vorlage „" + selected.Name + "“ gelöscht.";
                editorStatusLabel.ForeColor = Muted;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Die Vorlage konnte nicht gelöscht werden.\r\n\r\n" + exception.Message, "Löschen fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplySelectedTemplate()
        {
            var selected = templateComboBox.SelectedItem as SelectionTemplate;
            if (selected == null)
                return;
            TemplateToApply = selected.Clone();
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal sealed class ImportWizardForm : Form
    {
        private readonly string csvPath;
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly CheckBox headerCheckBox = new CheckBox();
        private readonly Label formatLabel = new Label();
        private readonly Label selectionLabel = new Label();
        private readonly CheckedListBox columnList = new CheckedListBox();
        private readonly DataGridView previewGrid = new DataGridView();
        private readonly Button importButton = new Button();
        private CsvDocument loadedDocument;

        public CsvDocument SelectedDocument { get; private set; }

        public ImportWizardForm(string path)
        {
            csvPath = path;
            Text = "PANDA Import-Assistent";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 600);
            Size = new Size(1040, 680);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout();
            ReloadPreview(true);
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(22, 18, 22, 18),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            Controls.Add(root);

            var titlePanel = new Panel { Dock = DockStyle.Fill };
            titlePanel.Controls.Add(new Label
            {
                Text = "CSV-Import vorbereiten",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            titlePanel.Controls.Add(new Label
            {
                Text = "Prüfe das Format und wähle die Spalten aus, die PANDA übernehmen soll.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 39)
            });
            root.Controls.Add(titlePanel, 0, 0);

            var filePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(14, 8, 14, 8),
                Margin = new Padding(0, 0, 0, 12)
            };
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            filePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            filePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            var stepOne = new Label
            {
                Text = "1   DATEI UND FORMAT",
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Blue,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            filePanel.Controls.Add(stepOne, 0, 0);
            filePanel.SetColumnSpan(stepOne, 3);
            filePanel.Controls.Add(new Label
            {
                Text = Path.GetFileName(csvPath),
                ForeColor = Navy,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);
            headerCheckBox.Text = "Erste Zeile enthält Überschriften";
            headerCheckBox.Checked = true;
            headerCheckBox.AutoSize = true;
            headerCheckBox.ForeColor = Navy;
            headerCheckBox.Anchor = AnchorStyles.Left;
            headerCheckBox.CheckedChanged += delegate { ReloadPreview(false); };
            filePanel.Controls.Add(headerCheckBox, 1, 1);
            formatLabel.ForeColor = Muted;
            formatLabel.AutoEllipsis = true;
            formatLabel.Dock = DockStyle.Fill;
            formatLabel.TextAlign = ContentAlignment.MiddleRight;
            filePanel.Controls.Add(formatLabel, 2, 1);
            root.Controls.Add(filePanel, 0, 1);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Background,
                Margin = new Padding(0)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(CreateSectionHeader("2   SPALTEN AUSWÄHLEN", false), 0, 0);
            content.Controls.Add(CreateSectionHeader("VORSCHAU DER DATEI", true), 1, 0);
            root.Controls.Add(content, 0, 2);

            var selectionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 12),
                Margin = new Padding(0, 0, 7, 0)
            };
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            selectionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            selectionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            selectionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            var allButton = CreateLinkButton("Alle auswählen");
            allButton.Click += delegate { SetAllColumns(true); };
            var noneButton = CreateLinkButton("Auswahl aufheben");
            noneButton.Click += delegate { SetAllColumns(false); };
            selectionPanel.Controls.Add(allButton, 0, 0);
            selectionPanel.Controls.Add(noneButton, 1, 0);
            columnList.Dock = DockStyle.Fill;
            columnList.BorderStyle = BorderStyle.None;
            columnList.CheckOnClick = true;
            columnList.ForeColor = Navy;
            columnList.BackColor = Color.White;
            columnList.HorizontalScrollbar = true;
            columnList.ItemCheck += delegate(object sender, ItemCheckEventArgs args)
            {
                if (args.Index >= 0 && args.Index < previewGrid.Columns.Count)
                    previewGrid.Columns[args.Index].Visible = args.NewValue == CheckState.Checked;
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateSelectionStatus));
            };
            selectionPanel.SetColumnSpan(columnList, 2);
            selectionPanel.Controls.Add(columnList, 0, 1);
            selectionLabel.ForeColor = Muted;
            selectionLabel.Dock = DockStyle.Fill;
            selectionLabel.TextAlign = ContentAlignment.MiddleLeft;
            selectionPanel.SetColumnSpan(selectionLabel, 2);
            selectionPanel.Controls.Add(selectionLabel, 0, 2);
            content.Controls.Add(selectionPanel, 0, 1);

            ConfigurePreviewGrid();
            content.Controls.Add(previewGrid, 1, 1);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            importButton.Text = "Importieren";
            importButton.Dock = DockStyle.Fill;
            importButton.Margin = new Padding(0);
            importButton.FlatStyle = FlatStyle.Flat;
            importButton.FlatAppearance.BorderSize = 0;
            importButton.BackColor = Blue;
            importButton.ForeColor = Color.White;
            importButton.Cursor = Cursors.Hand;
            importButton.Click += delegate { FinishImport(); };
            var cancelButton = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Navy,
                Margin = new Padding(0)
            };
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
            footer.Controls.Add(cancelButton, 1, 0);
            footer.Controls.Add(importButton, 3, 0);
            root.Controls.Add(footer, 0, 3);
            AcceptButton = importButton;
            CancelButton = cancelButton;
        }

        private Panel CreateSectionHeader(string text, bool preview)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = preview ? Color.FromArgb(236, 243, 255) : Color.White,
                Margin = preview ? new Padding(7, 0, 0, 0) : new Padding(0, 0, 7, 0)
            };
            panel.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = preview ? Blue : Navy,
                AutoSize = true,
                Location = new Point(12, 12)
            });
            return panel;
        }

        private Button CreateLinkButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Blue,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 4)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void ConfigurePreviewGrid()
        {
            previewGrid.Dock = DockStyle.Fill;
            previewGrid.Margin = new Padding(7, 0, 0, 0);
            previewGrid.BackgroundColor = Color.White;
            previewGrid.BorderStyle = BorderStyle.None;
            previewGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            previewGrid.GridColor = Color.FromArgb(228, 234, 242);
            previewGrid.RowHeadersVisible = false;
            previewGrid.ColumnHeadersHeight = 36;
            previewGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            previewGrid.ColumnHeadersDefaultCellStyle.ForeColor = Navy;
            previewGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            previewGrid.EnableHeadersVisualStyles = false;
            previewGrid.DefaultCellStyle.BackColor = Color.White;
            previewGrid.DefaultCellStyle.ForeColor = Navy;
            previewGrid.DefaultCellStyle.SelectionBackColor = Color.White;
            previewGrid.DefaultCellStyle.SelectionForeColor = Navy;
            previewGrid.RowTemplate.Height = 28;
            previewGrid.AllowUserToAddRows = false;
            previewGrid.AllowUserToDeleteRows = false;
            previewGrid.AllowUserToOrderColumns = false;
            previewGrid.ReadOnly = true;
            previewGrid.MultiSelect = false;
            previewGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void ReloadPreview(bool firstLoad)
        {
            try
            {
                loadedDocument = CsvCodec.Load(csvPath, headerCheckBox.Checked);
                if (loadedDocument.Headers.Count == 0)
                    throw new InvalidDataException("Die Datei enthält keine auswertbaren CSV-Daten.");
                formatLabel.Text = loadedDocument.Rows.Count + " Zeilen  •  " + loadedDocument.Headers.Count + " Spalten  •  " + DelimiterName(loadedDocument.Delimiter);
                columnList.Items.Clear();
                foreach (string header in loadedDocument.Headers)
                    columnList.Items.Add(header, true);
                PopulatePreview();
                UpdateSelectionStatus();
            }
            catch (Exception exception)
            {
                if (!firstLoad)
                    MessageBox.Show(this, "Die Vorschau konnte nicht aktualisiert werden.\r\n\r\n" + exception.Message, "Import-Assistent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                importButton.Enabled = false;
            }
        }

        private void PopulatePreview()
        {
            previewGrid.SuspendLayout();
            previewGrid.Columns.Clear();
            previewGrid.Rows.Clear();
            for (int column = 0; column < loadedDocument.Headers.Count; column++)
            {
                previewGrid.Columns.Add("Preview" + column, loadedDocument.Headers[column]);
                previewGrid.Columns[column].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (var row in loadedDocument.Rows.Take(50))
                previewGrid.Rows.Add(row.Cast<object>().ToArray());
            previewGrid.ClearSelection();
            previewGrid.ResumeLayout();
        }

        private void SetAllColumns(bool selected)
        {
            for (int index = 0; index < columnList.Items.Count; index++)
                columnList.SetItemChecked(index, selected);
            BeginInvoke(new Action(UpdateSelectionStatus));
        }

        private void UpdateSelectionStatus()
        {
            int count = columnList.CheckedIndices.Count;
            selectionLabel.Text = count + " von " + columnList.Items.Count + " Spalten ausgewählt";
            importButton.Enabled = count > 0;
        }

        private void FinishImport()
        {
            var selected = columnList.CheckedIndices.Cast<int>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show(this, "Bitte wähle mindestens eine Spalte aus.", "Keine Spalte ausgewählt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedDocument = CsvCodec.SelectColumns(loadedDocument, selected);
            DialogResult = DialogResult.OK;
            Close();
        }

        private static string DelimiterName(char delimiter)
        {
            if (delimiter == ';') return "Semikolon";
            if (delimiter == ',') return "Komma";
            if (delimiter == '\t') return "Tabulator";
            return delimiter.ToString();
        }

        internal void UncheckLastColumnForPreview()
        {
            if (columnList.Items.Count > 1)
                columnList.SetItemChecked(columnList.Items.Count - 1, false);
        }
    }

    internal sealed class QuickConversionForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly TextBox inputTextBox = new TextBox();
        private readonly TextBox resultTextBox = new TextBox();
        private readonly TextBox shiftSequenceTextBox = new TextBox();
        private readonly Button copyButton = new Button();

        internal string ResultText { get { return resultTextBox.Text; } }

        public QuickConversionForm(string defaultShiftSequence)
        {
            Text = "PANDA – Schnellumwandlung";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 590);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout(defaultShiftSequence);
        }

        private void BuildLayout(string defaultShiftSequence)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24, 20, 24, 18),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            Controls.Add(root);

            var heading = new Panel { Dock = DockStyle.Fill };
            heading.Controls.Add(new Label
            {
                Text = "Schnellumwandlung",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            heading.Controls.Add(new Label
            {
                Text = "Text direkt umwandeln – unabhängig von einer importierten CSV-Datei.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 40)
            });
            root.Controls.Add(heading, 0, 0);

            var card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.White,
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(0, 0, 0, 12)
            };
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.Controls.Add(card, 0, 1);

            card.Controls.Add(CreateSectionLabel("Eingabetext"), 0, 0);
            ConfigureTextBox(inputTextBox, false);
            card.Controls.Add(inputTextBox, 0, 1);

            var controls = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 8),
                Margin = new Padding(0)
            };
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172));
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172));
            controls.Controls.Add(new Label
            {
                Text = "Zählfolge",
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 0);
            shiftSequenceTextBox.Text = ShiftSequence.Normalize(defaultShiftSequence, "1");
            shiftSequenceTextBox.TextAlign = HorizontalAlignment.Center;
            shiftSequenceTextBox.MaxLength = 200;
            shiftSequenceTextBox.Dock = DockStyle.Fill;
            shiftSequenceTextBox.Margin = new Padding(0);
            controls.Controls.Add(shiftSequenceTextBox, 1, 0);
            var upButton = CreateActionButton("Hochzählen  +", Color.FromArgb(29, 157, 105));
            upButton.Click += delegate { ApplyConfiguredShift(false); };
            controls.Controls.Add(upButton, 3, 0);
            var downButton = CreateActionButton("Runterzählen  −", Color.FromArgb(230, 91, 84));
            downButton.Click += delegate { ApplyConfiguredShift(true); };
            controls.Controls.Add(downButton, 5, 0);
            card.Controls.Add(controls, 0, 2);

            card.Controls.Add(CreateSectionLabel("Ergebnis"), 0, 3);
            ConfigureTextBox(resultTextBox, true);
            card.Controls.Add(resultTextBox, 0, 4);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 0)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            copyButton.Text = "Ergebnis kopieren";
            copyButton.Enabled = false;
            StyleSecondaryButton(copyButton);
            copyButton.Click += delegate { CopyResult(); };
            var closeButton = new Button
            {
                Text = "Schließen",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Blue,
                ForeColor = Color.White,
                Margin = new Padding(0)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            footer.Controls.Add(copyButton, 1, 0);
            footer.Controls.Add(closeButton, 3, 0);
            root.Controls.Add(footer, 0, 2);
            CancelButton = closeButton;
        }

        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
        }

        private static void ConfigureTextBox(TextBox textBox, bool readOnly)
        {
            textBox.Multiline = true;
            textBox.AcceptsReturn = true;
            textBox.AcceptsTab = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = readOnly;
            textBox.BackColor = readOnly ? Color.FromArgb(248, 250, 253) : Color.White;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Margin = new Padding(0);
        }

        private static Button CreateActionButton(string text, Color background)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = background,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void StyleSecondaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Navy;
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(0);
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
        }

        private void ApplyConfiguredShift(bool countDown)
        {
            List<int> values;
            string errorMessage;
            if (!ShiftSequence.TryParse(shiftSequenceTextBox.Text, out values, out errorMessage))
            {
                MessageBox.Show(this, errorMessage, "Ungültige Zählfolge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                shiftSequenceTextBox.Focus();
                shiftSequenceTextBox.SelectAll();
                return;
            }
            shiftSequenceTextBox.Text = ShiftSequence.Format(values);
            ApplyShift(values.Select(value => countDown ? -value : value).ToList());
        }

        private void ApplyShift(IList<int> amounts)
        {
            resultTextBox.Text = LetterShifter.Shift(inputTextBox.Text, amounts);
            copyButton.Enabled = resultTextBox.TextLength > 0;
        }

        private void CopyResult()
        {
            if (resultTextBox.TextLength == 0)
                return;
            try
            {
                Clipboard.SetText(resultTextBox.Text);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Das Ergebnis konnte nicht in die Zwischenablage kopiert werden.\r\n\r\n" + exception.Message, "Kopieren fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        internal void SetInputText(string value)
        {
            inputTextBox.Text = value ?? string.Empty;
        }

        internal void ApplyShiftForTest(int amount)
        {
            ApplyShift(new[] { amount });
        }

        internal void ApplySequenceForTest(string sequence, bool countDown)
        {
            shiftSequenceTextBox.Text = sequence;
            List<int> values;
            string errorMessage;
            if (!ShiftSequence.TryParse(sequence, out values, out errorMessage))
                throw new InvalidOperationException(errorMessage);
            ApplyShift(values.Select(value => countDown ? -value : value).ToList());
        }

        internal void LoadPreviewData()
        {
            inputTextBox.Text = "Anna Meyer\r\nBüro 12 – Raum Nord";
            ApplyConfiguredShift(false);
        }
    }

    internal sealed class RowFilter
    {
        public int ColumnIndex { get; private set; }
        public List<string> ExactValues { get; private set; }
        public string WildcardPattern { get; private set; }

        public RowFilter(int columnIndex, IEnumerable<string> exactValues, string wildcardPattern)
        {
            ColumnIndex = columnIndex;
            ExactValues = (exactValues ?? Enumerable.Empty<string>())
                .Select(value => value ?? string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            WildcardPattern = (wildcardPattern ?? string.Empty).Trim();
        }

        public bool IsEmpty
        {
            get { return ExactValues.Count == 0 && WildcardPattern.Length == 0; }
        }
    }

    internal static class RowFilterEngine
    {
        internal static List<int> FindHiddenRows(IList<IList<string>> rows, RowFilter filter)
        {
            var hidden = new List<int>();
            if (rows == null || filter == null || filter.IsEmpty || filter.ColumnIndex < 0)
                return hidden;
            var exactValues = new HashSet<string>(filter.ExactValues, StringComparer.OrdinalIgnoreCase);
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                IList<string> row = rows[rowIndex];
                if (row == null || filter.ColumnIndex >= row.Count)
                    continue;
                string value = row[filter.ColumnIndex] ?? string.Empty;
                bool exactMatch = exactValues.Contains(value);
                bool wildcardMatch = filter.WildcardPattern.Length > 0 && WildcardIsMatch(value, filter.WildcardPattern);
                if (exactMatch || wildcardMatch)
                    hidden.Add(rowIndex);
            }
            return hidden;
        }

        internal static bool WildcardIsMatch(string value, string pattern)
        {
            value = value ?? string.Empty;
            pattern = pattern ?? string.Empty;
            int valueIndex = 0;
            int patternIndex = 0;
            int lastStar = -1;
            int retryValueIndex = -1;
            while (valueIndex < value.Length)
            {
                if (patternIndex < pattern.Length
                    && (pattern[patternIndex] == '?' || CharactersEqual(value[valueIndex], pattern[patternIndex])))
                {
                    valueIndex++;
                    patternIndex++;
                }
                else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    lastStar = patternIndex++;
                    retryValueIndex = valueIndex;
                }
                else if (lastStar >= 0)
                {
                    patternIndex = lastStar + 1;
                    valueIndex = ++retryValueIndex;
                }
                else
                {
                    return false;
                }
            }
            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                patternIndex++;
            return patternIndex == pattern.Length;
        }

        private static bool CharactersEqual(char first, char second)
        {
            return char.ToUpperInvariant(first) == char.ToUpperInvariant(second);
        }
    }

    internal sealed class RowFilterValueItem
    {
        public string Value { get; private set; }

        public RowFilterValueItem(string value)
        {
            Value = value ?? string.Empty;
        }

        public override string ToString()
        {
            if (Value.Length == 0)
                return "(leer)";
            return Value.Replace("\r\n", " ↵ ").Replace("\r", " ↵ ").Replace("\n", " ↵ ");
        }
    }

    internal sealed class RowFilterForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly IList<string> headers;
        private readonly IList<IList<string>> rows;
        private readonly RowFilter initialFilter;
        private readonly ComboBox columnComboBox = new ComboBox();
        private readonly CheckedListBox valuesList = new CheckedListBox();
        private readonly TextBox wildcardTextBox = new TextBox();
        private readonly Label selectionLabel = new Label();
        private readonly Button applyButton = new Button();
        private readonly Button clearFilterButton = new Button();

        public RowFilter SelectedFilter { get; private set; }

        public RowFilterForm(IList<string> headers, IList<IList<string>> rows, RowFilter initialFilter)
        {
            this.headers = headers ?? new List<string>();
            this.rows = rows ?? new List<IList<string>>();
            this.initialFilter = initialFilter;
            Text = "PANDA – Zeilen filtern";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 650);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            BuildLayout();
            LoadColumns();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24, 20, 24, 18),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            Controls.Add(root);

            var heading = new Panel { Dock = DockStyle.Fill };
            heading.Controls.Add(new Label
            {
                Text = "Zeilen ausblenden",
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Navy,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            heading.Controls.Add(new Label
            {
                Text = "Wähle Spaltenwerte oder ergänze ein Wildcard-Muster.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 40)
            });
            root.Controls.Add(heading, 0, 0);

            var card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Color.White,
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(0, 0, 0, 12)
            };
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.Controls.Add(card, 0, 1);

            card.Controls.Add(new Label
            {
                Text = "Treffer werden nur ausgeblendet. Die zugrunde liegenden CSV-Daten bleiben erhalten.",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 0);
            card.Controls.Add(CreateSectionLabel("Spalte"), 0, 1);
            columnComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            columnComboBox.Dock = DockStyle.Fill;
            columnComboBox.Margin = new Padding(0, 2, 0, 6);
            columnComboBox.SelectedIndexChanged += delegate { ReloadColumnValues(); };
            card.Controls.Add(columnComboBox, 0, 2);

            var valuesHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0) };
            valuesHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            valuesHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            valuesHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            valuesHeader.Controls.Add(CreateSectionLabel("Diese vorhandenen Werte ausblenden"), 0, 0);
            var allButton = CreateSmallButton("Alle wählen");
            allButton.Click += delegate { SetAllValuesChecked(true); };
            valuesHeader.Controls.Add(allButton, 1, 0);
            var noneButton = CreateSmallButton("Keine");
            noneButton.Click += delegate { SetAllValuesChecked(false); };
            valuesHeader.Controls.Add(noneButton, 2, 0);
            card.Controls.Add(valuesHeader, 0, 3);

            valuesList.CheckOnClick = true;
            valuesList.Dock = DockStyle.Fill;
            valuesList.BorderStyle = BorderStyle.FixedSingle;
            valuesList.HorizontalScrollbar = true;
            valuesList.IntegralHeight = false;
            valuesList.ItemCheck += delegate
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateSelectionState));
            };
            card.Controls.Add(valuesList, 0, 4);

            var wildcardHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            wildcardHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            wildcardHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            wildcardHeader.Controls.Add(CreateSectionLabel("Zusätzliches Wildcard-Muster"), 0, 0);
            var helpButton = CreateSmallButton("?");
            helpButton.Font = new Font("Segoe UI Semibold", 10F);
            helpButton.Click += delegate { ShowWildcardHelp(); };
            wildcardHeader.Controls.Add(helpButton, 1, 0);
            card.Controls.Add(wildcardHeader, 0, 5);

            wildcardTextBox.Dock = DockStyle.Fill;
            wildcardTextBox.MaxLength = 200;
            wildcardTextBox.Margin = new Padding(0, 2, 0, 6);
            wildcardTextBox.TextChanged += delegate { UpdateSelectionState(); };
            card.Controls.Add(wildcardTextBox, 0, 6);
            selectionLabel.ForeColor = Muted;
            selectionLabel.AutoEllipsis = true;
            selectionLabel.Dock = DockStyle.Fill;
            selectionLabel.TextAlign = ContentAlignment.MiddleLeft;
            card.Controls.Add(selectionLabel, 0, 7);

            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1, Padding = new Padding(0, 10, 0, 0) };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            clearFilterButton.Text = "Filter aufheben";
            clearFilterButton.Enabled = initialFilter != null && !initialFilter.IsEmpty;
            StyleSecondaryButton(clearFilterButton);
            clearFilterButton.Click += delegate
            {
                SelectedFilter = null;
                DialogResult = DialogResult.OK;
                Close();
            };
            var cancelButton = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel };
            StyleSecondaryButton(cancelButton);
            applyButton.Text = "Anwenden";
            applyButton.Dock = DockStyle.Fill;
            applyButton.FlatStyle = FlatStyle.Flat;
            applyButton.BackColor = Blue;
            applyButton.ForeColor = Color.White;
            applyButton.Margin = new Padding(0);
            applyButton.FlatAppearance.BorderSize = 0;
            applyButton.Click += delegate { ApplyFilter(); };
            footer.Controls.Add(clearFilterButton, 1, 0);
            footer.Controls.Add(cancelButton, 3, 0);
            footer.Controls.Add(applyButton, 5, 0);
            root.Controls.Add(footer, 0, 2);
            CancelButton = cancelButton;
            AcceptButton = applyButton;
        }

        private void LoadColumns()
        {
            columnComboBox.Items.Clear();
            foreach (string header in headers)
                columnComboBox.Items.Add(header);
            if (columnComboBox.Items.Count == 0)
                return;
            int initialColumn = initialFilter == null ? 0 : initialFilter.ColumnIndex;
            columnComboBox.SelectedIndex = Math.Max(0, Math.Min(columnComboBox.Items.Count - 1, initialColumn));
            wildcardTextBox.Text = initialFilter == null ? string.Empty : initialFilter.WildcardPattern;
            ReloadColumnValues();
        }

        private void ReloadColumnValues()
        {
            int columnIndex = columnComboBox.SelectedIndex;
            if (columnIndex < 0)
                return;
            valuesList.BeginUpdate();
            valuesList.Items.Clear();
            var distinctValues = rows
                .Where(row => row != null && columnIndex < row.Count)
                .Select(row => row[columnIndex] ?? string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            foreach (string value in distinctValues)
            {
                bool isChecked = initialFilter != null
                    && initialFilter.ColumnIndex == columnIndex
                    && initialFilter.ExactValues.Contains(value, StringComparer.OrdinalIgnoreCase);
                valuesList.Items.Add(new RowFilterValueItem(value), isChecked);
            }
            valuesList.EndUpdate();
            UpdateSelectionState();
        }

        private List<string> GetCheckedValues()
        {
            return valuesList.CheckedItems
                .Cast<RowFilterValueItem>()
                .Select(item => item.Value)
                .ToList();
        }

        private void SetAllValuesChecked(bool isChecked)
        {
            for (int index = 0; index < valuesList.Items.Count; index++)
                valuesList.SetItemChecked(index, isChecked);
            BeginInvoke(new Action(UpdateSelectionState));
        }

        private void UpdateSelectionState()
        {
            int selectedCount = valuesList.CheckedItems.Count;
            bool hasWildcard = !string.IsNullOrWhiteSpace(wildcardTextBox.Text);
            selectionLabel.Text = selectedCount + " von " + valuesList.Items.Count + " Werten ausgewählt"
                + (hasWildcard ? "  •  Wildcard aktiv" : string.Empty);
            applyButton.Enabled = columnComboBox.SelectedIndex >= 0 && (selectedCount > 0 || hasWildcard);
        }

        private void ApplyFilter()
        {
            var filter = new RowFilter(columnComboBox.SelectedIndex, GetCheckedValues(), wildcardTextBox.Text);
            if (filter.IsEmpty)
                return;
            SelectedFilter = filter;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowWildcardHelp()
        {
            MessageBox.Show(this,
                "Wildcards werden ohne Beachtung der Groß- und Kleinschreibung auf den vollständigen Zellinhalt angewendet.\r\n\r\n"
                + "*  ersetzt beliebig viele Zeichen (auch kein Zeichen)\r\n"
                + "?  ersetzt genau ein Zeichen\r\n\r\n"
                + "Beispiele:\r\n"
                + "M* blendet Meyer, München und Mainz aus.\r\n"
                + "*Berlin* findet Berlin an beliebiger Stelle.\r\n"
                + "B?b findet zum Beispiel Bob oder Bab.",
                "Hilfe zu Wildcards",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
        }

        private Button CreateSmallButton(string text)
        {
            var button = new Button { Text = text };
            StyleSecondaryButton(button);
            button.Margin = new Padding(4, 3, 0, 3);
            return button;
        }

        private void StyleSecondaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Navy;
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(0);
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
        }
    }

    internal sealed class ShiftModeSelector : Control
    {
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Border = Color.FromArgb(206, 216, 230);
        private bool advanced;

        internal event EventHandler SimpleSelected;
        internal event EventHandler AdvancedSelected;

        internal bool Advanced
        {
            get { return advanced; }
            set
            {
                if (advanced == value)
                    return;
                advanced = value;
                Invalidate();
            }
        }

        internal ShiftModeSelector()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        protected override void OnPaint(PaintEventArgs eventArgs)
        {
            base.OnPaint(eventArgs);
            int firstWidth = ClientSize.Width / 2;
            var simpleBounds = new Rectangle(0, 0, firstWidth, ClientSize.Height);
            var advancedBounds = new Rectangle(firstWidth, 0, ClientSize.Width - firstWidth, ClientSize.Height);
            DrawSegment(eventArgs.Graphics, simpleBounds, "Einfach", !advanced);
            DrawSegment(eventArgs.Graphics, advancedBounds, "Erweitert", advanced);
            if (Focused)
                ControlPaint.DrawFocusRectangle(eventArgs.Graphics, advanced ? advancedBounds : simpleBounds);
        }

        private void DrawSegment(Graphics graphics, Rectangle bounds, string text, bool selected)
        {
            using (var brush = new SolidBrush(selected ? Blue : Background))
                graphics.FillRectangle(brush, bounds);
            ControlPaint.DrawBorder(graphics, bounds, selected ? Blue : Border, ButtonBorderStyle.Solid);
            TextRenderer.DrawText(graphics, text, Font, bounds, selected ? Color.White : Navy,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        protected override void OnMouseUp(MouseEventArgs eventArgs)
        {
            base.OnMouseUp(eventArgs);
            SelectMode(eventArgs.X >= ClientSize.Width / 2);
        }

        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            base.OnKeyDown(eventArgs);
            if (eventArgs.KeyCode == Keys.Left)
            {
                SelectMode(false);
                eventArgs.Handled = true;
            }
            else if (eventArgs.KeyCode == Keys.Right)
            {
                SelectMode(true);
                eventArgs.Handled = true;
            }
        }

        private void SelectMode(bool selectAdvanced)
        {
            EventHandler handler = selectAdvanced ? AdvancedSelected : SimpleSelected;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color PaleBlue = Color.FromArgb(236, 243, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);

        private readonly DataGridView originalGrid = new DataGridView();
        private readonly DataGridView resultGrid = new DataGridView();
        private readonly ComboBox scopeComboBox = new ComboBox();
        private readonly NumericUpDown simpleShiftNumeric = new NumericUpDown();
        private readonly NumericUpDown[] advancedShiftNumerics =
        {
            new NumericUpDown(), new NumericUpDown(), new NumericUpDown(), new NumericUpDown(), new NumericUpDown()
        };
        private readonly TableLayoutPanel shiftValueHost = new TableLayoutPanel();
        private readonly TableLayoutPanel simpleShiftPanel = new TableLayoutPanel();
        private readonly TableLayoutPanel advancedShiftPanel = new TableLayoutPanel();
        private readonly ShiftModeSelector modeSelectorHost = new ShiftModeSelector();
        private readonly Label shiftValuesCaptionLabel = new Label();
        private readonly Label sequenceRestartHint = new Label();
        private readonly Label statusLabel = new Label();
        private readonly Label fileLabel = new Label();
        private readonly Button exportButton = new Button();
        private readonly Button resetButton = new Button();
        private readonly Button settingsButton = new Button();
        private readonly Button templatesButton = new Button();
        private readonly Button updateButton = new Button();
        private readonly Button quickConversionButton = new Button();
        private readonly Button clearButton = new Button();
        private readonly Button filterButton = new Button();

        private CsvDocument document;
        private string importedPath;
        private string baseFileLabelText = string.Empty;
        private RowFilter activeRowFilter;
        private AppSettings appSettings = AppSettings.Load();
        private readonly List<SelectionTemplate> selectionTemplates = SelectionTemplateStore.Load();
        private readonly bool automaticUpdateChecksEnabled;
        private string activeInterfaceStyle;
        private Control layoutRoot;
        private bool advancedShiftMode;
        private bool updateCheckInProgress;

        public MainForm()
            : this(true, null)
        {
        }

        internal MainForm(bool automaticUpdateChecksEnabled)
            : this(automaticUpdateChecksEnabled, null)
        {
        }

        internal MainForm(bool automaticUpdateChecksEnabled, string interfaceStyleOverride)
        {
            this.automaticUpdateChecksEnabled = automaticUpdateChecksEnabled;
            if (!string.IsNullOrEmpty(interfaceStyleOverride))
                appSettings.InterfaceStyle = string.Equals(interfaceStyleOverride, "Classic", StringComparison.OrdinalIgnoreCase) ? "Classic" : "Metro";
            activeInterfaceStyle = string.Equals(appSettings.InterfaceStyle, "Classic", StringComparison.OrdinalIgnoreCase) ? "Classic" : "Metro";
            Text = "PANDA – Pseudonymisierung alphanumerischer Nutzdaten";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 680);
            Size = new Size(1320, 820);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

            InitializeSharedControls();
            WireSharedControlEvents();
            BuildLayout();
            ConfigureGrid(originalGrid, true);
            ConfigureGrid(resultGrid, false);
            SetDocumentControlsEnabled(false);
            if (automaticUpdateChecksEnabled)
                Shown += delegate { InitializeUpdateCheck(); };
        }

        private void BuildLayout()
        {
            string previousFileText = fileLabel.Text;
            string previousStatusText = statusLabel.Text;
            Color previousStatusColor = statusLabel.ForeColor;
            string previousUpdateText = updateButton.Text;
            Control previousRoot = layoutRoot;

            SuspendLayout();
            if (string.Equals(activeInterfaceStyle, "Classic", StringComparison.OrdinalIgnoreCase))
                BuildClassicLayout();
            else
                BuildMetroLayout();

            if (previousRoot != null && previousRoot != layoutRoot)
            {
                Controls.Remove(previousRoot);
                previousRoot.Dispose();
            }
            if (!string.IsNullOrEmpty(previousFileText))
                fileLabel.Text = previousFileText;
            if (!string.IsNullOrEmpty(previousStatusText))
            {
                statusLabel.Text = previousStatusText;
                statusLabel.ForeColor = previousStatusColor;
            }
            if (!string.IsNullOrEmpty(previousUpdateText))
                updateButton.Text = previousUpdateText;
            filterButton.Text = activeRowFilter != null && !activeRowFilter.IsEmpty
                ? "Filter aktiv"
                : (IsClassicDesign ? "Filter" : "Zeilen filtern");
            RefreshShiftModeUi();
            SetDocumentControlsEnabled(document != null);
            ResumeLayout(true);
        }

        private void InitializeSharedControls()
        {
            scopeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            scopeComboBox.Items.AddRange(new object[] { "Markierte Zellen", "Aktuelle Zelle", "Alle Einträge" });
            scopeComboBox.SelectedIndex = 0;

            ConfigureShiftNumeric(simpleShiftNumeric);
            simpleShiftNumeric.Width = 88;
            foreach (NumericUpDown numeric in advancedShiftNumerics)
                ConfigureShiftNumeric(numeric);

            simpleShiftPanel.Dock = DockStyle.Fill;
            simpleShiftPanel.ColumnCount = 2;
            simpleShiftPanel.RowCount = 1;
            simpleShiftPanel.Margin = new Padding(0);
            simpleShiftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            simpleShiftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            simpleShiftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            simpleShiftPanel.Controls.Add(simpleShiftNumeric, 0, 0);

            advancedShiftPanel.Dock = DockStyle.Fill;
            advancedShiftPanel.ColumnCount = 10;
            advancedShiftPanel.RowCount = 1;
            advancedShiftPanel.Margin = new Padding(0);
            advancedShiftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int index = 0; index < 5; index++)
            {
                int column = index * 2;
                advancedShiftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
                advancedShiftPanel.Controls.Add(advancedShiftNumerics[index], column, 0);
                if (index < 4)
                {
                    advancedShiftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18));
                    advancedShiftPanel.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(150, 160, 174),
                        Size = new Size(8, 2),
                        Anchor = AnchorStyles.None,
                        Margin = new Padding(0)
                    }, column + 1, 0);
                }
            }
            advancedShiftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            shiftValueHost.Dock = DockStyle.Fill;
            shiftValueHost.ColumnCount = 1;
            shiftValueHost.RowCount = 1;
            shiftValueHost.Margin = new Padding(0);
            shiftValueHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shiftValueHost.Controls.Add(simpleShiftPanel, 0, 0);
            shiftValueHost.Controls.Add(advancedShiftPanel, 0, 0);

            modeSelectorHost.Dock = DockStyle.Fill;
            modeSelectorHost.Margin = new Padding(0);

            shiftValuesCaptionLabel.ForeColor = Muted;
            shiftValuesCaptionLabel.Font = new Font("Segoe UI Semibold", 7.5F);
            shiftValuesCaptionLabel.Dock = DockStyle.Fill;
            shiftValuesCaptionLabel.TextAlign = ContentAlignment.MiddleLeft;

            sequenceRestartHint.ForeColor = Muted;
            sequenceRestartHint.Font = new Font("Segoe UI", 8F);
            sequenceRestartHint.Dock = DockStyle.Fill;
            sequenceRestartHint.TextAlign = ContentAlignment.MiddleCenter;
            sequenceRestartHint.AutoEllipsis = true;
            sequenceRestartHint.Margin = new Padding(8, 0, 0, 0);

            LoadShiftConfiguration(appSettings.DefaultShiftSequence);
        }

        private void ConfigureShiftNumeric(NumericUpDown numeric)
        {
            numeric.Minimum = 1;
            numeric.Maximum = 25;
            numeric.Value = 1;
            numeric.TextAlign = HorizontalAlignment.Center;
            numeric.Dock = DockStyle.None;
            numeric.Anchor = AnchorStyles.None;
            numeric.Width = 54;
            numeric.Margin = new Padding(0);
            numeric.Font = new Font("Segoe UI Semibold", 9F);
        }

        private void WireSharedControlEvents()
        {
            clearButton.Click += delegate { ClearCurrentCsv(); };
            filterButton.Click += delegate { OpenRowFilter(); };
            quickConversionButton.Click += delegate { OpenQuickConversion(); };
            templatesButton.Click += delegate { OpenSelectionTemplates(); };
            settingsButton.Click += delegate { OpenSettings(); };
            updateButton.Click += delegate { CheckForUpdates(true); };
            resetButton.Click += delegate { ResetResults(); };
            exportButton.Click += delegate { ExportCsv(); };
            modeSelectorHost.SimpleSelected += delegate { SetAdvancedShiftMode(false); };
            modeSelectorHost.AdvancedSelected += delegate { SetAdvancedShiftMode(true); };
        }

        private void BuildMetroLayout()
        {
            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(241, 244, 248),
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(shell);
            layoutRoot = shell;

            var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 37, 54), Margin = new Padding(0) };
            sidebar.Controls.Add(new Label
            {
                Text = "PANDA",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 22F),
                AutoSize = true,
                Location = new Point(22, 24)
            });
            sidebar.Controls.Add(new Label
            {
                Text = "PSEUDONYMISIERUNG",
                ForeColor = Color.FromArgb(139, 158, 184),
                Font = new Font("Segoe UI Semibold", 8F),
                AutoSize = true,
                Location = new Point(24, 68)
            });
            var accent = new Panel { BackColor = Blue, Location = new Point(0, 20), Size = new Size(5, 62) };
            sidebar.Controls.Add(accent);

            var navigation = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Location = new Point(18, 116),
                Size = new Size(184, 500),
                BackColor = sidebar.BackColor,
                Margin = new Padding(0)
            };
            var importButton = CreateMetroNavigationButton("CSV importieren", true);
            importButton.Click += delegate { ImportCsv(); };
            navigation.Controls.Add(importButton);
            ConfigureMetroNavigationButton(clearButton, "Aktuelle CSV leeren");
            navigation.Controls.Add(clearButton);
            ConfigureMetroNavigationButton(filterButton, "Zeilen filtern");
            navigation.Controls.Add(filterButton);
            ConfigureMetroNavigationButton(quickConversionButton, "Schnellumwandlung");
            navigation.Controls.Add(quickConversionButton);
            ConfigureMetroNavigationButton(templatesButton, "Auswahlvorlagen");
            navigation.Controls.Add(templatesButton);
            ConfigureMetroNavigationButton(settingsButton, "Einstellungen");
            navigation.Controls.Add(settingsButton);
            sidebar.Controls.Add(navigation);
            var designFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                BackColor = sidebar.BackColor
            };
            designFooter.Controls.Add(new Label
            {
                Text = "DESIGN  •  METRO",
                ForeColor = Color.FromArgb(115, 137, 165),
                Font = new Font("Segoe UI Semibold", 8F),
                AutoSize = true,
                Location = new Point(24, 12)
            });
            sidebar.Controls.Add(designFooter);
            shell.Controls.Add(sidebar, 0, 0);

            var workspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(22, 18, 22, 14),
                BackColor = Color.FromArgb(241, 244, 248),
                Margin = new Padding(0)
            };
            workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            shell.Controls.Add(workspace, 1, 0);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = workspace.BackColor };
            header.Controls.Add(new Label
            {
                Text = "Daten umwandeln",
                ForeColor = Navy,
                Font = new Font("Segoe UI Semibold", 20F),
                AutoSize = true,
                Location = new Point(0, 0)
            });
            fileLabel.Text = "Noch keine CSV geladen";
            fileLabel.ForeColor = Muted;
            fileLabel.AutoEllipsis = true;
            fileLabel.Dock = DockStyle.None;
            fileLabel.Size = new Size(700, 22);
            fileLabel.Location = new Point(2, 43);
            header.Controls.Add(fileLabel);
            updateButton.Text = "Updates prüfen";
            StyleSecondaryButton(updateButton);
            updateButton.Dock = DockStyle.None;
            updateButton.Size = new Size(158, 36);
            updateButton.Location = new Point(0, 4);
            updateButton.Margin = new Padding(0);
            var updateHost = new Panel
            {
                Dock = DockStyle.Right,
                Width = 158,
                BackColor = workspace.BackColor
            };
            updateHost.Controls.Add(updateButton);
            header.Controls.Add(updateHost);
            workspace.Controls.Add(header, 0, 0);

            var commandBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 0, 0, 12)
            };
            for (int column = 0; column < 4; column++)
                commandBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            commandBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            commandBar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            commandBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            commandBar.Controls.Add(CreateMetroInputPanel("BEREICH", scopeComboBox), 0, 0);
            commandBar.Controls.Add(CreateMetroInputPanel("VERSCHLÜSSELUNG", modeSelectorHost), 1, 0);
            Control metroValuesPanel = CreateShiftValuesInputPanel();
            commandBar.Controls.Add(metroValuesPanel, 2, 0);
            commandBar.SetColumnSpan(metroValuesPanel, 2);
            var upButton = CreateButton("Hochzählen  +", Color.FromArgb(20, 153, 102), Color.White);
            upButton.Click += delegate { ApplyConfiguredShift(false); };
            commandBar.Controls.Add(upButton, 0, 1);
            var downButton = CreateButton("Runterzählen  −", Color.FromArgb(221, 78, 73), Color.White);
            downButton.Click += delegate { ApplyConfiguredShift(true); };
            commandBar.Controls.Add(downButton, 1, 1);
            resetButton.Text = "Zurücksetzen";
            StyleSecondaryButton(resetButton);
            commandBar.Controls.Add(resetButton, 2, 1);
            exportButton.Text = "CSV exportieren";
            StyleSecondaryButton(exportButton);
            commandBar.Controls.Add(exportButton, 3, 1);
            if (sequenceRestartHint.Parent != null)
                sequenceRestartHint.Parent.Controls.Remove(sequenceRestartHint);
            commandBar.SetColumnSpan(sequenceRestartHint, 1);
            commandBar.Controls.Add(sequenceRestartHint, 4, 1);
            workspace.Controls.Add(commandBar, 0, 1);

            var grids = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = workspace.BackColor,
                Margin = new Padding(0)
            };
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grids.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            grids.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grids.Controls.Add(CreateGridHeader("ORIGINAL", "Importierte CSV-Werte", false), 0, 0);
            grids.Controls.Add(CreateGridHeader("ERGEBNIS", "Veränderte Werte", true), 1, 0);
            originalGrid.Margin = new Padding(0, 0, 7, 0);
            resultGrid.Margin = new Padding(7, 0, 0, 0);
            grids.Controls.Add(originalGrid, 0, 1);
            grids.Controls.Add(resultGrid, 1, 1);
            workspace.Controls.Add(grids, 0, 2);

            var statusPanel = new Panel { Dock = DockStyle.Fill, BackColor = workspace.BackColor };
            statusLabel.Text = "Bereit – bitte eine CSV-Datei importieren.";
            statusLabel.ForeColor = Muted;
            statusLabel.AutoSize = false;
            statusLabel.AutoEllipsis = true;
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusPanel.Controls.Add(statusLabel);
            workspace.Controls.Add(statusPanel, 0, 3);
        }

        private void BuildClassicLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20, 18, 20, 16),
                BackColor = Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 164));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            Controls.Add(root);
            layoutRoot = root;

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Background };
            var title = new Label
            {
                Text = "PANDA",
                ForeColor = Navy,
                Font = new Font("Segoe UI Semibold", 19F),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var subtitle = new Label
            {
                Text = "Pseudonymisierung alphanumerischer Nutzdaten durch Alphabetverschiebung",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 39)
            };
            var headerActions = new Panel
            {
                Dock = DockStyle.Right,
                Width = 382,
                BackColor = Background
            };
            quickConversionButton.Text = "Schnellumwandlung";
            StyleSecondaryButton(quickConversionButton);
            quickConversionButton.Dock = DockStyle.None;
            quickConversionButton.Size = new Size(176, 34);
            quickConversionButton.Location = new Point(0, 5);
            headerActions.Controls.Add(quickConversionButton);
            updateButton.Text = "Updates prüfen";
            StyleSecondaryButton(updateButton);
            updateButton.Dock = DockStyle.None;
            updateButton.Size = new Size(176, 34);
            updateButton.Location = new Point(190, 5);
            headerActions.Controls.Add(updateButton);
            header.Controls.Add(headerActions);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            root.Controls.Add(header, 0, 0);

            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 3,
                Padding = new Padding(12, 10, 12, 8),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };
            float[] classicColumnWeights = { 15F, 15F, 22F, 16F, 16F, 16F };
            foreach (float weight in classicColumnWeights)
                toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, weight));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            root.Controls.Add(toolbar, 0, 1);

            var importButton = CreateButton("Importieren", Blue, Color.White);
            importButton.Click += delegate { ImportCsv(); };
            importButton.Margin = new Padding(4, 10, 4, 10);
            toolbar.Controls.Add(importButton, 0, 0);
            toolbar.Controls.Add(CreateMetroInputPanel("BEREICH", scopeComboBox), 1, 0);
            toolbar.Controls.Add(CreateMetroInputPanel("VERSCHLÜSSELUNG", modeSelectorHost), 2, 0);
            Control classicValuesPanel = CreateShiftValuesInputPanel();
            toolbar.Controls.Add(classicValuesPanel, 3, 0);
            toolbar.SetColumnSpan(classicValuesPanel, 3);

            var upButton = CreateButton("Hochzählen  +", Color.FromArgb(29, 157, 105), Color.White);
            upButton.Click += delegate { ApplyConfiguredShift(false); };
            toolbar.Controls.Add(upButton, 0, 1);
            var downButton = CreateButton("Runterzählen  −", Color.FromArgb(230, 91, 84), Color.White);
            downButton.Click += delegate { ApplyConfiguredShift(true); };
            toolbar.Controls.Add(downButton, 1, 1);
            resetButton.Text = "Zurücksetzen";
            StyleSecondaryButton(resetButton);
            toolbar.Controls.Add(resetButton, 2, 1);
            exportButton.Text = "CSV exportieren";
            StyleSecondaryButton(exportButton);
            toolbar.Controls.Add(exportButton, 3, 1);
            settingsButton.Text = "Einstellungen";
            StyleSecondaryButton(settingsButton);
            toolbar.Controls.Add(settingsButton, 4, 1);
            templatesButton.Text = "Vorlagen";
            StyleSecondaryButton(templatesButton);
            toolbar.Controls.Add(templatesButton, 5, 1);
            if (sequenceRestartHint.Parent != null)
                sequenceRestartHint.Parent.Controls.Remove(sequenceRestartHint);
            toolbar.SetColumnSpan(sequenceRestartHint, 1);
            toolbar.Controls.Add(sequenceRestartHint, 6, 1);

            clearButton.Text = "Leeren";
            StyleSecondaryButton(clearButton);
            toolbar.Controls.Add(clearButton, 0, 2);
            filterButton.Text = "Filter";
            StyleSecondaryButton(filterButton);
            toolbar.Controls.Add(filterButton, 1, 2);
            fileLabel.Text = "Noch keine CSV geladen";
            fileLabel.ForeColor = Muted;
            fileLabel.AutoEllipsis = true;
            fileLabel.Dock = DockStyle.Fill;
            fileLabel.TextAlign = ContentAlignment.MiddleLeft;
            fileLabel.Margin = new Padding(8, 0, 8, 0);
            toolbar.Controls.Add(fileLabel, 2, 2);
            toolbar.SetColumnSpan(fileLabel, 3);
            var hint = new Label
            {
                Text = "Tipp: Spaltenkopf anklicken; mit Strg weitere Spalten ergänzen.",
                ForeColor = Muted,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            toolbar.Controls.Add(hint, 5, 2);
            toolbar.SetColumnSpan(hint, 2);

            var grids = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Background,
                Margin = new Padding(0)
            };
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grids.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            grids.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(grids, 0, 2);

            grids.Controls.Add(CreateGridHeader("ORIGINAL", "Importierte CSV-Werte", false), 0, 0);
            grids.Controls.Add(CreateGridHeader("ERGEBNIS", "Veränderte Werte", true), 1, 0);

            originalGrid.Margin = new Padding(0, 0, 7, 0);
            resultGrid.Margin = new Padding(7, 0, 0, 0);
            grids.Controls.Add(originalGrid, 0, 1);
            grids.Controls.Add(resultGrid, 1, 1);

            var statusPanel = new Panel { Dock = DockStyle.Fill, BackColor = Background };
            statusLabel.Text = "Bereit – bitte eine CSV-Datei importieren.";
            statusLabel.ForeColor = Muted;
            statusLabel.AutoSize = true;
            statusLabel.Dock = DockStyle.None;
            statusLabel.Location = new Point(2, 12);
            statusPanel.Controls.Add(statusLabel);
            root.Controls.Add(statusPanel, 0, 3);
        }

        private Panel CreateGridHeader(string eyebrow, string caption, bool result)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = result ? PaleBlue : Color.White,
                Margin = result ? new Padding(7, 0, 0, 0) : new Padding(0, 0, 7, 0)
            };
            var title = new Label
            {
                Text = eyebrow + "   " + caption,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = result ? Blue : Navy,
                AutoSize = true,
                Location = new Point(12, 11)
            };
            panel.Controls.Add(title);
            return panel;
        }

        private Button CreateButton(string text, Color background, Color foreground)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = background,
                ForeColor = foreground,
                Cursor = Cursors.Hand,
                Margin = new Padding(4, 2, 4, 2),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = false,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void StyleSecondaryButton(Button button)
        {
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Navy;
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(4, 2, 4, 2);
            button.Padding = new Padding(0);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.UseCompatibleTextRendering = false;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
        }

        private Button CreateMetroNavigationButton(string text, bool accent)
        {
            var button = new Button();
            ConfigureMetroNavigationButton(button, text);
            if (accent)
            {
                button.BackColor = Blue;
                button.FlatAppearance.BorderSize = 0;
            }
            return button;
        }

        private void ConfigureMetroNavigationButton(Button button, string text)
        {
            button.Text = text;
            button.Size = new Size(184, 44);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(34, 49, 69);
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 0, 0);
            button.Margin = new Padding(0, 0, 0, 8);
            button.UseCompatibleTextRendering = false;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = Color.FromArgb(56, 74, 98);
        }

        private Control CreateMetroInputPanel(string caption, Control control)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(4, 0, 4, 0)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.Controls.Add(new Label
            {
                Text = caption,
                ForeColor = Muted,
                Font = new Font("Segoe UI Semibold", 7.5F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0);
            panel.Controls.Add(control, 0, 1);
            return panel;
        }

        private Control CreateShiftValuesInputPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(4, 0, 4, 0)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            if (shiftValuesCaptionLabel.Parent != null)
                shiftValuesCaptionLabel.Parent.Controls.Remove(shiftValuesCaptionLabel);
            panel.Controls.Add(shiftValuesCaptionLabel, 0, 0);
            shiftValueHost.Dock = DockStyle.Fill;
            shiftValueHost.Margin = new Padding(0);
            panel.Controls.Add(shiftValueHost, 0, 1);
            return panel;
        }

        private void LoadShiftConfiguration(string sequenceText)
        {
            string normalized = ShiftSequence.NormalizeForMainMode(sequenceText);
            List<int> values;
            string errorMessage;
            ShiftSequence.TryParse(normalized, out values, out errorMessage);
            advancedShiftMode = values.Count > 1;
            List<int> advancedValues = ShiftSequence.ToFiveValues(values);
            simpleShiftNumeric.Value = advancedValues[0];
            for (int index = 0; index < advancedShiftNumerics.Length; index++)
                advancedShiftNumerics[index].Value = advancedValues[index];
            appSettings.DefaultShiftSequence = normalized;
            RefreshShiftModeUi();
        }

        private void SetAdvancedShiftMode(bool advanced)
        {
            if (advancedShiftMode == advanced)
                return;
            if (advanced)
                advancedShiftNumerics[0].Value = simpleShiftNumeric.Value;
            else
                simpleShiftNumeric.Value = advancedShiftNumerics[0].Value;
            advancedShiftMode = advanced;
            RefreshShiftModeUi();
            statusLabel.Text = advanced
                ? "Erweiterte Verschlüsselung aktiv: Die Chiffre verwendet fünf Werte nacheinander."
                : "Einfache Verschlüsselung aktiv: ein Zählwert wird durchgehend verwendet.";
            statusLabel.ForeColor = Muted;
        }

        private void RefreshShiftModeUi()
        {
            simpleShiftPanel.Visible = !advancedShiftMode;
            advancedShiftPanel.Visible = advancedShiftMode;
            if (advancedShiftMode)
                advancedShiftPanel.BringToFront();
            else
                simpleShiftPanel.BringToFront();
            modeSelectorHost.Advanced = advancedShiftMode;
            shiftValuesCaptionLabel.Text = advancedShiftMode ? "CHIFFRE" : "ZÄHLWERT";
            sequenceRestartHint.Text = advancedShiftMode
                ? "Zählfolge beginnt je Wert neu"
                : "Ein Zählwert gilt für alle Buchstaben";
            sequenceRestartHint.Visible = true;
        }

        private List<int> GetConfiguredShiftValues()
        {
            if (!advancedShiftMode)
                return new List<int> { (int)simpleShiftNumeric.Value };
            return advancedShiftNumerics.Select(numeric => (int)numeric.Value).ToList();
        }

        private bool IsClassicDesign
        {
            get { return string.Equals(activeInterfaceStyle, "Classic", StringComparison.OrdinalIgnoreCase); }
        }

        private void OpenSettings()
        {
            using (var dialog = new SettingsForm(appSettings))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                string previousInterfaceStyle = appSettings.InterfaceStyle;
                appSettings.DefaultShiftSequence = dialog.DefaultShiftSequence;
                appSettings.InterfaceStyle = dialog.InterfaceStyle;
                appSettings.ConfirmBeforeShift = dialog.ConfirmBeforeShift;
                appSettings.CheckForUpdates = dialog.CheckForUpdates;
                appSettings.AskForUpdateCheckOnStart = dialog.AskForUpdateCheckOnStart;
                try
                {
                    appSettings.Save();
                    bool designChanged = !string.Equals(previousInterfaceStyle, appSettings.InterfaceStyle, StringComparison.OrdinalIgnoreCase);
                    LoadShiftConfiguration(appSettings.DefaultShiftSequence);
                    if (designChanged)
                        ApplyInterfaceStyle(appSettings.InterfaceStyle);
                    statusLabel.Text = "Einstellungen gespeichert. Standard-Zählfolge: " + appSettings.DefaultShiftSequence + "."
                        + (designChanged ? " Das Design wurde sofort gewechselt." : string.Empty);
                    statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, "Die Einstellungen konnten nicht gespeichert werden.\r\n\r\n" + exception.Message, "Speichern fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenQuickConversion()
        {
            using (var dialog = new QuickConversionForm(ShiftSequence.Format(GetConfiguredShiftValues())))
                dialog.ShowDialog(this);
        }

        private void ApplyInterfaceStyle(string interfaceStyle)
        {
            activeInterfaceStyle = string.Equals(interfaceStyle, "Classic", StringComparison.OrdinalIgnoreCase) ? "Classic" : "Metro";
            appSettings.InterfaceStyle = activeInterfaceStyle;
            BuildLayout();
        }

        private void InitializeUpdateCheck()
        {
            RefreshUpdateButtonFromCache();
            if (ShouldAskForUpdateCheckOnStart(appSettings))
            {
                DialogResult choice = MessageBox.Show(this,
                    "Möchtest du jetzt prüfen, ob eine neue PANDA-Version verfügbar ist?\r\n\r\n"
                    + "PANDA lädt oder startet dabei keine Datei automatisch.",
                    "Nach Updates suchen?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (choice == DialogResult.Yes)
                    CheckForUpdates(true);
                return;
            }
            if (ShouldCheckForUpdatesOnStart(appSettings, DateTime.UtcNow))
                CheckForUpdates(false);
        }

        internal static bool ShouldAskForUpdateCheckOnStart(AppSettings settings)
        {
            return settings != null && settings.CheckForUpdates && settings.AskForUpdateCheckOnStart;
        }

        internal static bool ShouldCheckForUpdatesOnStart(AppSettings settings, DateTime utcNow)
        {
            if (settings == null || !settings.CheckForUpdates)
                return false;
            if (settings.AskForUpdateCheckOnStart)
                return false;
            if (settings.LastUpdateCheckUtcTicks <= 0)
                return true;
            try
            {
                var lastCheck = new DateTime(settings.LastUpdateCheckUtcTicks, DateTimeKind.Utc);
                TimeSpan age = utcNow.ToUniversalTime() - lastCheck;
                return age < TimeSpan.Zero || age >= TimeSpan.FromHours(24);
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
        }

        private void CheckForUpdates(bool userInitiated)
        {
            if (updateCheckInProgress)
                return;
            updateCheckInProgress = true;
            updateButton.Enabled = false;
            updateButton.Text = "Prüfung läuft ...";
            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateCheckResult result = UpdateChecker.CheckLatestRelease();
                try
                {
                    if (IsDisposed || Disposing || !IsHandleCreated)
                        return;
                    BeginInvoke(new Action(delegate { CompleteUpdateCheck(result, userInitiated); }));
                }
                catch (InvalidOperationException)
                {
                }
            });
        }

        private void CompleteUpdateCheck(UpdateCheckResult result, bool userInitiated)
        {
            updateCheckInProgress = false;
            updateButton.Enabled = true;
            if (!result.Success)
            {
                RefreshUpdateButtonFromCache();
                if (userInitiated)
                    MessageBox.Show(this, "Die Updateprüfung konnte nicht abgeschlossen werden.\r\n\r\n" + result.ErrorMessage, "Updateprüfung fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Version current = Assembly.GetExecutingAssembly().GetName().Version;
            string latestText = UpdateChecker.DisplayVersion(result.LatestVersion);
            bool newer = UpdateChecker.IsNewer(result.LatestVersion, current);
            appSettings.LastUpdateCheckUtcTicks = DateTime.UtcNow.Ticks;
            appSettings.LatestKnownVersion = latestText;
            try { appSettings.Save(); } catch { }
            RefreshUpdateButtonFromCache();

            if (!newer)
            {
                if (userInitiated)
                    MessageBox.Show(this, "PANDA ist aktuell.\r\n\r\nInstallierte Version: " + UpdateChecker.DisplayVersion(current), "Keine Updates verfügbar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool notify = userInitiated || !string.Equals(appSettings.LastNotifiedVersion, latestText, StringComparison.OrdinalIgnoreCase);
            if (!notify)
                return;
            appSettings.LastNotifiedVersion = latestText;
            try { appSettings.Save(); } catch { }
            DialogResult choice = MessageBox.Show(this,
                "Eine neue PANDA-Version ist verfügbar.\r\n\r\n"
                + "Installiert: " + UpdateChecker.DisplayVersion(current) + "\r\n"
                + "Verfügbar: " + latestText + "\r\n\r\n"
                + "PANDA lädt oder startet keine Datei automatisch. Möchtest du die offizielle GitHub-Release-Seite öffnen?",
                "PANDA-Update verfügbar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            if (choice == DialogResult.Yes)
                OpenReleasePage();
        }

        private void RefreshUpdateButtonFromCache()
        {
            Version latest;
            Version current = Assembly.GetExecutingAssembly().GetName().Version;
            bool updateAvailable = UpdateChecker.TryParseVersionText(appSettings.LatestKnownVersion, out latest)
                && UpdateChecker.IsNewer(latest, current);
            if (updateAvailable)
            {
                updateButton.Text = "Update v" + UpdateChecker.DisplayVersion(latest) + " verfügbar";
                updateButton.BackColor = Color.FromArgb(29, 157, 105);
                updateButton.ForeColor = Color.White;
                updateButton.FlatAppearance.BorderSize = 0;
            }
            else
            {
                updateButton.Text = "Updates prüfen";
                updateButton.BackColor = Color.White;
                updateButton.ForeColor = Navy;
                updateButton.FlatAppearance.BorderSize = 1;
                updateButton.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
            }
        }

        private void OpenReleasePage()
        {
            try
            {
                Process.Start(new ProcessStartInfo(UpdateChecker.ReleasePageUrl) { UseShellExecute = true });
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Die Release-Seite konnte nicht geöffnet werden.\r\n\r\n" + UpdateChecker.ReleasePageUrl + "\r\n\r\n" + exception.Message, "Browser konnte nicht geöffnet werden", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid(DataGridView grid, bool selectable)
        {
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(228, 234, 242);
            grid.RowHeadersVisible = true;
            grid.RowHeadersWidth = selectable ? 74 : 50;
            grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            grid.RowHeadersDefaultCellStyle.ForeColor = Muted;
            grid.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersHeight = 36;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 253);
            grid.EnableHeadersVisualStyles = false;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Navy;
            grid.DefaultCellStyle.SelectionBackColor = selectable ? Color.FromArgb(214, 227, 255) : Color.White;
            grid.DefaultCellStyle.SelectionForeColor = Navy;
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grid.RowTemplate.Height = 30;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToOrderColumns = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = true;
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            grid.DataError += delegate { };

            if (selectable)
            {
                grid.RowPostPaint += DrawOriginalRowHeader;
                grid.RowHeaderMouseClick += ToggleOriginalRow;
                grid.ColumnHeaderMouseClick += SelectOriginalColumn;
                grid.Scroll += delegate { SyncScroll(originalGrid, resultGrid); };
            }
            else
            {
                grid.MultiSelect = false;
                grid.TabStop = false;
                grid.SelectionChanged += delegate
                {
                    if (grid.SelectedCells.Count > 0)
                        grid.ClearSelection();
                };
                grid.CellMouseDown += delegate(object sender, DataGridViewCellMouseEventArgs args)
                {
                    if (args.RowIndex < 0 || args.ColumnIndex < 0)
                        return;
                    originalGrid.ClearSelection();
                    originalGrid.CurrentCell = originalGrid.Rows[args.RowIndex].Cells[args.ColumnIndex];
                    originalGrid.CurrentCell.Selected = true;
                    originalGrid.Focus();
                    BeginInvoke(new Action(delegate
                    {
                        grid.ClearSelection();
                        grid.CurrentCell = null;
                    }));
                };
                grid.Scroll += delegate { SyncScroll(resultGrid, originalGrid); };
            }
        }

        private void DrawOriginalRowHeader(object sender, DataGridViewRowPostPaintEventArgs args)
        {
            var grid = (DataGridView)sender;
            var bounds = new Rectangle(0, args.RowBounds.Top, grid.RowHeadersWidth, args.RowBounds.Height);
            using (var backgroundBrush = new SolidBrush(Color.FromArgb(248, 250, 253)))
                args.Graphics.FillRectangle(backgroundBrush, bounds);
            using (var linePen = new Pen(Color.FromArgb(228, 234, 242)))
                args.Graphics.DrawLine(linePen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

            bool selected = grid.Rows[args.RowIndex].Tag is bool && (bool)grid.Rows[args.RowIndex].Tag;
            int checkSize = 14;
            var checkPoint = new Point(8, bounds.Top + (bounds.Height - checkSize) / 2);
            CheckBoxRenderer.DrawCheckBox(args.Graphics, checkPoint, selected ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
            var numberBounds = new Rectangle(29, bounds.Top, bounds.Width - 32, bounds.Height);
            TextRenderer.DrawText(args.Graphics, (args.RowIndex + 1).ToString(), grid.Font, numberBounds, Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        private void ToggleOriginalRow(object sender, DataGridViewCellMouseEventArgs args)
        {
            if (args.RowIndex < 0 || args.RowIndex >= originalGrid.Rows.Count)
                return;
            var row = originalGrid.Rows[args.RowIndex];
            bool wasSelected = row.Tag is bool && (bool)row.Tag;
            bool isSelected = !wasSelected;
            row.Tag = isSelected;
            foreach (DataGridViewCell cell in row.Cells)
                cell.Selected = isSelected;
            scopeComboBox.SelectedIndex = 0;
            originalGrid.InvalidateRow(args.RowIndex);
            BeginInvoke(new Action(RefreshCheckedRowSelections));
            statusLabel.Text = isSelected
                ? "Zeile " + (args.RowIndex + 1) + " vollständig ausgewählt."
                : "Zeile " + (args.RowIndex + 1) + " aus der vollständigen Auswahl entfernt.";
            statusLabel.ForeColor = Muted;
        }

        private void RefreshCheckedRowSelections()
        {
            foreach (DataGridViewRow row in originalGrid.Rows)
            {
                bool isSelected = row.Tag is bool && (bool)row.Tag;
                if (!isSelected)
                    continue;
                foreach (DataGridViewCell cell in row.Cells)
                    cell.Selected = true;
            }
            originalGrid.Invalidate();
        }

        private void SelectOriginalColumn(object sender, DataGridViewCellMouseEventArgs args)
        {
            SelectOriginalColumn(args.ColumnIndex, (ModifierKeys & Keys.Control) == Keys.Control);
        }

        internal void SelectOriginalColumn(int columnIndex, bool additive)
        {
            if (columnIndex < 0 || columnIndex >= originalGrid.Columns.Count)
                return;
            bool hasCheckedRows = originalGrid.Rows.Cast<DataGridViewRow>().Any(row => row.Tag is bool && (bool)row.Tag);
            if (!additive || hasCheckedRows)
            {
                originalGrid.ClearSelection();
                ClearCheckedRows();
                additive = false;
            }
            var visibleRows = originalGrid.Rows.Cast<DataGridViewRow>().Where(row => row.Visible).ToList();
            bool allSelected = visibleRows.Count > 0 && visibleRows.All(row => row.Cells[columnIndex].Selected);
            bool selectColumn = !additive || !allSelected;
            foreach (DataGridViewRow row in visibleRows)
                row.Cells[columnIndex].Selected = selectColumn;
            scopeComboBox.SelectedIndex = 0;
            originalGrid.Invalidate();
            string columnName = originalGrid.Columns[columnIndex].HeaderText;
            statusLabel.Text = selectColumn
                ? "Spalte „" + columnName + "“ vollständig ausgewählt. Mit Strg + Klick lassen sich weitere Spalten ergänzen."
                : "Spalte „" + columnName + "“ aus der Auswahl entfernt.";
            statusLabel.ForeColor = Muted;
        }

        internal int SelectedCellCount
        {
            get { return originalGrid.SelectedCells.Count; }
        }

        internal bool IsColumnFullySelected(int columnIndex)
        {
            var visibleRows = originalGrid.Rows.Cast<DataGridViewRow>().Where(row => row.Visible).ToList();
            return visibleRows.Count > 0 && visibleRows.All(row => row.Cells[columnIndex].Selected);
        }

        private void ClearCheckedRows()
        {
            foreach (DataGridViewRow row in originalGrid.Rows)
                row.Tag = false;
            originalGrid.Invalidate();
        }

        private void OpenSelectionTemplates()
        {
            if (document == null)
            {
                ShowCsvRequiredMessage("Auswahlvorlagen");
                return;
            }
            List<int> selectedColumns = originalGrid.SelectedCells
                .Cast<DataGridViewCell>()
                .Select(cell => cell.ColumnIndex)
                .Distinct()
                .OrderBy(index => index)
                .ToList();
            using (var dialog = new SelectionTemplatesForm(document.Headers, selectionTemplates, selectedColumns))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.TemplateToApply != null)
                    ApplySelectionTemplate(dialog.TemplateToApply);
            }
        }

        private void ApplySelectionTemplate(SelectionTemplate template)
        {
            List<int> columns = SelectionTemplateStore.FindColumnIndices(document.Headers, template.Columns);
            var missing = template.Columns
                .Where(column => !document.Headers.Any(header => string.Equals(header, column, StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (columns.Count == 0)
            {
                MessageBox.Show(this, "Keine Spalte der Vorlage „" + template.Name + "“ ist in der importierten CSV vorhanden.", "Vorlage nicht anwendbar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            originalGrid.ClearSelection();
            ClearCheckedRows();
            DataGridViewRow firstVisibleRow = originalGrid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row => row.Visible);
            if (firstVisibleRow != null)
                originalGrid.CurrentCell = firstVisibleRow.Cells[columns[0]];
            foreach (int column in columns)
                foreach (DataGridViewRow row in originalGrid.Rows.Cast<DataGridViewRow>().Where(row => row.Visible))
                    row.Cells[column].Selected = true;
            scopeComboBox.SelectedIndex = 0;
            originalGrid.Invalidate();
            statusLabel.Text = "Vorlage „" + template.Name + "“ angewendet: " + columns.Count + " Spalten ausgewählt."
                + (missing.Count == 0 ? string.Empty : " Nicht gefunden: " + string.Join(", ", missing.ToArray()) + ".");
            statusLabel.ForeColor = missing.Count == 0 ? Color.FromArgb(29, 132, 88) : Color.FromArgb(188, 118, 24);
        }

        private void ImportCsv()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "CSV-Datei importieren";
                dialog.Filter = "CSV-Dateien (*.csv)|*.csv|Textdateien (*.txt)|*.txt|Alle Dateien (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    using (var wizard = new ImportWizardForm(dialog.FileName))
                    {
                        if (wizard.ShowDialog(this) != DialogResult.OK)
                            return;
                        document = wizard.SelectedDocument;
                    }
                    importedPath = dialog.FileName;
                    activeRowFilter = null;
                    filterButton.Text = IsClassicDesign ? "Filter" : "Zeilen filtern";
                    PopulateGrids();
                    baseFileLabelText = Path.GetFileName(dialog.FileName) + "  •  " + document.Rows.Count + " Zeilen  •  " + document.Headers.Count + " importierte Spalten  •  " + DelimiterName(document.Delimiter);
                    UpdateFileLabel(0);
                    statusLabel.Text = "Import erfolgreich. Wähle links Zellen aus oder nutze ‚Alle Einträge‘.";
                    statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
                    SetDocumentControlsEnabled(true);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, "Die CSV-Datei konnte nicht importiert werden.\r\n\r\n" + exception.Message, "Import fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Import fehlgeschlagen.";
                    statusLabel.ForeColor = Color.FromArgb(190, 60, 55);
                }
            }
        }

        private void ClearCurrentCsv()
        {
            if (document == null)
            {
                ShowCsvRequiredMessage("Aktuelle CSV leeren");
                return;
            }
            DialogResult confirmation = MessageBox.Show(this,
                "Die aktuelle CSV wird aus PANDA entfernt. Nicht exportierte Änderungen gehen verloren.\r\n\r\n"
                + "Die ursprüngliche Datei auf dem Datenträger bleibt unverändert. Möchtest du fortfahren?",
                "Aktuelle CSV leeren?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirmation != DialogResult.Yes)
                return;
            ClearDocumentCore();
        }

        private void ClearDocumentCore()
        {
            originalGrid.CurrentCell = null;
            resultGrid.CurrentCell = null;
            originalGrid.Columns.Clear();
            resultGrid.Columns.Clear();
            document = null;
            importedPath = null;
            baseFileLabelText = string.Empty;
            activeRowFilter = null;
            filterButton.Text = IsClassicDesign ? "Filter" : "Zeilen filtern";
            fileLabel.Text = "Noch keine CSV geladen";
            statusLabel.Text = "Bereit – bitte eine CSV-Datei importieren.";
            statusLabel.ForeColor = Muted;
            scopeComboBox.SelectedIndex = 0;
            SetDocumentControlsEnabled(false);
        }

        private void OpenRowFilter()
        {
            if (document == null)
            {
                ShowCsvRequiredMessage("Zeilen filtern");
                return;
            }
            var rows = document.Rows.Select(row => (IList<string>)row).ToList();
            using (var dialog = new RowFilterForm(document.Headers, rows, activeRowFilter))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                activeRowFilter = dialog.SelectedFilter;
                ApplyRowFilter();
            }
        }

        private void ApplyRowFilter()
        {
            if (document == null)
                return;
            var rows = document.Rows.Select(row => (IList<string>)row).ToList();
            List<int> hiddenRows = RowFilterEngine.FindHiddenRows(rows, activeRowFilter);
            var hiddenSet = new HashSet<int>(hiddenRows);
            originalGrid.CurrentCell = null;
            resultGrid.CurrentCell = null;
            originalGrid.ClearSelection();
            resultGrid.ClearSelection();
            ClearCheckedRows();
            for (int rowIndex = 0; rowIndex < document.Rows.Count; rowIndex++)
            {
                bool visible = !hiddenSet.Contains(rowIndex);
                originalGrid.Rows[rowIndex].Visible = visible;
                resultGrid.Rows[rowIndex].Visible = visible;
            }
            bool filterActive = activeRowFilter != null && !activeRowFilter.IsEmpty;
            filterButton.Text = filterActive ? "Filter aktiv" : (IsClassicDesign ? "Filter" : "Zeilen filtern");
            UpdateFileLabel(hiddenRows.Count);
            if (filterActive)
            {
                string columnName = activeRowFilter.ColumnIndex >= 0 && activeRowFilter.ColumnIndex < document.Headers.Count
                    ? document.Headers[activeRowFilter.ColumnIndex]
                    : "Spalte";
                statusLabel.Text = hiddenRows.Count + " von " + document.Rows.Count + " Zeilen über „" + columnName + "“ ausgeblendet. Ausgeblendete Zeilen werden nicht umgewandelt.";
            }
            else
            {
                statusLabel.Text = "Filter aufgehoben. Alle " + document.Rows.Count + " Zeilen werden wieder angezeigt.";
            }
            statusLabel.ForeColor = filterActive ? Color.FromArgb(188, 118, 24) : Muted;
        }

        private void UpdateFileLabel(int hiddenRowCount)
        {
            fileLabel.Text = baseFileLabelText + (hiddenRowCount > 0 ? "  •  " + hiddenRowCount + " ausgeblendet" : string.Empty);
        }

        private string DelimiterName(char delimiter)
        {
            if (delimiter == ';') return "Semikolon";
            if (delimiter == ',') return "Komma";
            if (delimiter == '\t') return "Tabulator";
            return delimiter.ToString();
        }

        private void PopulateGrids()
        {
            originalGrid.SuspendLayout();
            resultGrid.SuspendLayout();
            originalGrid.Columns.Clear();
            resultGrid.Columns.Clear();
            originalGrid.Rows.Clear();
            resultGrid.Rows.Clear();

            for (int column = 0; column < document.Headers.Count; column++)
            {
                string key = "Column" + column;
                originalGrid.Columns.Add(key, document.Headers[column]);
                resultGrid.Columns.Add(key, document.Headers[column]);
                originalGrid.Columns[column].SortMode = DataGridViewColumnSortMode.NotSortable;
                resultGrid.Columns[column].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            foreach (var row in document.Rows)
            {
                originalGrid.Rows.Add(row.Cast<object>().ToArray());
                resultGrid.Rows.Add(row.Cast<object>().ToArray());
            }

            for (int row = 0; row < document.Rows.Count; row++)
            {
                originalGrid.Rows[row].HeaderCell.Value = (row + 1).ToString();
                originalGrid.Rows[row].Tag = false;
                resultGrid.Rows[row].HeaderCell.Value = (row + 1).ToString();
            }

            originalGrid.ClearSelection();
            resultGrid.ClearSelection();
            resultGrid.CurrentCell = null;
            originalGrid.ResumeLayout();
            resultGrid.ResumeLayout();
        }

        private void ApplyConfiguredShift(bool countDown)
        {
            ApplyShift(GetConfiguredShiftValues(), countDown);
        }

        private void ApplyShift(IList<int> sequence, bool countDown)
        {
            if (document == null)
                return;

            var cells = GetTargetCells();
            if (cells.Count == 0)
            {
                MessageBox.Show(this, "Bitte markiere mindestens eine Zelle in der linken Tabelle oder wähle als Bereich ‚Alle Einträge‘.", "Keine Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (appSettings.ConfirmBeforeShift)
            {
                string message = BuildConfirmationMessage(sequence, countDown, cells.Count, scopeComboBox.SelectedIndex == 2);
                DialogResult confirmation = MessageBox.Show(this, message, "Umwandlung bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (confirmation != DialogResult.Yes)
                {
                    statusLabel.Text = "Umwandlung abgebrochen.";
                    statusLabel.ForeColor = Muted;
                    return;
                }
            }

            int changed = 0;
            List<int> signedSequence = sequence.Select(value => countDown ? -Math.Abs(value) : Math.Abs(value)).ToList();
            foreach (var coordinate in cells)
            {
                var cell = resultGrid.Rows[coordinate.Item1].Cells[coordinate.Item2];
                string before = Convert.ToString(cell.Value) ?? string.Empty;
                string after = LetterShifter.Shift(before, signedSequence);
                cell.Value = after;
                if (!string.Equals(before, after, StringComparison.Ordinal))
                    changed++;
            }

            statusLabel.Text = changed + " von " + cells.Count + " Zellen verändert ("
                + (countDown ? "runter" : "hoch") + ": " + ShiftSequence.Format(sequence) + ").";
            statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
        }

        internal static string BuildConfirmationMessage(int amount, int cellCount, bool allValues)
        {
            return BuildConfirmationMessage(new[] { Math.Abs(amount) }, amount < 0, cellCount, allValues);
        }

        internal static string BuildConfirmationMessage(IList<int> sequence, bool countDown, int cellCount, bool allValues)
        {
            string subject = allValues ? "Alle Werte" : "Die ausgewählten Werte";
            string direction = countDown ? "runtergezählt" : "hochgezählt";
            string sequenceText = ShiftSequence.Format(sequence);
            string description = sequence.Count == 1
                ? subject + " werden um " + sequenceText + " " + direction + "."
                : subject + " werden mit der Zählfolge " + sequenceText + " " + direction + ".";
            return description + "\r\n\r\n"
                + "Gewählte Zählfolge: " + sequenceText + "\r\n"
                + "Betroffene Zellen: " + cellCount + "\r\n\r\n"
                + "Möchtest du die Umwandlung durchführen?";
        }

        private List<Tuple<int, int>> GetTargetCells()
        {
            var result = new List<Tuple<int, int>>();
            if (scopeComboBox.SelectedIndex == 2)
            {
                for (int row = 0; row < resultGrid.Rows.Count; row++)
                {
                    if (!resultGrid.Rows[row].Visible)
                        continue;
                    for (int column = 0; column < resultGrid.Columns.Count; column++)
                        result.Add(Tuple.Create(row, column));
                }
                return result;
            }

            if (scopeComboBox.SelectedIndex == 1)
            {
                var current = originalGrid.CurrentCell;
                if (current != null)
                    result.Add(Tuple.Create(current.RowIndex, current.ColumnIndex));
                return result;
            }

            foreach (DataGridViewCell cell in originalGrid.SelectedCells)
                result.Add(Tuple.Create(cell.RowIndex, cell.ColumnIndex));
            foreach (DataGridViewRow row in originalGrid.Rows)
            {
                bool wholeRow = row.Tag is bool && (bool)row.Tag;
                if (!wholeRow)
                    continue;
                for (int column = 0; column < resultGrid.Columns.Count; column++)
                    result.Add(Tuple.Create(row.Index, column));
            }
            return result.Distinct().ToList();
        }

        private void ResetResults()
        {
            if (document == null)
                return;
            for (int row = 0; row < document.Rows.Count; row++)
                for (int column = 0; column < document.Headers.Count; column++)
                    resultGrid.Rows[row].Cells[column].Value = document.Rows[row][column];
            statusLabel.Text = "Alle Ergebnisse wurden auf die importierten Werte zurückgesetzt.";
            statusLabel.ForeColor = Muted;
        }

        private void ExportCsv()
        {
            if (document == null)
                return;

            List<IList<string>> currentRows = GetResultRows();
            if (currentRows.Count == 0)
            {
                MessageBox.Show(this, "Die importierte CSV enthält keine Datenzeilen.", "Keine Zeilen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            List<IList<string>> originalRows = document.Rows.Select(row => (IList<string>)row).ToList();
            List<int> changedRows = ExportRowSelector.FindChangedRows(originalRows, currentRows);
            List<int> initiallySelectedRows = originalGrid.Rows
                .Cast<DataGridViewRow>()
                .Where(row => row.Tag is bool && (bool)row.Tag)
                .Select(row => row.Index)
                .ToList();
            if (initiallySelectedRows.Count == 0)
                initiallySelectedRows = changedRows.ToList();

            List<int> selectedRows;
            using (var options = new ExportOptionsForm(document.Headers, currentRows, changedRows, initiallySelectedRows))
            {
                if (options.ShowDialog(this) != DialogResult.OK)
                    return;
                selectedRows = options.SelectedRowIndices;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Veränderte CSV exportieren";
                dialog.Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
                dialog.DefaultExt = "csv";
                dialog.AddExtension = true;
                string sourceName = string.IsNullOrEmpty(importedPath) ? "ergebnis" : Path.GetFileNameWithoutExtension(importedPath) + "_veraendert";
                dialog.FileName = sourceName + ".csv";
                dialog.InitialDirectory = string.IsNullOrEmpty(importedPath) ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) : Path.GetDirectoryName(importedPath);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    var rowsToExport = selectedRows.Select(row => currentRows[row]).ToList();
                    CsvCodec.Save(dialog.FileName, document, rowsToExport);
                    statusLabel.Text = "Export erfolgreich: " + selectedRows.Count + " von " + currentRows.Count + " Zeilen gespeichert.";
                    statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
                    MessageBox.Show(this, selectedRows.Count + " von " + currentRows.Count + " Zeilen wurden erfolgreich gespeichert.", "Export abgeschlossen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, "Die CSV-Datei konnte nicht gespeichert werden.\r\n\r\n" + exception.Message, "Export fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private List<IList<string>> GetResultRows()
        {
            var rows = new List<IList<string>>();
            foreach (DataGridViewRow gridRow in resultGrid.Rows)
            {
                var values = new List<string>();
                foreach (DataGridViewCell cell in gridRow.Cells)
                    values.Add(Convert.ToString(cell.Value) ?? string.Empty);
                rows.Add(values);
            }
            return rows;
        }

        private void SyncScroll(DataGridView source, DataGridView target)
        {
            if (source.Rows.Count == 0 || target.Rows.Count == 0)
                return;
            try
            {
                target.FirstDisplayedScrollingRowIndex = source.FirstDisplayedScrollingRowIndex;
                target.HorizontalScrollingOffset = source.HorizontalScrollingOffset;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        private void SetDocumentControlsEnabled(bool enabled)
        {
            scopeComboBox.Enabled = enabled;
            if (enabled && scopeComboBox.SelectedIndex < 0 && scopeComboBox.Items.Count > 0)
                scopeComboBox.SelectedIndex = 0;
            simpleShiftNumeric.Enabled = enabled;
            foreach (NumericUpDown numeric in advancedShiftNumerics)
                numeric.Enabled = enabled;
            exportButton.Enabled = enabled;
            resetButton.Enabled = enabled;
            bool enableMetroNavigation = !IsClassicDesign;
            templatesButton.Enabled = enabled || enableMetroNavigation;
            clearButton.Enabled = enabled || enableMetroNavigation;
            filterButton.Enabled = enabled || enableMetroNavigation;
            scopeComboBox.Refresh();
        }

        private void ShowCsvRequiredMessage(string action)
        {
            MessageBox.Show(this,
                "Bitte importiere zuerst eine CSV-Datei, bevor du \u201e" + action + "\u201c verwendest.",
                "Keine CSV geladen",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        internal void LoadPreviewData()
        {
            document = new CsvDocument { Delimiter = ';', FirstRowIsHeader = true };
            document.Headers.AddRange(new[] { "Kundennummer", "Vorname", "Nachname", "Ort" });
            document.Rows.Add(new List<string> { "1001", "Anna", "Meyer", "Berlin" });
            document.Rows.Add(new List<string> { "1002", "Jonas", "Schmidt", "Hamburg" });
            document.Rows.Add(new List<string> { "1003", "Zoe", "Fischer", "München" });
            document.Rows.Add(new List<string> { "1004", "Lena", "Wagner", "Köln" });
            importedPath = "beispiel.csv";
            activeRowFilter = null;
            PopulateGrids();
            LoadShiftConfiguration("3-5-8-2-6");
            var previewSequence = new List<int> { 3, 5, 8, 2, 6 };
            for (int column = 0; column < document.Headers.Count; column++)
                resultGrid.Rows[0].Cells[column].Value = LetterShifter.Shift(document.Rows[0][column], previewSequence);
            originalGrid.Rows[0].Tag = true;
            RefreshCheckedRowSelections();
            baseFileLabelText = "beispiel.csv  •  4 Zeilen  •  4 Spalten  •  Trennzeichen: Semikolon";
            UpdateFileLabel(0);
            statusLabel.Text = "3 von 4 Zellen verändert (hoch: 3-5-8-2-6).";
            statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
            SetDocumentControlsEnabled(true);
            scopeComboBox.SelectedIndex = 0;
            scopeComboBox.Refresh();
        }

        internal int VisibleRowCount
        {
            get { return originalGrid.Rows.Cast<DataGridViewRow>().Count(row => row.Visible); }
        }

        internal bool HasLoadedDocument
        {
            get { return document != null; }
        }

        internal bool UsesAdvancedShiftMode
        {
            get { return advancedShiftMode; }
        }

        internal string CurrentShiftSequence
        {
            get { return ShiftSequence.Format(GetConfiguredShiftValues()); }
        }

        internal string ShiftValuesCaptionText
        {
            get { return shiftValuesCaptionLabel.Text; }
        }

        internal string ShiftModeHintText
        {
            get { return sequenceRestartHint.Text; }
        }

        internal string ActiveInterfaceStyle
        {
            get { return activeInterfaceStyle; }
        }

        internal void LoadShiftConfigurationForTest(string sequenceText)
        {
            LoadShiftConfiguration(sequenceText);
        }

        internal void SetAdvancedShiftModeForTest(bool advanced)
        {
            SetAdvancedShiftMode(advanced);
        }

        internal void SwitchInterfaceStyleForTest(string interfaceStyle)
        {
            ApplyInterfaceStyle(interfaceStyle);
        }

        internal void ApplyFilterForTest(RowFilter filter)
        {
            activeRowFilter = filter;
            ApplyRowFilter();
        }

        internal void ClearDocumentForTest()
        {
            ClearDocumentCore();
        }
    }
}
