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

            var settings = AppSettings.Parse(new[] { "DefaultShift=6", "ConfirmBeforeShift=False" });
            AssertEqual(6, settings.DefaultShift, "settings default shift");
            AssertEqual(false, settings.ConfirmBeforeShift, "settings confirmation flag");
            var boundedSettings = AppSettings.Parse(new[] { "DefaultShift=99" });
            AssertEqual(25, boundedSettings.DefaultShift, "settings upper bound");
            AssertEqual("DefaultShift=6", settings.Serialize()[0], "settings serialization");
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
            }

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
