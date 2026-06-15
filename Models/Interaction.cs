using System;

namespace ContactManager.Models
{
    public class Interaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Datum { get; set; } = DateTime.Now;
        public string Art { get; set; } = "Telefon"; // E-Mail, Meeting, Telefon, etc.
        public string Notiz { get; set; } = string.Empty;
        public string MitarbeiterName { get; set; } = string.Empty;
    }
}
