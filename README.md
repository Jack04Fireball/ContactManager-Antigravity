# Contact Manager (Antigravity Edition)

Ein voll funktionsfähiges Windows-Forms-Semesterprojekt in C# .NET 8.0 zur effizienten Verwaltung von Kunden- und Mitarbeiterdaten (inkl. Lernenden) für den Schweizer Markt.

---

## Inhaltsverzeichnis
1. [Funktionsumfang](#funktionsumfang)
2. [Voraussetzungen](#voraussetzungen)
3. [Installation und Ausführung](#installation-und-ausführung)
4. [Login-Daten](#login-daten)
5. [Datenstruktur und Persistenz](#datenstruktur-und-persistenz)

---

## Funktionsumfang

- **Sicheres Login:** Rollenbasierte Anmeldung für Administratoren und Standardbenutzer.
- **Dashboard:** Echtzeit-Kennzahlen (Anzahl Kunden/Mitarbeiter), automatische Erinnerung an Geburtstage in den nächsten 30 Tagen und Anzeige der letzten Kundenkontakte.
- **Kundenverwaltung (CRUD):** 
  - Erstellen, Lesen, Bearbeiten und Löschen von Kunden.
  - Dokumentation und Historie aller Kontakte (E-Mail, Telefon, Meeting) pro Kunde.
- **Mitarbeiterverwaltung (CRUD):**
  - Getrennte Erfassung von regulären Angestellten und Lernenden (Auszubildenden) mit dynamischer Anzeige lehrzeitenspezifischer Felder.
  - Automatische Generierung der Personalnummer (z. B. M10001, M10002...).
  - Validierung der Schweizer AHV-Nummer (`756.XXXX.XXXX.XX`).
- **Mutationshistorie:** Lückenlose Protokollierung aller Änderungen an den Stammdaten (altes Feld, neuer Wert, Benutzer und Zeitstempel).
- **CSV-Schnittstelle:** Importieren von Kontaktdaten aus einer CSV-Datei mit Echtzeit-Vorschau und Validierungsprüfung.

---

## Voraussetzungen

Um die Anwendung auszuführen, benötigen Sie:
- Ein Windows-Betriebssystem (da Windows Forms auf Windows-APIs basiert).
- Das **.NET 8.0 Desktop Runtime** oder **.NET 8.0 SDK** (kann hier kostenlos heruntergeladen werden: [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)).

---

## Installation und Ausführung

### Option A: Herunterladen der kompilierten Anwendung (Empfohlen für Endbenutzer)
1. Laden Sie die ZIP-Datei des neuesten Releases von GitHub herunter.
2. Entpacken Sie die ZIP-Datei in einen beliebigen Ordner auf Ihrem PC.
3. Starten Sie die Anwendung durch Doppelklick auf die Datei `ContactManager.exe`.

### Option B: Ausführen über den Quellcode (Für Entwickler)
1. Klonen Sie das Repository:
   ```bash
   git clone https://github.com/Jack04Fireball/ContactManager-Antigravity.git
   ```
2. Wechseln Sie in das Projektverzeichnis:
   ```bash
   cd ContactManager-Antigravity
   ```
3. Führen Sie die App über die Eingabeaufforderung/PowerShell aus:
   ```bash
   dotnet run --framework net8.0-windows
   ```

---

## Login-Daten

Die Anwendung startet mit einem Anmeldefenster. Folgende Standard-Accounts sind vorkonfiguriert:

| Benutzername | Passwort | Rolle |
| :--- | :--- | :--- |
| **admin** | admin | Administrator |
| **user** | user | Standardbenutzer |

---

## Datenstruktur und Persistenz

- Die Daten werden automatisch bei jedem Speichervorgang in einer lokalen Datei namens `contacts.json` im Verzeichnis der ausführbaren Datei abgelegt.
- Das Projekt verwendet polymorphe JSON-Serialisierung zur sauberen Abbildung der Vererbungshierarchie:
  `Person` (abstrakt) &rarr; `Kunde` / `Mitarbeiter` &rarr; `Lernender`.
- Eine Beispieldatei für den CSV-Import befindet sich unter `data/import_samples.csv`.
