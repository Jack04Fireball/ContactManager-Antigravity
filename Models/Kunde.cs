using System;
using System.Collections.Generic;

namespace ContactManager.Models
{
    public class Kunde : Person
    {
        public string Anrede { get; set; } = "Herr";
        public string Geschlecht { get; set; } = "Männlich";
        public string Titel { get; set; } = string.Empty;
        
        // Kundenkontakte/Interaktionen Historie
        public List<Interaction> InteraktionsHistorie { get; set; } = new List<Interaction>();
    }
}
