using System;

namespace ContactManager.Models
{
    public class Lernender : Mitarbeiter
    {
        public int Lehrjahre { get; set; } = 3;
        public int AktuellesLehrjahr { get; set; } = 1;
    }
}
