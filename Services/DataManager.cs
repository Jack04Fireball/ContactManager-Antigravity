using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContactManager.Models;

namespace ContactManager.Services
{
    public class DataManager
    {
        private readonly string _filePath;
        public List<Person> Contacts { get; private set; } = new List<Person>();

        public DataManager(string? filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "contacts.json");
            LoadData();
        }

        // Saves contacts polymorphically in JSON format
        public void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(Contacts, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        // Loads contacts polymorphically from JSON
        public void LoadData()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Contacts = new List<Person>();
                    GenerateMockData();
                    SaveData();
                    return;
                }

                string json = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions();
                var loaded = JsonSerializer.Deserialize<List<Person>>(json, options);
                Contacts = loaded ?? new List<Person>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                Contacts = new List<Person>();
            }
        }

        // Generates an automatic employee number (format: M10001, M10002...)
        public string GetNextMitarbeiterNummer()
        {
            int maxNum = 10000;
            foreach (var p in Contacts.OfType<Mitarbeiter>())
            {
                if (string.IsNullOrEmpty(p.MitarbeiterNummer)) continue;
                
                string numPart = p.MitarbeiterNummer.StartsWith("M") 
                    ? p.MitarbeiterNummer.Substring(1) 
                    : p.MitarbeiterNummer;

                if (int.TryParse(numPart, out int parsed))
                {
                    if (parsed > maxNum)
                    {
                        maxNum = parsed;
                    }
                }
            }
            return $"M{maxNum + 1}";
        }

        // Performs a search across contacts
        public List<Person> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Contacts;
            }

            query = query.Trim().ToLower();
            return Contacts.Where(p =>
                p.Vorname.ToLower().Contains(query) ||
                p.Nachname.ToLower().Contains(query) ||
                p.Geburtsdatum.ToString("dd.MM.yyyy").Contains(query) ||
                p.Geburtsdatum.ToString("yyyy-MM-dd").Contains(query) ||
                p.TypDisplay.ToLower().Contains(query) ||
                (p is Mitarbeiter m && (
                    m.MitarbeiterNummer.ToLower().Contains(query) ||
                    m.Abteilung.ToLower().Contains(query) ||
                    m.Rolle.ToLower().Contains(query)
                ))
            ).ToList();
        }

        // Adds a log entry of mutations when a contact is edited
        public void LogMutation(Person person, string fieldName, string oldVal, string newVal, string user)
        {
            if (oldVal == newVal) return;
            person.MutationsHistorie.Add(new MutationLogEntry
            {
                Zeitpunkt = DateTime.Now,
                FeldName = fieldName,
                AlterWert = oldVal,
                NeuerWert = newVal,
                Benutzer = user
            });
        }

        private void GenerateMockData()
        {
            // Add initial mock data if the file does not exist
            var k1 = new Kunde
            {
                Vorname = "Hans",
                Nachname = "Meier",
                Geburtsdatum = new DateTime(1985, 4, 12),
                Mobiltelefonnummer = "+41 79 123 45 67",
                TelefonnummerGeschäft = "+41 44 987 65 43",
                EmailAdresse = "hans.meier@gmail.com",
                Anrede = "Herr",
                Geschlecht = "Männlich",
                Titel = "Dr. med.",
                IsActive = true,
                InteraktionsHistorie = new List<Interaction>
                {
                    new Interaction { Datum = DateTime.Today.AddDays(-5), Art = "Telefon", Notiz = "Anfrage wegen Offerte für neue IT-Systeme.", MitarbeiterName = "Sarah Keller" },
                    new Interaction { Datum = DateTime.Today.AddDays(-2), Art = "E-Mail", Notiz = "Offerte zugestellt.", MitarbeiterName = "Sarah Keller" }
                }
            };

            var k2 = new Kunde
            {
                Vorname = "Anna",
                Nachname = "Müller",
                Geburtsdatum = new DateTime(1992, 11, 23),
                Mobiltelefonnummer = "+41 78 555 44 33",
                TelefonnummerGeschäft = "+41 31 333 22 11",
                EmailAdresse = "anna.mueller@gmx.ch",
                Anrede = "Frau",
                Geschlecht = "Weiblich",
                Titel = "",
                IsActive = false
            };

            var m1 = new Mitarbeiter
            {
                MitarbeiterNummer = "M10001",
                Vorname = "Sarah",
                Nachname = "Keller",
                Geburtsdatum = new DateTime(1980, 8, 15),
                Mobiltelefonnummer = "+41 79 999 88 77",
                TelefonnummerGeschäft = "+41 44 111 22 33",
                EmailAdresse = "s.keller@firma.ch",
                Abteilung = "Verkauf",
                AhvNummer = "756.1234.5678.90",
                Wohnort = "Zürich",
                Nationalität = "Schweiz",
                Adresse = "Bahnhofstrasse 12",
                Postleitzahl = "8001",
                Eintrittsdatum = new DateTime(2015, 1, 1),
                Beschäftigungsgrad = 100.0,
                Rolle = "Verkaufsleiterin",
                Kaderstufe = 3,
                Geschäftsadresse = "Firma AG, Hauptstrasse 1, 8000 Zürich",
                IsActive = true
            };

            var l1 = new Lernender
            {
                MitarbeiterNummer = "M10002",
                Vorname = "David",
                Nachname = "Brunner",
                Geburtsdatum = new DateTime(2007, 2, 5),
                Mobiltelefonnummer = "+41 76 444 33 22",
                TelefonnummerGeschäft = "+41 44 111 22 34",
                EmailAdresse = "d.brunner@firma.ch",
                Abteilung = "IT Support",
                AhvNummer = "756.9876.5432.10",
                Wohnort = "Winterthur",
                Nationalität = "Schweiz",
                Adresse = "Lindenweg 4",
                Postleitzahl = "8400",
                Eintrittsdatum = new DateTime(2025, 8, 1),
                Beschäftigungsgrad = 100.0,
                Rolle = "Lernender Informatiker EFZ",
                Kaderstufe = 0,
                Geschäftsadresse = "Firma AG, Hauptstrasse 1, 8000 Zürich",
                Lehrjahre = 4,
                AktuellesLehrjahr = 1,
                IsActive = true
            };

            Contacts.Add(k1);
            Contacts.Add(k2);
            Contacts.Add(m1);
            Contacts.Add(l1);
        }
    }
}
