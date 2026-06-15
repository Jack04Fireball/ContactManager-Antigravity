using System;
using System.Drawing;
using System.Windows.Forms;
using ContactManager.Services;

namespace ContactManager.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService;
        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;
        private Label _lblError = null!;

        public string LoggedInUser { get; private set; } = string.Empty;

        public LoginForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Anmeldung - Contact Manager";
            this.Size = new Size(400, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 246, 250);

            // Title Label
            var lblTitle = new Label
            {
                Text = "Contact Manager Login",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 94, 40), // Premium Orange
                Location = new Point(20, 15),
                Size = new Size(360, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Username Label & TextBox
            var lblUsername = new Label
            {
                Text = "Benutzername:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 42, 52),
                Location = new Point(40, 65),
                Size = new Size(120, 20)
            };
            this.Controls.Add(lblUsername);

            _txtUsername = new TextBox
            {
                Location = new Point(160, 62),
                Size = new Size(200, 23),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(_txtUsername);

            // Password Label & TextBox
            var lblPassword = new Label
            {
                Text = "Passwort:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 42, 52),
                Location = new Point(40, 105),
                Size = new Size(120, 20)
            };
            this.Controls.Add(lblPassword);

            _txtPassword = new TextBox
            {
                Location = new Point(160, 102),
                Size = new Size(200, 23),
                Font = new Font("Segoe UI", 9),
                UseSystemPasswordChar = true
            };
            this.Controls.Add(_txtPassword);

            // Error Label
            _lblError = new Label
            {
                ForeColor = Color.Crimson,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(40, 135),
                Size = new Size(320, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_lblError);

            // Buttons panel
            var btnLogin = new Button
            {
                Text = "Anmelden",
                Location = new Point(130, 180),
                Size = new Size(110, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(235, 94, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            var btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new Point(250, 180),
                Size = new Size(110, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string username = _txtUsername.Text;
            string password = _txtPassword.Text;

            if (_authService.ValidateCredentials(username, password, out string? error))
            {
                this.LoggedInUser = username;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                _lblError.Text = error ?? "Ungültige Anmeldung.";
            }
        }
    }
}
