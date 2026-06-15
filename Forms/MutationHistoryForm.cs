using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ContactManager.Models;

namespace ContactManager.Forms
{
    public class MutationHistoryForm : Form
    {
        private readonly Person _person;
        private DataGridView _dgvHistory = null!;

        public MutationHistoryForm(Person person)
        {
            _person = person;
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            this.Text = $"Mutationshistorie - {_person.NameDisplay} ({_person.TypDisplay})";
            this.Size = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 246, 250);

            var lblTitle = new Label
            {
                Text = $"Änderungsverlauf für {_person.NameDisplay}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(15, 15),
                Size = new Size(600, 25),
                ForeColor = Color.FromArgb(37, 42, 52)
            };
            this.Controls.Add(lblTitle);

            _dgvHistory = new DataGridView
            {
                Location = new Point(15, 50),
                Size = new Size(600, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(_dgvHistory);

            var btnClose = new Button
            {
                Text = "Schliessen",
                Location = new Point(515, 315),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadHistory()
        {
            if (_person.MutationsHistorie == null || !_person.MutationsHistorie.Any())
            {
                // Create a temporary binding list or empty message
                _dgvHistory.DataSource = null;
                return;
            }

            _dgvHistory.DataSource = _person.MutationsHistorie
                .OrderByDescending(m => m.Zeitpunkt)
                .Select(m => new
                {
                    Zeitpunkt = m.Zeitpunkt.ToString("dd.MM.yyyy HH:mm:ss"),
                    Feld = m.FeldName,
                    Alt = m.AlterWert,
                    Neu = m.NeuerWert,
                    Benutzer = m.Benutzer
                }).ToList();
        }
    }
}
