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
5. Links einzelne oder mehrere Zellen markieren. Über die Checkbox neben einer Zeilennummer lässt sich die komplette Zeile auswählen. Alternativ **Alle Einträge** wählen.
6. Schrittweite festlegen und **Hochzählen (+)** oder **Runterzählen (-)** verwenden.
7. Das Ergebnis rechts kontrollieren und mit **CSV exportieren** speichern.

## Einstellungen

Über **Einstellungen** lässt sich der Standard-Zählwert von 1 bis 25 festlegen. Dieser Wert wird benutzerbezogen gespeichert und beim nächsten Programmstart wiederhergestellt.

Standardmäßig zeigt PANDA vor jeder Umwandlung eine Bestätigung mit Richtung, gewähltem Zählwert und Anzahl der betroffenen Zellen. Diese Abfrage kann im Einstellungsfenster deaktiviert werden.

Auswahlmarkierungen werden ausschließlich in der linken Originaltabelle dargestellt.

Die Buchstaben `A-Z` und `a-z` werden zyklisch verschoben. Beispiel: `Z + 1 = A` und `a - 1 = z`. Zahlen, Leerzeichen, Satzzeichen und Umlaute bleiben unverändert.

## Dateien

- `PANDA-Setup.exe` – Installer inklusive Uninstaller
- `PANDA-Portable.exe` – portable Einzeldatei
- `PANDA.ico` / `PANDA-icon-final.png` – Programmsymbol
- `Beispiel.csv` – Beispieldaten
- `Program.cs` – Programmquellcode
- `Installer.cs` / `Uninstaller.cs` – Setupquellcode
- `build.ps1` / `Tests.cs` – Build und automatische Tests
