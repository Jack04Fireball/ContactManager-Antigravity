using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ContactManager.Models;

namespace ContactManager.Services
{
    public class CsvImporter
    {
        public static (List<Person> Imported, List<string> Errors) ImportContacts(string filePath, DataManager dataManager)
        {
            var imported = new List<Person>();
            var errors = new List<string>();

            if (!File.Exists(filePath))
            {
                errors.Add("Die angegebene CSV-Datei existiert nicht.");
                return (imported, errors);
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length <= 1)
                {
                    errors.Add("Die Datei ist leer oder enthält keine Datenzeilen.");
                    return (imported, errors);
                }

                // Read header and determine delimiter
                string header = lines[0];
                char delimiter = ';';
                if (header.Contains(","))
                {
                    delimiter = ',';
                }

                var headers = header.Split(delimiter);
                var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < headers.Length; i++)
                {
                    headerMap[headers[i].Trim()] = i;
                }

                // Required column check
                if (!headerMap.ContainsKey("Typ") || !headerMap.ContainsKey("Vorname") || !headerMap.ContainsKey("Nachname"))
                {
                    errors.Add("Ungültiges CSV-Format. Spalten 'Typ', 'Vorname' und 'Nachname' sind zwingend erforderlich.");
                    return (imported, errors);
                }

                for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
                {
                    string line = lines[lineIndex];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(delimiter);
                    
                    // Helper to get value safely
                    string GetVal(string columnName)
                    {
                        if (headerMap.TryGetValue(columnName, out int index) && index < parts.Length)
                        {
                            return parts[index].Trim();
                        }
                        return string.Empty;
                    }

                    string typeStr = GetVal("Typ");
                    string vorname = GetVal("Vorname");
                    string nachname = GetVal("Nachname");

                    if (string.IsNullOrWhiteSpace(typeStr) || string.IsNullOrWhiteSpace(vorname) || string.IsNullOrWhiteSpace(nachname))
                    {
                        errors.Add($"Zeile {lineIndex + 1}: Typ, Vorname und Nachname dürfen nicht leer sein.");
                        continue;
                    }

                    Person person;
                    if (typeStr.Equals("Kunde", StringComparison.OrdinalIgnoreCase))
                    {
                        var k = new Kunde
                        {
                            Anrede = GetVal("Anrede"),
                            Geschlecht = GetVal("Geschlecht"),
                            Titel = GetVal("Titel")
                        };
                        if (string.IsNullOrWhiteSpace(k.Anrede)) k.Anrede = "Herr";
                        if (string.IsNullOrWhiteSpace(k.Geschlecht)) k.Geschlecht = "Männlich";
                        person = k;
                    }
                    else if (typeStr.Equals("Mitarbeiter", StringComparison.OrdinalIgnoreCase))
                    {
                        var m = new Mitarbeiter();
                        PopulateMitarbeiterFields(m, GetVal, dataManager, lineIndex, errors);
                        person = m;
                    }
                    else if (typeStr.Equals("Lernender", StringComparison.OrdinalIgnoreCase))
                    {
                        var l = new Lernender();
                        PopulateMitarbeiterFields(l, GetVal, dataManager, lineIndex, errors);

                        if (int.TryParse(GetVal("Lehrjahre"), out int lehrjahre))
                        {
                            l.Lehrjahre = lehrjahre;
                        }
                        if (int.TryParse(GetVal("AktuellesLehrjahr"), out int aktLehrjahr))
                        {
                            l.AktuellesLehrjahr = aktLehrjahr;
                        }
                        person = l;
                    }
                    else
                    {
                        errors.Add($"Zeile {lineIndex + 1}: Unbekannter Typ '{typeStr}'. Erlaubt sind 'Kunde', 'Mitarbeiter', 'Lernender'.");
                        continue;
                    }

                    // Populate common fields
                    person.Vorname = vorname;
                    person.Nachname = nachname;
                    
                    if (DateTime.TryParse(GetVal("Geburtsdatum"), out DateTime bday))
                    {
                        person.Geburtsdatum = bday;
                    }
                    
                    person.Mobiltelefonnummer = GetVal("Mobiltelefonnummer");
                    person.TelefonnummerGeschäft = GetVal("TelefonnummerGeschäft");
                    person.EmailAdresse = GetVal("EmailAdresse");

                    string activeStr = GetVal("IsActive");
                    if (bool.TryParse(activeStr, out bool active))
                    {
                        person.IsActive = active;
                    }
                    else if (activeStr.Equals("aktiv", StringComparison.OrdinalIgnoreCase))
                    {
                        person.IsActive = true;
                    }
                    else if (activeStr.Equals("passiv", StringComparison.OrdinalIgnoreCase))
                    {
                        person.IsActive = false;
                    }
                    else
                    {
                        person.IsActive = true; // default
                    }

                    imported.Add(person);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Fehler beim Einlesen der CSV-Datei: {ex.Message}");
            }

            return (imported, errors);
        }

        private static void PopulateMitarbeiterFields(Mitarbeiter m, Func<string, string> getVal, DataManager dataManager, int lineIndex, List<string> errors)
        {
            string mitId = getVal("MitarbeiterNummer");
            m.MitarbeiterNummer = string.IsNullOrWhiteSpace(mitId) ? dataManager.GetNextMitarbeiterNummer() : mitId;

            m.Abteilung = getVal("Abteilung");
            m.AhvNummer = getVal("AhvNummer");
            
            // Validate AHV
            if (!string.IsNullOrWhiteSpace(m.AhvNummer) && !Mitarbeiter.ValidateAhvNummer(m.AhvNummer))
            {
                errors.Add($"Zeile {lineIndex + 1}: Ungültiges AHV-Nummernformat '{m.AhvNummer}' (erwartet: 756.XXXX.XXXX.XX).");
            }

            m.Wohnort = getVal("Wohnort");
            m.Nationalität = getVal("Nationalität");
            m.Adresse = getVal("Adresse");
            m.Postleitzahl = getVal("Postleitzahl");

            if (DateTime.TryParse(getVal("Eintrittsdatum"), out DateTime entryDate))
            {
                m.Eintrittsdatum = entryDate;
            }

            string exitStr = getVal("Austrittsdatum");
            if (!string.IsNullOrWhiteSpace(exitStr) && DateTime.TryParse(exitStr, out DateTime exitDate))
            {
                m.Austrittsdatum = exitDate;
            }

            if (double.TryParse(getVal("Beschäftigungsgrad"), out double pct))
            {
                m.Beschäftigungsgrad = pct;
            }

            m.Rolle = getVal("Rolle");

            if (int.TryParse(getVal("Kaderstufe"), out int level))
            {
                m.Kaderstufe = Math.Clamp(level, 0, 5);
            }

            m.Geschäftsadresse = getVal("Geschäftsadresse");
        }
    }
}
