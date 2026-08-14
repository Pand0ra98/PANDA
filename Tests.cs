using System;
using System.Collections.Generic;
using System.IO;

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

            using (var quickForm = new QuickConversionForm(6))
            {
                quickForm.SetInputText("Abc XYZ 123");
                quickForm.ApplyShiftForTest(2);
                AssertEqual("Cde ZAB 123", quickForm.ResultText, "quick conversion up");
                quickForm.ApplyShiftForTest(-2);
                AssertEqual("Yza VWX 123", quickForm.ResultText, "quick conversion down");
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

            var settings = AppSettings.Parse(new[] { "DefaultShift=6", "ConfirmBeforeShift=False", "CheckForUpdates=False", "AskForUpdateCheckOnStart=True", "LastUpdateCheckUtcTicks=123", "LatestKnownVersion=1.7.0", "LastNotifiedVersion=v1.7.0" });
            AssertEqual(6, settings.DefaultShift, "settings default shift");
            AssertEqual(false, settings.ConfirmBeforeShift, "settings confirmation flag");
            AssertEqual(false, settings.CheckForUpdates, "settings update check flag");
            AssertEqual(true, settings.AskForUpdateCheckOnStart, "settings ask on start update flag");
            AssertEqual(123L, settings.LastUpdateCheckUtcTicks, "settings last update check");
            AssertEqual("1.7.0", settings.LatestKnownVersion, "settings latest known version");
            var boundedSettings = AppSettings.Parse(new[] { "DefaultShift=99" });
            AssertEqual(25, boundedSettings.DefaultShift, "settings upper bound");
            AssertEqual("DefaultShift=6", settings.Serialize()[0], "settings serialization");
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

            using (var form = new MainForm())
            {
                form.LoadPreviewData();
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
                form.SelectOriginalColumn(1, false);
                AssertEqual(2, form.SelectedCellCount, "column selection excludes hidden rows");
                form.ApplyFilterForTest(null);
                AssertEqual(4, form.VisibleRowCount, "clearing filter restores rows");
                form.ClearDocumentForTest();
                AssertEqual(false, form.HasLoadedDocument, "clear removes current document");
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
