using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ContactManager.Models
{
    [JsonDerivedType(typeof(Kunde), typeDiscriminator: "Kunde")]
    [JsonDerivedType(typeof(Mitarbeiter), typeDiscriminator: "Mitarbeiter")]
    [JsonDerivedType(typeof(Lernender), typeDiscriminator: "Lernender")]
    public abstract class Person
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Vorname { get; set; } = string.Empty;
        public string Nachname { get; set; } = string.Empty;
        public DateTime Geburtsdatum { get; set; } = DateTime.Today.AddYears(-30);
        public string Mobiltelefonnummer { get; set; } = string.Empty;
        public string TelefonnummerGeschäft { get; set; } = string.Empty;
        public string EmailAdresse { get; set; } = string.Empty;
        
        // Status: aktiv = true, passiv = false
        public bool IsActive { get; set; } = true;
        
        // Mutationshistorie
        public List<MutationLogEntry> MutationsHistorie { get; set; } = new List<MutationLogEntry>();

        [JsonIgnore]
        public string NameDisplay => $"{Nachname}, {Vorname}";

        [JsonIgnore]
        public string TypDisplay => this switch
        {
            Lernender => "Lernender",
            Mitarbeiter => "Mitarbeiter",
            Kunde => "Kunde",
            _ => "Unbekannt"
        };
    }
}
