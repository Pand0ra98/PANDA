# PANDA

**P**seudonymisierung **a**lphanumerischer **N**utzdaten **d**urch **A**lphabetverschiebung

PANDA importiert CSV-Dateien, pseudonymisiert ausgewählte Werte durch eine umkehrbare Alphabetverschiebung und exportiert das Ergebnis wieder als CSV.

## Fest installieren

`PANDA-Setup.exe` doppelt anklicken. Der Installer:

- installiert PANDA für das aktuelle Windows-Benutzerkonto,
- kann Desktop- und Startmenü-Verknüpfungen anlegen,
- trägt PANDA unter **Windows → Installierte Apps** ein,
- installiert einen vollständigen Uninstaller.

Administratorrechte sind bei der Standardinstallation nicht erforderlich.

## Ohne Installation starten

Alternativ `PANDA-Portable.exe` doppelt anklicken. Diese Einzeldatei benötigt keine zusätzlichen Programmdateien.

## Bedienung

1. **Importieren** anklicken und eine CSV-Datei auswählen.
2. Festlegen, ob die erste Zeile Überschriften enthält.
3. Spalten an- oder abwählen. Die Dateivorschau aktualisiert sich sofort.
4. Im Importfenster **Importieren** anklicken.
5. Links einzelne oder mehrere Zellen markieren. Ein Klick auf eine Spaltenüberschrift markiert die komplette Spalte; mit **Strg + Klick** lassen sich weitere Spalten ergänzen oder entfernen. Über die Checkbox neben einer Zeilennummer lässt sich die komplette Zeile auswählen. Alternativ **Alle Einträge** wählen.
6. Schrittweite festlegen und **Hochzählen (+)** oder **Runterzählen (-)** verwenden.
7. Das Ergebnis rechts kontrollieren und mit **CSV exportieren** speichern. Im Exportdialog kann zwischen allen Zeilen, den ersten frei wählbaren `N` Zeilen, ausschließlich veränderten Zeilen oder einer eigenen Checkbox-Auswahl gewählt werden.

## Einstellungen

Über **Einstellungen** lässt sich der Standard-Zählwert von 1 bis 25 festlegen. Dieser Wert wird benutzerbezogen gespeichert und beim nächsten Programmstart wiederhergestellt.

Standardmäßig zeigt PANDA vor jeder Umwandlung eine Bestätigung mit Richtung, gewähltem Zählwert und Anzahl der betroffenen Zellen. Diese Abfrage kann im Einstellungsfenster deaktiviert werden.

Die automatische Updateprüfung kann ebenfalls in den Einstellungen deaktiviert werden. Wenn sie aktiv ist, fragt PANDA höchstens einmal innerhalb von 24 Stunden die aktuelle Versionsnummer ab.

Auswahlmarkierungen werden ausschließlich in der linken Originaltabelle dargestellt.

## Auswahlvorlagen

Über **Vorlagen** lassen sich häufig verwendete Spaltenkombinationen speichern, zum Beispiel „Vorname und Büro“. Beim Öffnen des Dialogs sind die aktuell markierten Spalten bereits vorausgewählt. Gespeicherte Vorlagen können angewendet, überschrieben oder gelöscht werden und stehen nach dem nächsten Programmstart weiterhin zur Verfügung.

Vorlagen werden anhand der Spaltenüberschriften angewendet. Fehlt eine gespeicherte Spalte in einer später importierten CSV, markiert PANDA die vorhandenen Spalten und weist in der Statuszeile auf fehlende Spalten hin.

## Zeilenauswahl beim Export

Vor dem Speichern zeigt PANDA einen eigenen Exportdialog. Dort stehen vier Möglichkeiten zur Verfügung:

- **Alle Zeilen** exportieren,
- nur die **ersten N Zeilen** exportieren,
- automatisch nur **veränderte Zeilen** exportieren,
- beliebige Zeilen über eine unabhängige **Checkbox-Auswahl** markieren.

Die Checkboxen im Exportdialog beeinflussen die Markierungen für die Pseudonymisierung nicht. Bereits über die Zeilencheckboxen im Hauptfenster ausgewählte Zeilen werden als Vorauswahl übernommen. Ist dort nichts ausgewählt, werden erkannte veränderte Zeilen vorausgewählt.

## Updateprüfung

Über **Updates prüfen** kann jederzeit manuell nach einer neuen PANDA-Version gesucht werden. PANDA verwendet dafür ausschließlich die fest hinterlegte GitHub-API-Adresse des offiziellen Repositories und wertet nur einen numerischen Versions-Tag aus.

Die Funktion lädt und startet keine Dateien. Wenn eine neuere Version verfügbar ist, kann ausschließlich die fest im Programm hinterlegte offizielle GitHub-Release-Seite im Standardbrowser geöffnet werden. Von GitHub gelieferte Download- oder Weiterleitungsadressen werden nicht übernommen.

Die Buchstaben `A-Z` und `a-z` werden zyklisch verschoben. Beispiel: `Z + 1 = A` und `a - 1 = z`. Zahlen, Leerzeichen, Satzzeichen und Umlaute bleiben unverändert.

## Dateien

- `PANDA-Setup.exe` – Installer inklusive Uninstaller
- `PANDA-Portable.exe` – portable Einzeldatei
- `PANDA.ico` / `PANDA-icon-final.png` – Programmsymbol
- `Beispiel.csv` – Beispieldaten
- `Program.cs` – Programmquellcode
- `Installer.cs` / `Uninstaller.cs` – Setupquellcode
- `build.ps1` / `Tests.cs` – Build und automatische Tests

## Lizenz

PANDA wird unter der [MIT-Lizenz](LICENSE) veröffentlicht. Nutzung, Veränderung und Weitergabe sind erlaubt, sofern der Lizenz- und Urheberhinweis erhalten bleibt.
