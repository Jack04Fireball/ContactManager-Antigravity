using System;

namespace ContactManager.Models
{
    public class Mitarbeiter : Person
    {
        public string MitarbeiterNummer { get; set; } = string.Empty;
        public string Abteilung { get; set; } = string.Empty;
        public string AhvNummer { get; set; } = string.Empty;
        public string Wohnort { get; set; } = string.Empty;
        public string Nationalität { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Postleitzahl { get; set; } = string.Empty;
        public DateTime Eintrittsdatum { get; set; } = DateTime.Today;
        public DateTime? Austrittsdatum { get; set; } = null;
        public double Beschäftigungsgrad { get; set; } = 100.0;
        public string Rolle { get; set; } = string.Empty;
        public int Kaderstufe { get; set; } = 0; // 0-5
        public string Geschäftsadresse { get; set; } = string.Empty;

        // Validates the Swiss AHV-Nummer (format: 756.XXXX.XXXX.XX)
        public static bool ValidateAhvNummer(string ahv)
        {
            if (string.IsNullOrWhiteSpace(ahv)) return false;
            // Matches 756.XXXX.XXXX.XX
            var regex = new System.Text.RegularExpressions.Regex(@"^756\.\d{4}\.\d{4}\.\d{2}$");
            return regex.IsMatch(ahv);
        }
    }
}
