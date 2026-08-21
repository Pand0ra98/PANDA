using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Panda
{
    internal static class Tests
    {
        private static int failures;

        [STAThread]
        private static void Main()
        {
            AssertEqual("BCD YZA", LetterShifter.Shift("ABC XYZ", 1), "shift up and wrap");
            AssertEqual("Zab", LetterShifter.Shift("Abc", -1), "shift down and preserve case");
            AssertEqual("Öl 123!", LetterShifter.Shift("Öl 123!", 0), "zero shift");
            AssertEqual("Öm 123!", LetterShifter.Shift("Öl 123!", 1), "non A-Z characters unchanged");
            AssertEqual("Aepfel, Oel, Ueber, gross", CompatibilityConverter.ToAscii("Äpfel, Öl, Über, groß"), "compatibility mode replaces umlauts and sharp s");
            var sequence = new List<int> { 3, 5, 8, 2 };
            AssertEqual("DGKFH", LetterShifter.Shift("ABCDE", sequence), "cyclic sequence shifts letters");
            AssertEqual("ABCDE", LetterShifter.Shift("DGKFH", sequence.Select(value => -value).ToList()), "cyclic sequence reverses");
            AssertEqual("D-G", LetterShifter.Shift("A-B", new List<int> { 3, 5 }), "punctuation does not consume sequence position");
            List<int> parsedSequence;
            string sequenceError;
            AssertEqual(true, ShiftSequence.TryParse("3-5-8-2", out parsedSequence, out sequenceError), "sequence parser accepts hyphen notation");
            AssertEqual("3-5-8-2", ShiftSequence.Format(parsedSequence), "sequence formatting");
            AssertEqual(false, ShiftSequence.TryParse("3-0-26", out parsedSequence, out sequenceError), "sequence parser enforces range");
            AssertEqual("7", ShiftSequence.NormalizeForMainMode("7"), "simple main mode keeps one value");
            AssertEqual("3-5-8-2-1", ShiftSequence.NormalizeForMainMode("3-5-8-2"), "advanced main mode pads to five values");
            AssertEqual("1-2-3-4-5", ShiftSequence.NormalizeForMainMode("1-2-3-4-5-6"), "advanced main mode limits to five values");
            List<int> pastedCipher;
            AssertEqual(true, CipherClipboard.TryParseFiveValues("3-5-8-2-6", out pastedCipher), "clipboard recognizes full cipher");
            AssertEqual("3-5-8-2-6", ShiftSequence.Format(pastedCipher), "clipboard distributes five cipher values");
            AssertEqual(true, CipherClipboard.TryParseFiveValues("4, 6, 2, 8, 3", out pastedCipher), "clipboard accepts comma separators");
            AssertEqual(false, CipherClipboard.TryParseFiveValues("6", out pastedCipher), "single clipboard value stays in current field");
            AssertEqual(false, CipherClipboard.TryParseFiveValues("1-2-3-4-5-6", out pastedCipher), "clipboard rejects more than five cipher values");
            AssertEqual(true, RowFilterEngine.WildcardIsMatch("Meyer", "M*"), "wildcard star match");
            AssertEqual(true, RowFilterEngine.WildcardIsMatch("Bob", "B?b"), "wildcard question mark match");
            AssertEqual(true, RowFilterEngine.WildcardIsMatch("BERLIN Mitte", "*berlin*"), "wildcard ignores case");
            AssertEqual(false, RowFilterEngine.WildcardIsMatch("Köln", "M*"), "wildcard mismatch");
            var filterRows = new List<IList<string>>
            {
                new List<string> { "Anna", "Berlin" },
                new List<string> { "Ben", "Hamburg" },
                new List<string> { "Clara", "München" },
                new List<string> { "David", "Mainz" }
            };
            var combinedFilter = new RowFilter(1, new[] { "Hamburg" }, "M*");
            List<int> hiddenFilterRows = RowFilterEngine.FindHiddenRows(filterRows, combinedFilter);
            AssertEqual(3, hiddenFilterRows.Count, "combined exact and wildcard filter count");
            AssertEqual(1, hiddenFilterRows[0], "exact filter row");
            AssertEqual(2, hiddenFilterRows[1], "wildcard first row");
            AssertEqual(3, hiddenFilterRows[2], "wildcard second row");
            using (var filterForm = new RowFilterForm(new[] { "Name", "Ort" }, filterRows, combinedFilter))
            {
                filterForm.Show();
                filterForm.Close();
            }

            using (var quickForm = new QuickConversionForm("6"))
            {
                AssertEqual(2, quickForm.ModeTabCount, "quick conversion exposes two mode tabs");
                AssertEqual("Standard", quickForm.StandardTabText, "quick conversion standard tab title");
                AssertEqual("Erweitert", quickForm.AdvancedTabText, "quick conversion advanced tab title");
                AssertEqual(false, quickForm.UsesAdvancedMode, "single default value selects standard tab");
                AssertEqual("6", quickForm.CurrentShiftSequence, "standard tab exposes one shift value");
                AssertEqual(true, quickForm.PasteCipherForTest("4-6-2-8-3"), "quick conversion accepts pasted cipher");
                AssertEqual(true, quickForm.UsesAdvancedMode, "pasted cipher selects advanced quick tab");
                AssertEqual("4-6-2-8-3", quickForm.CurrentShiftSequence, "quick conversion distributes pasted cipher");
                quickForm.SetInputText("Abc XYZ 123");
                quickForm.ApplyShiftForTest(2);
                AssertEqual("Cde ZAB 123", quickForm.ResultText, "quick conversion up");
                quickForm.ApplyShiftForTest(-2);
                AssertEqual("Yza VWX 123", quickForm.ResultText, "quick conversion down");
                quickForm.SetInputText("ABCDE");
                quickForm.ApplySequenceForTest("3-5-8-2-6", false);
                AssertEqual(true, quickForm.UsesAdvancedMode, "sequence selects advanced tab");
                AssertEqual("3-5-8-2-6", quickForm.CurrentShiftSequence, "advanced quick conversion uses five cipher values");
                AssertEqual("DGKFK", quickForm.ResultText, "quick conversion sequence up");
                quickForm.SetInputText("DGKFK");
                quickForm.ApplySequenceForTest("3-5-8-2-6", true);
                AssertEqual("ABCDE", quickForm.ResultText, "quick conversion sequence down");
                quickForm.SetInputText("ABCDE\r\nABCDE");
                quickForm.ApplySequenceForTest("3-5-8-2-6", false);
                AssertEqual("DGKFK\r\nDGKFK", quickForm.ResultText, "advanced quick conversion restarts cipher per line");
            }
            using (var compatibleQuickForm = new QuickConversionForm("1", true))
            {
                compatibleQuickForm.SetInputText("Ärger Öl Über ß");
                compatibleQuickForm.ApplyShiftForTest(0);
                AssertEqual("Aerger Oel Ueber ss", compatibleQuickForm.ResultText, "quick conversion compatibility mode");
            }

            string sample = "Name;Notiz\r\n\"Meyer, Anna\";\"Hallo; Welt\"\r\nBob;\"Zeile 1\r\nZeile 2\"\r\n";
            AssertEqual(';', CsvCodec.DetectDelimiter(sample), "delimiter detection");
            List<List<string>> parsed = CsvCodec.Parse(sample, ';');
            AssertEqual(3, parsed.Count, "record count");
            AssertEqual("Meyer, Anna", parsed[1][0], "quoted comma");
            AssertEqual("Hallo; Welt", parsed[1][1], "quoted delimiter");
            AssertEqual("Zeile 1\r\nZeile 2", parsed[2][1], "quoted newline");

            var source = new CsvDocument { Delimiter = ';', FirstRowIsHeader = true };
            source.Headers.AddRange(new[] { "A", "B", "C" });
            source.Rows.Add(new List<string> { "a1", "b1", "c1" });
            source.Rows.Add(new List<string> { "a2", "b2", "c2" });
            var filtered = CsvCodec.SelectColumns(source, new List<int> { 2, 0 });
            AssertEqual(2, filtered.Headers.Count, "selected column count");
            AssertEqual("C", filtered.Headers[0], "selected column order");
            AssertEqual("a2", filtered.Rows[1][1], "selected column values");

            var settings = AppSettings.Parse(new[] { "DefaultShiftSequence=3-5-8-2", "InterfaceStyle=Classic", "ConfirmBeforeShift=False", "CompatibilityMode=True", "CheckForUpdates=False", "AskForUpdateCheckOnStart=True", "LastUpdateCheckUtcTicks=123", "LatestKnownVersion=1.7.0", "LastNotifiedVersion=v1.7.0" });
            AssertEqual("3-5-8-2", settings.DefaultShiftSequence, "settings default shift sequence");
            AssertEqual("Classic", settings.InterfaceStyle, "settings classic interface");
            AssertEqual(false, settings.ConfirmBeforeShift, "settings confirmation flag");
            AssertEqual(true, settings.CompatibilityMode, "settings compatibility mode flag");
            AssertEqual(false, settings.CheckForUpdates, "settings update check flag");
            AssertEqual(true, settings.AskForUpdateCheckOnStart, "settings ask on start update flag");
            AssertEqual(123L, settings.LastUpdateCheckUtcTicks, "settings last update check");
            AssertEqual("1.7.0", settings.LatestKnownVersion, "settings latest known version");
            var boundedSettings = AppSettings.Parse(new[] { "DefaultShift=99" });
            AssertEqual("25", boundedSettings.DefaultShiftSequence, "legacy settings upper bound");
            AssertEqual("DefaultShiftSequence=3-5-8-2", settings.Serialize()[0], "settings sequence serialization");
            AssertEqual("InterfaceStyle=Classic", settings.Serialize()[1], "settings interface serialization");
            AssertEqual("CompatibilityMode=True", settings.Serialize()[3], "settings compatibility serialization");
            Version parsedUpdateVersion = UpdateChecker.ParseLatestVersion("{\"tag_name\":\"v1.7.0\"}");
            AssertEqual("1.7.0", UpdateChecker.DisplayVersion(parsedUpdateVersion), "github update version parsing");
            AssertEqual(true, UpdateChecker.IsNewer(parsedUpdateVersion, new Version(1, 6, 0, 0)), "new update detected");
            AssertEqual(false, UpdateChecker.IsNewer(parsedUpdateVersion, new Version(1, 7, 0, 0)), "equal update ignored");
            Version invalidUpdateVersion;
            AssertEqual(false, UpdateChecker.TryParseVersionText("v1.7.0/../../datei", out invalidUpdateVersion), "unsafe update tag rejected");
            var updateNow = new DateTime(2026, 8, 12, 8, 0, 0, DateTimeKind.Utc);
            var disabledUpdates = new AppSettings { CheckForUpdates = false, AskForUpdateCheckOnStart = true };
            AssertEqual(false, MainForm.ShouldAskForUpdateCheckOnStart(disabledUpdates), "disabled update prompt stays disabled");
            AssertEqual(false, MainForm.ShouldCheckForUpdatesOnStart(disabledUpdates, updateNow), "disabled automatic updates stay disabled");
            var promptedUpdates = new AppSettings { CheckForUpdates = true, AskForUpdateCheckOnStart = true, LastUpdateCheckUtcTicks = updateNow.Ticks };
            AssertEqual(true, MainForm.ShouldAskForUpdateCheckOnStart(promptedUpdates), "enabled update prompt appears on start");
            AssertEqual(false, MainForm.ShouldCheckForUpdatesOnStart(promptedUpdates, updateNow), "prompt mode prevents automatic request");
            var dailyUpdates = new AppSettings { CheckForUpdates = true, AskForUpdateCheckOnStart = false, LastUpdateCheckUtcTicks = updateNow.AddHours(-2).Ticks };
            AssertEqual(false, MainForm.ShouldCheckForUpdatesOnStart(dailyUpdates, updateNow), "daily update skips recent check");
            dailyUpdates.LastUpdateCheckUtcTicks = updateNow.AddHours(-25).Ticks;
            AssertEqual(true, MainForm.ShouldCheckForUpdatesOnStart(dailyUpdates, updateNow), "daily update runs after interval");
            string confirmationUp = MainForm.BuildConfirmationMessage(6, 12, false);
            AssertEqual(true, confirmationUp.Contains("Die ausgewählten Werte werden um 6 hochgezählt."), "confirmation count up text");
            AssertEqual(true, confirmationUp.Contains("Betroffene Zellen: 12"), "confirmation affected cells");
            string confirmationDown = MainForm.BuildConfirmationMessage(-4, 20, true);
            AssertEqual(true, confirmationDown.Contains("Alle Werte werden um 4 runtergezählt."), "confirmation count down text");
            string confirmationSequence = MainForm.BuildConfirmationMessage(new[] { 3, 5, 8, 2 }, false, 9, false);
            AssertEqual(true, confirmationSequence.Contains("mit der Zählfolge 3-5-8-2 hochgezählt"), "confirmation sequence text");

            var template = new SelectionTemplate("Vorname & Büro", new[] { "Vorname", "Büro|Nord" });
            string serializedTemplate = SelectionTemplateStore.SerializeLine(template);
            SelectionTemplate parsedTemplate;
            AssertEqual(true, SelectionTemplateStore.TryParseLine(serializedTemplate, out parsedTemplate), "template parsing");
            AssertEqual("Vorname & Büro", parsedTemplate.Name, "template name roundtrip");
            AssertEqual("Büro|Nord", parsedTemplate.Columns[1], "template column escaping");
            AssertEqual(false, SelectionTemplateStore.TryParseLine("ungültig", out parsedTemplate), "invalid template rejected");
            List<int> templateIndices = SelectionTemplateStore.FindColumnIndices(
                new[] { "Kundennummer", "VORNAME", "Nachname", "Büro" },
                new[] { "Vorname", "Büro", "Fehlt" });
            AssertEqual(2, templateIndices.Count, "template matching count");
            AssertEqual(1, templateIndices[0], "template matching ignores case");
            AssertEqual(3, templateIndices[1], "template matching office");

            using (var form = new MainForm(false, "Metro"))
            {
                form.LoadPreviewData();
                AssertEqual(true, form.HasColumnManagerButtonInLayout, "metro design contains column manager button");
                AssertEqual(true, form.HasCompatibilityModeControlInLayout, "metro design contains compatibility mode control");
                AssertEqual(true, form.UsesAdvancedShiftMode, "preview uses advanced shift mode");
                AssertEqual("3-5-8-2-6", form.CurrentShiftSequence, "advanced mode exposes five separate values");
                AssertEqual("CHIFFRE", form.ShiftValuesCaptionText, "advanced mode uses cipher caption");
                AssertEqual("Zählfolge beginnt je Wert neu", form.ShiftModeHintText, "advanced mode explains sequence restart");
                form.SetAdvancedShiftModeForTest(false);
                AssertEqual(false, form.UsesAdvancedShiftMode, "mode switch activates simple shift mode");
                AssertEqual("3", form.CurrentShiftSequence, "simple mode uses one value");
                AssertEqual("ZÄHLWERT", form.ShiftValuesCaptionText, "simple mode uses singular value caption");
                AssertEqual("Ein Zählwert gilt für alle Buchstaben", form.ShiftModeHintText, "simple mode explains single value behavior");
                form.SetAdvancedShiftModeForTest(true);
                AssertEqual("3-5-8-2-6", form.CurrentShiftSequence, "advanced values survive a mode roundtrip");
                AssertEqual(true, form.PasteCipherForTest("4;6;2;8;3"), "main view accepts pasted cipher");
                AssertEqual(true, form.UsesAdvancedShiftMode, "pasted cipher activates advanced main mode");
                AssertEqual("4-6-2-8-3", form.CurrentShiftSequence, "main view distributes pasted cipher");
                form.SelectOriginalColumn(1, false);
                AssertEqual(4, form.SelectedCellCount, "column header selects complete column");
                AssertEqual(true, form.IsColumnFullySelected(1), "first selected column is complete");
                form.SelectOriginalColumn(3, true);
                AssertEqual(8, form.SelectedCellCount, "ctrl column header adds column");
                AssertEqual(true, form.IsColumnFullySelected(3), "second selected column is complete");
                form.SelectOriginalColumn(1, true);
                AssertEqual(4, form.SelectedCellCount, "ctrl column header removes column");
                AssertEqual(false, form.IsColumnFullySelected(1), "removed column is no longer complete");
                form.ApplyFilterForTest(new RowFilter(3, new[] { "Hamburg" }, "M*"));
                AssertEqual(2, form.VisibleRowCount, "filter hides matching rows in main view");
                form.SwitchInterfaceStyleForTest("Classic");
                AssertEqual("Classic", form.ActiveInterfaceStyle, "design changes immediately to classic");
                AssertEqual(true, form.HasLoadedDocument, "live design change keeps imported document");
                AssertEqual(2, form.VisibleRowCount, "live design change keeps active filter");
                form.SwitchInterfaceStyleForTest("Metro");
                AssertEqual("Metro", form.ActiveInterfaceStyle, "design changes immediately back to metro");
                AssertEqual(2, form.VisibleRowCount, "second live design change keeps rows");
                form.SelectOriginalColumn(1, false);
                AssertEqual(2, form.SelectedCellCount, "column selection excludes hidden rows");
                form.ApplyFilterForTest(null);
                AssertEqual(4, form.VisibleRowCount, "clearing filter restores rows");
                form.ApplyColumnLayoutForTest(new[] { 3, 0, 1, 2 }, new[] { "Standort", "ID", "Vorname", "Nachname" });
                AssertEqual("Standort", form.CurrentHeaders[0], "column manager renames and reorders header");
                AssertEqual("Berlin", form.OriginalCellForTest(0, 0), "column manager moves original column content");
                AssertEqual("Ejznoq", form.ResultCellForTest(0, 0), "column manager preserves and moves converted result content");
                form.ClearDocumentForTest();
                AssertEqual(false, form.HasLoadedDocument, "clear removes current document");
            }
            using (var classicForm = new MainForm(false, "Classic"))
            {
                classicForm.LoadPreviewData();
                AssertEqual(4, classicForm.VisibleRowCount, "classic backup design remains functional");
                AssertEqual(true, classicForm.HasQuickConversionButtonInLayout, "classic design contains quick conversion button");
                AssertEqual(true, classicForm.HasColumnManagerButtonInLayout, "classic design contains column manager button");
                AssertEqual(true, classicForm.HasCompatibilityModeControlInLayout, "classic design contains compatibility mode control");
            }

            AssertEqual(20, ExportRowSelector.SelectRows(20, ExportRowMode.All, 0, null, null).Count, "export all rows");
            List<int> firstFive = ExportRowSelector.SelectRows(20, ExportRowMode.First, 5, null, null);
            AssertEqual(5, firstFive.Count, "export first five count");
            AssertEqual(4, firstFive[4], "export first five last index");
            List<int> limitedFirst = ExportRowSelector.SelectRows(3, ExportRowMode.First, 10, null, null);
            AssertEqual(3, limitedFirst.Count, "export first rows bounded by total");
            List<int> customExport = ExportRowSelector.SelectRows(20, ExportRowMode.Custom, 0, null, new[] { 12, 4, 12, -1, 30 });
            AssertEqual(2, customExport.Count, "custom export removes invalid duplicates");
            AssertEqual(4, customExport[0], "custom export sorted first row");
            AssertEqual(12, customExport[1], "custom export sorted second row");
            var originalExportRows = new List<IList<string>>
            {
                new List<string> { "Anna", "Berlin" },
                new List<string> { "Ben", "Hamburg" },
                new List<string> { "Clara", "Köln" }
            };
            var changedExportRows = new List<IList<string>>
            {
                new List<string> { "Anna", "Berlin" },
                new List<string> { "Cfo", "Hamburg" },
                new List<string> { "Clara", "Köln" }
            };
            List<int> detectedChanges = ExportRowSelector.FindChangedRows(originalExportRows, changedExportRows);
            AssertEqual(1, detectedChanges.Count, "changed export row count");
            AssertEqual(1, detectedChanges[0], "changed export row index");
            using (var exportNamesForm = new ExportColumnNamesForm(new[] { "Kundennummer", "Büro" }))
            {
                exportNamesForm.SetExportNameForTest(0, "CustomerId");
                AssertEqual("CustomerId", exportNamesForm.ExportHeaders[0], "export dialog keeps independent column name");
                int importedExportNames = exportNamesForm.ApplyExportPatternForTest(new[] { new ColumnMapping("Büro", "Office") });
                AssertEqual(1, importedExportNames, "export dialog imports export pattern");
                AssertEqual("Office", exportNamesForm.ExportHeaders[1], "export pattern changes matching export name");
                int importedHeaderNames = exportNamesForm.ApplyExportHeaderPatternForTest(new[] { "customer_id", "office" });
                AssertEqual(2, importedHeaderNames, "export dialog imports normal CSV header pattern");
                AssertEqual("customer_id", exportNamesForm.ExportHeaders[0], "normal CSV pattern maps headers by position");
                int expandedHeaderNames = exportNamesForm.ApplyExportHeaderPatternForTest(new[] { "import_id", "device_key", "device_name" });
                AssertEqual(3, expandedHeaderNames, "export dialog accepts a larger target schema");
                AssertEqual(3, exportNamesForm.ExportHeaders.Count, "larger target schema keeps every target column");
                AssertEqual(-1, exportNamesForm.ExportSourceIndices[0], "unmatched target column remains empty");
                exportNamesForm.MapSourceToTargetForTest(0, "device_key");
                AssertEqual(0, exportNamesForm.ExportSourceIndices[1], "source-centric mapping assigns source to selected target");
                AssertEqual(-1, exportNamesForm.ExportSourceIndices[0], "source-centric mapping leaves other target empty");
                int assignedOrderedColumns = exportNamesForm.ApplyOrderedExportPatternForTest(new[]
                {
                    new ColumnMapping(string.Empty, "import_id"),
                    new ColumnMapping("Büro", "device_name")
                });
                AssertEqual(1, assignedOrderedColumns, "ordered export pattern restores source assignments");
                AssertEqual(2, exportNamesForm.ExportHeaders.Count, "ordered export pattern restores target structure");
                AssertEqual(1, exportNamesForm.ExportSourceIndices[1], "ordered export pattern resolves source column");
            }
            List<List<string>> projectedExportRows = ExportProjection.ProjectRows(
                new[] { (IList<string>)new List<string> { "A", "B", "C" } },
                new[] { 2, -1, 0 });
            AssertEqual("C", projectedExportRows[0][0], "export projection reorders mapped source column");
            AssertEqual(string.Empty, projectedExportRows[0][1], "export projection creates empty target column");
            AssertEqual("A", projectedExportRows[0][2], "export projection maps final source column");
            using (var targetSearch = new ComboBox())
            {
                ExportColumnNamesForm.ConfigureTargetSearchComboBox(targetSearch);
                AssertEqual(ComboBoxStyle.DropDown, targetSearch.DropDownStyle, "mapping target search accepts typed input");
                AssertEqual(AutoCompleteMode.SuggestAppend, targetSearch.AutoCompleteMode, "mapping target search shows live suggestions");
                AssertEqual(AutoCompleteSource.ListItems, targetSearch.AutoCompleteSource, "mapping target search uses target columns");
            }
            var reorderedRows = ColumnLayoutEngine.ReorderRows(originalExportRows, new[] { 1, 0 });
            AssertEqual("Berlin", reorderedRows[0][0], "column layout engine moves complete column content");
            AssertEqual(1, ColumnLayoutEngine.RemapColumnIndex(new[] { 1, 0 }, 0), "column layout engine remaps filter column");
            string columnNameError;
            AssertEqual(true, ColumnNameRules.TryValidate(new[] { "ID", "Name" }, out columnNameError), "unique column names accepted");
            AssertEqual(false, ColumnNameRules.TryValidate(new[] { "Name", "name" }, out columnNameError), "duplicate column names rejected");
            var mappingItems = new List<ColumnLayoutItem>
            {
                new ColumnLayoutItem(0, "Vorname", "Vorname"),
                new ColumnLayoutItem(1, "Büro", "Büro")
            };
            int appliedMappings = ColumnMappingStore.Apply(mappingItems, new[] { new ColumnMapping("Vorname", "FirstName"), new ColumnMapping("Büro", "Office") });
            AssertEqual(2, appliedMappings, "column mappings applied");
            AssertEqual("FirstName", mappingItems[0].TargetName, "column mapping changes target name");

            string tempPath = Path.Combine(Path.GetTempPath(), "csv-buchstaben-test-" + Guid.NewGuid().ToString("N") + ".csv");
            try
            {
                var doc = new CsvDocument { Delimiter = ';', FirstRowIsHeader = true };
                doc.Headers.Add("Name");
                doc.Headers.Add("Notiz");
                var rows = new List<IList<string>>
                {
                    new List<string> { "Anna", "Hallo; \"Welt\"" },
                    new List<string> { "Bob", "Mehr\r\nZeilig" }
                };
                CsvCodec.Save(tempPath, doc, rows);
                var loaded = CsvCodec.Load(tempPath, true);
                AssertEqual("Hallo; \"Welt\"", loaded.Rows[0][1], "save/load quotes");
                AssertEqual("Mehr\r\nZeilig", loaded.Rows[1][1], "save/load newline");
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }

            string mappingPath = Path.Combine(Path.GetTempPath(), "panda-mapping-test-" + Guid.NewGuid().ToString("N") + ".csv");
            try
            {
                ColumnMappingStore.Save(mappingPath, new[] { new ColumnMapping("Vorname", "FirstName"), new ColumnMapping("Büro", "Office") });
                List<ColumnMapping> loadedMappings = ColumnMappingStore.Load(mappingPath);
                AssertEqual(2, loadedMappings.Count, "mapping template save and import count");
                AssertEqual("Office", loadedMappings[1].TargetName, "mapping template target roundtrip");
            }
            finally
            {
                if (File.Exists(mappingPath)) File.Delete(mappingPath);
            }

            string headerPatternPath = Path.Combine(Path.GetTempPath(), "panda-export-header-test-" + Guid.NewGuid().ToString("N") + ".csv");
            try
            {
                File.WriteAllText(headerPatternPath, "import_id;device_key;device_name\r\n1;A;Notebook\r\n", new System.Text.UTF8Encoding(true));
                ExportPatternDefinition loadedPattern = ColumnMappingStore.LoadExportPattern(headerPatternPath);
                AssertEqual(true, loadedPattern.IsHeaderTemplate, "normal CSV recognized as header export pattern");
                AssertEqual(3, loadedPattern.HeaderNames.Count, "normal CSV header export pattern count");
                AssertEqual("device_key", loadedPattern.HeaderNames[1], "normal CSV header export pattern value");
                AssertEqual(1, loadedPattern.DataRowCount, "normal CSV data rows are reported as ignored");
            }
            finally
            {
                if (File.Exists(headerPatternPath)) File.Delete(headerPatternPath);
            }

            string orderedPatternPath = Path.Combine(Path.GetTempPath(), "panda-ordered-export-test-" + Guid.NewGuid().ToString("N") + ".csv");
            try
            {
                ColumnMappingStore.SaveExportPattern(
                    orderedPatternPath,
                    new[] { "Kundennummer", "Büro" },
                    new[] { new ExportColumnDefinition(-1, "import_id"), new ExportColumnDefinition(1, "device_name") });
                ExportPatternDefinition loadedOrderedPattern = ColumnMappingStore.LoadExportPattern(orderedPatternPath);
                AssertEqual(true, loadedOrderedPattern.IsOrderedMapping, "saved export pattern keeps ordered schema marker");
                AssertEqual(2, loadedOrderedPattern.Mappings.Count, "saved export pattern keeps empty and mapped targets");
                AssertEqual(string.Empty, loadedOrderedPattern.Mappings[0].SourceName, "saved export pattern keeps empty source");
                AssertEqual("Büro", loadedOrderedPattern.Mappings[1].SourceName, "saved export pattern keeps assigned source");
            }
            finally
            {
                if (File.Exists(orderedPatternPath)) File.Delete(orderedPatternPath);
            }

            if (failures > 0)
            {
                Console.Error.WriteLine(failures + " test(s) failed.");
                Environment.Exit(1);
            }
            Console.WriteLine("All tests passed.");
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                failures++;
                Console.Error.WriteLine("FAIL " + name + ": expected [" + expected + "] actual [" + actual + "]");
            }
        }
    }
}
