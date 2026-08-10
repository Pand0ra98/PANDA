using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

[assembly: AssemblyTitle("PANDA")]
[assembly: AssemblyDescription("Pseudonymisierung alphanumerischer Nutzdaten durch Alphabetverschiebung")]
[assembly: AssemblyProduct("PANDA")]
[assembly: AssemblyCompany("PANDA")]
[assembly: AssemblyVersion("1.5.0.0")]
[assembly: AssemblyFileVersion("1.5.0.0")]

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
                using (var form = new MainForm())
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
                var previewSettings = new AppSettings { DefaultShift = 6, ConfirmBeforeShift = true };
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
            if (string.IsNullOrEmpty(value) || amount == 0)
                return value;

            var result = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                if (character >= 'A' && character <= 'Z')
                    result.Append(ShiftInRange(character, 'A', 26, amount));
                else if (character >= 'a' && character <= 'z')
                    result.Append(ShiftInRange(character, 'a', 26, amount));
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

    internal sealed class AppSettings
    {
        public int DefaultShift = 1;
        public bool ConfirmBeforeShift = true;

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
                if (string.Equals(key, "DefaultShift", StringComparison.OrdinalIgnoreCase))
                {
                    int parsed;
                    if (int.TryParse(value, out parsed))
                        settings.DefaultShift = Math.Max(1, Math.Min(25, parsed));
                }
                else if (string.Equals(key, "ConfirmBeforeShift", StringComparison.OrdinalIgnoreCase))
                {
                    bool parsed;
                    if (bool.TryParse(value, out parsed))
                        settings.ConfirmBeforeShift = parsed;
                }
            }
            return settings;
        }

        internal string[] Serialize()
        {
            return new[]
            {
                "DefaultShift=" + DefaultShift,
                "ConfirmBeforeShift=" + ConfirmBeforeShift
            };
        }
    }

    internal sealed class SettingsForm : Form
    {
        private readonly Color Navy = Color.FromArgb(24, 38, 58);
        private readonly Color Blue = Color.FromArgb(41, 112, 255);
        private readonly Color Background = Color.FromArgb(244, 247, 251);
        private readonly Color Muted = Color.FromArgb(94, 108, 128);
        private readonly NumericUpDown defaultShiftNumeric = new NumericUpDown();
        private readonly CheckBox confirmationCheckBox = new CheckBox();

        public int DefaultShift { get { return (int)defaultShiftNumeric.Value; } }
        public bool ConfirmBeforeShift { get { return confirmationCheckBox.Checked; } }

        public SettingsForm(AppSettings settings)
        {
            Text = "PANDA – Einstellungen";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 330);
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
                RowCount = 4,
                BackColor = Color.White,
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(0, 0, 0, 12)
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var shiftLabel = new Label
            {
                Text = "Standard-Zählwert",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Navy,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            card.Controls.Add(shiftLabel, 0, 0);
            card.SetColumnSpan(shiftLabel, 2);
            card.Controls.Add(new Label
            {
                Text = "Dieser Wert wird beim Programmstart und nach dem Speichern voreingestellt.",
                ForeColor = Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 1);
            defaultShiftNumeric.Minimum = 1;
            defaultShiftNumeric.Maximum = 25;
            defaultShiftNumeric.Value = Math.Max(1, Math.Min(25, settings.DefaultShift));
            defaultShiftNumeric.TextAlign = HorizontalAlignment.Center;
            defaultShiftNumeric.Dock = DockStyle.Fill;
            defaultShiftNumeric.Margin = new Padding(8, 5, 0, 5);
            card.Controls.Add(defaultShiftNumeric, 1, 1);
            confirmationCheckBox.Text = "Vor jeder Umwandlung eine Bestätigung mit dem gewählten Zählwert anzeigen";
            confirmationCheckBox.Checked = settings.ConfirmBeforeShift;
            confirmationCheckBox.AutoSize = true;
            confirmationCheckBox.ForeColor = Navy;
            confirmationCheckBox.Anchor = AnchorStyles.Left;
            card.SetColumnSpan(confirmationCheckBox, 2);
            card.Controls.Add(confirmationCheckBox, 0, 3);
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
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Blue,
                ForeColor = Color.White,
                Margin = new Padding(0)
            };
            saveButton.FlatAppearance.BorderSize = 0;
            footer.Controls.Add(cancelButton, 1, 0);
            footer.Controls.Add(saveButton, 3, 0);
            root.Controls.Add(footer, 0, 2);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
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
        private readonly NumericUpDown stepNumeric = new NumericUpDown();
        private readonly Label statusLabel = new Label();
        private readonly Label fileLabel = new Label();
        private readonly Button exportButton = new Button();
        private readonly Button resetButton = new Button();
        private readonly Button settingsButton = new Button();
        private readonly Button templatesButton = new Button();

        private CsvDocument document;
        private string importedPath;
        private AppSettings appSettings = AppSettings.Load();
        private readonly List<SelectionTemplate> selectionTemplates = SelectionTemplateStore.Load();

        public MainForm()
        {
            Text = "PANDA – Pseudonymisierung alphanumerischer Nutzdaten";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 680);
            Size = new Size(1320, 820);
            BackColor = Background;
            Font = new Font("Segoe UI", 9F);
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

            BuildLayout();
            ConfigureGrid(originalGrid, true);
            ConfigureGrid(resultGrid, false);
            SetDocumentControlsEnabled(false);
        }

        private void BuildLayout()
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            Controls.Add(root);

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
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            root.Controls.Add(header, 0, 0);

            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 9,
                RowCount = 2,
                Padding = new Padding(12, 10, 12, 8),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 12)
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            root.Controls.Add(toolbar, 0, 1);

            var importButton = CreateButton("Importieren", Blue, Color.White);
            importButton.Click += delegate { ImportCsv(); };
            toolbar.Controls.Add(importButton, 0, 0);

            scopeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            scopeComboBox.Items.AddRange(new object[] { "Markierte Zellen", "Aktuelle Zelle", "Alle Einträge" });
            scopeComboBox.SelectedIndex = 0;
            scopeComboBox.Dock = DockStyle.Fill;
            scopeComboBox.Margin = new Padding(7, 3, 7, 3);
            toolbar.Controls.Add(scopeComboBox, 1, 0);

            stepNumeric.Minimum = 1;
            stepNumeric.Maximum = 25;
            stepNumeric.Value = Math.Max(1, Math.Min(25, appSettings.DefaultShift));
            stepNumeric.Dock = DockStyle.Fill;
            stepNumeric.TextAlign = HorizontalAlignment.Center;
            stepNumeric.Margin = new Padding(7, 3, 7, 3);
            toolbar.Controls.Add(stepNumeric, 2, 0);

            var upButton = CreateButton("Hochzählen  +", Color.FromArgb(29, 157, 105), Color.White);
            upButton.Click += delegate { ApplyShift((int)stepNumeric.Value); };
            toolbar.Controls.Add(upButton, 3, 0);

            var downButton = CreateButton("Runterzählen  −", Color.FromArgb(230, 91, 84), Color.White);
            downButton.Click += delegate { ApplyShift(-(int)stepNumeric.Value); };
            toolbar.Controls.Add(downButton, 4, 0);

            resetButton.Text = "Zurücksetzen";
            StyleSecondaryButton(resetButton);
            resetButton.Click += delegate { ResetResults(); };
            toolbar.Controls.Add(resetButton, 5, 0);

            exportButton.Text = "CSV exportieren";
            StyleSecondaryButton(exportButton);
            exportButton.Click += delegate { ExportCsv(); };
            toolbar.Controls.Add(exportButton, 6, 0);

            settingsButton.Text = "Einstellungen";
            StyleSecondaryButton(settingsButton);
            settingsButton.Click += delegate { OpenSettings(); };
            toolbar.Controls.Add(settingsButton, 7, 0);

            templatesButton.Text = "Vorlagen";
            StyleSecondaryButton(templatesButton);
            templatesButton.Click += delegate { OpenSelectionTemplates(); };
            toolbar.Controls.Add(templatesButton, 8, 0);

            fileLabel.Text = "Noch keine CSV geladen";
            fileLabel.ForeColor = Muted;
            fileLabel.AutoEllipsis = true;
            fileLabel.Dock = DockStyle.Fill;
            fileLabel.TextAlign = ContentAlignment.MiddleLeft;
            toolbar.SetColumnSpan(fileLabel, 4);
            toolbar.Controls.Add(fileLabel, 0, 1);

            var hint = new Label
            {
                Text = "Tipp: Spaltenkopf anklicken; mit Strg weitere Spalten ergänzen.",
                ForeColor = Muted,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            toolbar.SetColumnSpan(hint, 5);
            toolbar.Controls.Add(hint, 4, 1);

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
                Margin = new Padding(4, 2, 4, 2)
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
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 216, 230);
        }

        private void OpenSettings()
        {
            using (var dialog = new SettingsForm(appSettings))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                appSettings.DefaultShift = dialog.DefaultShift;
                appSettings.ConfirmBeforeShift = dialog.ConfirmBeforeShift;
                try
                {
                    appSettings.Save();
                    stepNumeric.Value = appSettings.DefaultShift;
                    statusLabel.Text = "Einstellungen gespeichert. Standard-Zählwert: " + appSettings.DefaultShift + ".";
                    statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, "Die Einstellungen konnten nicht gespeichert werden.\r\n\r\n" + exception.Message, "Speichern fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            bool allSelected = originalGrid.Rows.Count > 0 && originalGrid.Rows.Cast<DataGridViewRow>().All(row => row.Cells[columnIndex].Selected);
            bool selectColumn = !additive || !allSelected;
            foreach (DataGridViewRow row in originalGrid.Rows)
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
            return originalGrid.Rows.Count > 0
                && originalGrid.Rows.Cast<DataGridViewRow>().All(row => row.Cells[columnIndex].Selected);
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
                return;
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
            if (originalGrid.Rows.Count > 0)
                originalGrid.CurrentCell = originalGrid.Rows[0].Cells[columns[0]];
            foreach (int column in columns)
                foreach (DataGridViewRow row in originalGrid.Rows)
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
                    PopulateGrids();
                    fileLabel.Text = Path.GetFileName(dialog.FileName) + "  •  " + document.Rows.Count + " Zeilen  •  " + document.Headers.Count + " importierte Spalten  •  " + DelimiterName(document.Delimiter);
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

        private void ApplyShift(int amount)
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
                string message = BuildConfirmationMessage(amount, cells.Count, scopeComboBox.SelectedIndex == 2);
                DialogResult confirmation = MessageBox.Show(this, message, "Umwandlung bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (confirmation != DialogResult.Yes)
                {
                    statusLabel.Text = "Umwandlung abgebrochen.";
                    statusLabel.ForeColor = Muted;
                    return;
                }
            }

            int changed = 0;
            foreach (var coordinate in cells)
            {
                var cell = resultGrid.Rows[coordinate.Item1].Cells[coordinate.Item2];
                string before = Convert.ToString(cell.Value) ?? string.Empty;
                string after = LetterShifter.Shift(before, amount);
                cell.Value = after;
                if (!string.Equals(before, after, StringComparison.Ordinal))
                    changed++;
            }

            statusLabel.Text = changed + " von " + cells.Count + " Zellen verändert (" + (amount > 0 ? "+" : string.Empty) + amount + ").";
            statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
        }

        internal static string BuildConfirmationMessage(int amount, int cellCount, bool allValues)
        {
            string subject = allValues ? "Alle Werte" : "Die ausgewählten Werte";
            string direction = amount > 0 ? "hochgezählt" : "runtergezählt";
            return subject + " werden um " + Math.Abs(amount) + " " + direction + ".\r\n\r\n"
                + "Gewählter Zählwert: " + Math.Abs(amount) + "\r\n"
                + "Betroffene Zellen: " + cellCount + "\r\n\r\n"
                + "Möchtest du die Umwandlung durchführen?";
        }

        private List<Tuple<int, int>> GetTargetCells()
        {
            var result = new List<Tuple<int, int>>();
            if (scopeComboBox.SelectedIndex == 2)
            {
                for (int row = 0; row < resultGrid.Rows.Count; row++)
                    for (int column = 0; column < resultGrid.Columns.Count; column++)
                        result.Add(Tuple.Create(row, column));
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
                    var rows = new List<IList<string>>();
                    foreach (DataGridViewRow gridRow in resultGrid.Rows)
                    {
                        var values = new List<string>();
                        foreach (DataGridViewCell cell in gridRow.Cells)
                            values.Add(Convert.ToString(cell.Value) ?? string.Empty);
                        rows.Add(values);
                    }
                    CsvCodec.Save(dialog.FileName, document, rows);
                    statusLabel.Text = "Export erfolgreich: " + dialog.FileName;
                    statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
                    MessageBox.Show(this, "Die veränderte CSV wurde erfolgreich gespeichert.", "Export abgeschlossen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, "Die CSV-Datei konnte nicht gespeichert werden.\r\n\r\n" + exception.Message, "Export fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
            stepNumeric.Enabled = enabled;
            exportButton.Enabled = enabled;
            resetButton.Enabled = enabled;
            templatesButton.Enabled = enabled;
            scopeComboBox.Refresh();
        }

        internal void LoadPreviewData()
        {
            document = new CsvDocument { Delimiter = ';', FirstRowIsHeader = true };
            document.Headers.AddRange(new[] { "Kundennummer", "Vorname", "Nachname", "Ort" });
            document.Rows.Add(new List<string> { "1001", "Anna", "Meyer", "Berlin" });
            document.Rows.Add(new List<string> { "1002", "Jonas", "Schmidt", "Hamburg" });
            document.Rows.Add(new List<string> { "1003", "Zoe", "Fischer", "München" });
            document.Rows.Add(new List<string> { "1004", "Lena", "Wagner", "Köln" });
            PopulateGrids();
            for (int column = 0; column < document.Headers.Count; column++)
                resultGrid.Rows[0].Cells[column].Value = LetterShifter.Shift(document.Rows[0][column], 1);
            originalGrid.Rows[0].Tag = true;
            RefreshCheckedRowSelections();
            fileLabel.Text = "beispiel.csv  •  4 Zeilen  •  4 Spalten  •  Trennzeichen: Semikolon";
            statusLabel.Text = "3 von 4 Zellen verändert (+1).";
            statusLabel.ForeColor = Color.FromArgb(29, 132, 88);
            SetDocumentControlsEnabled(true);
            scopeComboBox.SelectedIndex = 0;
            scopeComboBox.Refresh();
        }
    }
}
