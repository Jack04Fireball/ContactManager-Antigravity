using System;
using System.Collections.Generic;

namespace ContactManager.Services
{
    public class AuthService
    {
        private readonly Dictionary<string, string> _credentials = new()
        {
            { "admin", "admin" },
            { "user", "user" }
        };

        public bool ValidateCredentials(string username, string password, out string? errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Benutzername und Passwort dürfen nicht leer sein.";
                return false;
            }

            string lowerUser = username.Trim().ToLower();
            if (_credentials.TryGetValue(lowerUser, out string? correctPassword))
            {
                if (correctPassword == password)
                {
                    return true;
                }
            }

            errorMessage = "Ungültiger Benutzername oder Passwort.";
            return false;
        }
    }
}
