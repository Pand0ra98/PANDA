# PANDA

**P**seudonymisierung **a**lphanumerischer **N**utzdaten **d**urch **A**lphabetverschiebung

PANDA importiert CSV-Dateien, pseudonymisiert ausgewählte Werte durch eine umkehrbare Alphabetverschiebung mit einzelnen Zählwerten oder Zahlenfolgen und exportiert das Ergebnis wieder als CSV.

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
6. Zwischen **Einfach** und **Erweitert** wählen. Einfach verwendet einen Zählwert; Erweitert verwendet fünf getrennte Zählwerte. Anschließend **Hochzählen (+)** oder **Runterzählen (-)** verwenden.
7. Das Ergebnis rechts kontrollieren und mit **CSV exportieren** speichern. Im Exportdialog kann zwischen allen Zeilen, den ersten frei wählbaren `N` Zeilen, ausschließlich veränderten Zeilen oder einer eigenen Checkbox-Auswahl gewählt werden.

Mit **Leeren** wird die aktuelle CSV nach einer Sicherheitsabfrage aus PANDA entfernt, sodass eine neue Datei importiert werden kann. Die ursprüngliche Datei auf dem Datenträger wird dabei nicht gelöscht oder verändert.

## Zeilenfilter

Über **Filter** lassen sich Zeilen anhand einer ausgewählten Spalte ausblenden. PANDA zeigt dafür die vorhandenen Inhalte der Spalte als auswählbare Liste an. Mehrere exakte Werte und ein zusätzliches Wildcard-Muster können gemeinsam verwendet werden. Der kleine **?**-Button im Filterfenster erklärt die Platzhalter mit Beispielen.

- `*` steht für beliebig viele Zeichen, beispielsweise blendet `M*` Werte wie „Meyer“ oder „München“ aus.
- `?` steht für genau ein Zeichen, beispielsweise findet `B?b` sowohl „Bob“ als auch „Bab“.
- Groß- und Kleinschreibung wird beim Filtern ignoriert.

Der Filter blendet Treffer synchron in der Original- und Ergebnistabelle aus. Ausgeblendete Zeilen werden bei Umwandlungen nicht verändert, bleiben aber in den CSV-Daten erhalten und stehen im Exportdialog weiterhin zur Verfügung. Über **Filter aufheben** werden alle Zeilen wieder eingeblendet.

## Schnellumwandlung

Über **Schnellumwandlung** lässt sich Text auch ohne CSV-Import direkt bearbeiten. Text und eine Zählfolge eingeben und anschließend **Hochzählen (+)** oder **Runterzählen (-)** anklicken. Das Ergebnis erscheint in einem separaten Feld und kann in die Zwischenablage kopiert werden. Die Eingabe wird nicht gespeichert und verändert keine bereits importierten CSV-Daten.

## Einfache und erweiterte Verschlüsselung

Im Modus **Einfach** wird ein einzelner Wert zwischen 1 und 25 auf jeden Buchstaben angewendet. Im Modus **Erweitert** stehen unter **Chiffre** fünf optisch getrennte Zahlenfelder zur Verfügung. Die fünf Werte bilden eine wiederholte Zählfolge.

Bei `3-5-8-2-6` wird der erste Buchstabe um 3, der zweite um 5, der dritte um 8, der vierte um 2 und der fünfte um 6 verschoben. Danach beginnt die Folge wieder bei 3. Der Hinweis **Zählfolge beginnt je Wert neu** wird nur im erweiterten Modus angezeigt. Zahlen, Leerzeichen, Satzzeichen und Umlaute werden nicht verschoben und verbrauchen keinen Schritt der Folge. **Runterzählen** mit derselben Folge stellt den ursprünglichen Text wieder her.

Die Schnellumwandlung akzeptiert weiterhin frei eingegebene Folgen aus 1 bis 32 Werten. Als Trennzeichen können Bindestriche, Kommas, Semikolons oder Leerzeichen verwendet werden.

## Einstellungen

Über **Einstellungen** lässt sich ein einfacher Standardwert oder eine erweiterte Standardfolge aus genau fünf Werten festlegen. Diese wird benutzerbezogen gespeichert und beim nächsten Programmstart wiederhergestellt.

Standardmäßig zeigt PANDA vor jeder Umwandlung eine Bestätigung mit Richtung, gewähltem Zählwert und Anzahl der betroffenen Zellen. Diese Abfrage kann im Einstellungsfenster deaktiviert werden.

Die automatische Updateprüfung kann ebenfalls in den Einstellungen deaktiviert werden. Wenn sie aktiv ist, prüft PANDA höchstens einmal innerhalb von 24 Stunden. Mit der zusätzlichen Option **Bei jedem Programmstart vorher nachfragen** erscheint stattdessen bei jedem Start eine Ja/Nein-Abfrage; erst nach einer Bestätigung wird die Versionsprüfung ausgeführt.

Auswahlmarkierungen werden ausschließlich in der linken Originaltabelle dargestellt.

## Oberflächendesigns

PANDA 2.0 startet standardmäßig mit der aufgeräumten **Metro-Oberfläche**. In den Einstellungen kann alternativ **Klassisch (Backup)** gewählt werden. Dieses zweite Design entspricht dem bisherigen Aufbau aus PANDA 1.9. Das gewählte Design wird direkt nach dem Speichern angewendet; ein Programmneustart ist nicht nötig. Geladene Daten, Filter, Markierungen und Änderungen bleiben beim Wechsel erhalten.

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
