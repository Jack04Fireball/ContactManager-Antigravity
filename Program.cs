using System;
using System.Windows.Forms;
using ContactManager.Forms;
using ContactManager.Services;

namespace ContactManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var authService = new AuthService();
            var dataManager = new DataManager();

            bool runApplication = true;
            while (runApplication)
            {
                using (var loginForm = new LoginForm(authService))
                {
                    var loginResult = loginForm.ShowDialog();
                    if (loginResult == DialogResult.OK)
                    {
                        string loggedInUser = loginForm.LoggedInUser;

                        using (var mainForm = new MainForm(dataManager, authService, loggedInUser))
                        {
                            var mainResult = mainForm.ShowDialog();
                            if (mainResult != DialogResult.Retry)
                            {
                                runApplication = false; // Normal exit
                            }
                        }
                    }
                    else
                    {
                        runApplication = false; // User closed login form
                    }
                }
            }
        }
    }
}
