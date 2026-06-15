using System;

namespace ContactManager.Models
{
    public class MutationLogEntry
    {
        public DateTime Zeitpunkt { get; set; } = DateTime.Now;
        public string FeldName { get; set; } = string.Empty;
        public string AlterWert { get; set; } = string.Empty;
        public string NeuerWert { get; set; } = string.Empty;
        public string Benutzer { get; set; } = string.Empty;
    }
}
