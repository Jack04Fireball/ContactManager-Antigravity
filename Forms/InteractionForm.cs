using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ContactManager.Models;

namespace ContactManager.Forms
{
    public class InteractionForm : Form
    {
        private readonly Kunde _kunde;
        private readonly List<Mitarbeiter> _mitarbeiterList;
        private readonly string _currentUser;
        
        private DataGridView _dgvInteractions = null!;
        private DateTimePicker _dtpDatum = null!;
        private ComboBox _cmbArt = null!;
        private ComboBox _cmbMitarbeiter = null!;
        private TextBox _txtNotiz = null!;

        public bool ChangesSaved { get; private set; } = false;

        public InteractionForm(Kunde kunde, List<Person> allContacts, string currentUser)
        {
            _kunde = kunde;
            _currentUser = currentUser;
            // Get all active employees to choose from
            _mitarbeiterList = allContacts.OfType<Mitarbeiter>().Where(m => m.IsActive).ToList();
            InitializeComponent();
            RefreshGrid();
        }

        private void InitializeComponent()
        {
            this.Text = $"Kontakthistorie - {_kunde.NameDisplay}";
            this.Size = new Size(700, 500);
            this.MinimumSize = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 246, 250);

            // Split container to have history on top and new interaction logger at the bottom
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 250
            };
            this.Controls.Add(splitContainer);

            // Top Panel: Grid
            var pnlTop = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            splitContainer.Panel1.Controls.Add(pnlTop);

            var lblHistory = new Label
            {
                Text = "Bisherige Kundenkontakte",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(300, 20)
            };
            pnlTop.Controls.Add(lblHistory);

            _dgvInteractions = new DataGridView
            {
                Location = new Point(10, 30),
                Size = new Size(660, 190),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            pnlTop.Controls.Add(_dgvInteractions);

            // Bottom Panel: Add new interaction
            var pnlBottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
            splitContainer.Panel2.Controls.Add(pnlBottom);

            var lblNew = new Label
            {
                Text = "Neuen Kontakt erfassen",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 94, 40),
                Location = new Point(10, 5),
                Size = new Size(300, 20)
            };
            pnlBottom.Controls.Add(lblNew);

            // Date
            CreateLabel(pnlBottom, "Datum:", 10, 35);
            _dtpDatum = new DateTimePicker
            {
                Location = new Point(110, 32),
                Size = new Size(150, 23),
                Format = DateTimePickerFormat.Short
            };
            pnlBottom.Controls.Add(_dtpDatum);

            // Type
            CreateLabel(pnlBottom, "Kontaktart:", 10, 70);
            _cmbArt = new ComboBox
            {
                Location = new Point(110, 67),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbArt.Items.AddRange(new object[] { "Telefon", "E-Mail", "Meeting", "Brief", "Sonstiges" });
            _cmbArt.SelectedIndex = 0;
            pnlBottom.Controls.Add(_cmbArt);

            // Employee
            CreateLabel(pnlBottom, "Mitarbeiter:", 10, 105);
            _cmbMitarbeiter = new ComboBox
            {
                Location = new Point(110, 102),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            foreach (var m in _mitarbeiterList)
            {
                _cmbMitarbeiter.Items.Add(m.NameDisplay);
            }
            if (_cmbMitarbeiter.Items.Count > 0)
            {
                _cmbMitarbeiter.SelectedIndex = 0;
            }
            else
            {
                _cmbMitarbeiter.Text = _currentUser;
            }
            pnlBottom.Controls.Add(_cmbMitarbeiter);

            // Notes
            CreateLabel(pnlBottom, "Notizen / Inhalt *:", 280, 35);
            _txtNotiz = new TextBox
            {
                Location = new Point(280, 55),
                Size = new Size(390, 70),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            pnlBottom.Controls.Add(_txtNotiz);

            // Button add
            var btnAdd = new Button
            {
                Text = "Kontakt hinzufügen",
                Location = new Point(410, 135),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(235, 94, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;
            pnlBottom.Controls.Add(btnAdd);

            var btnClose = new Button
            {
                Text = "Schliessen",
                Location = new Point(570, 135),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            pnlBottom.Controls.Add(btnClose);
        }

        private void CreateLabel(Panel pnl, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(95, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnl.Controls.Add(lbl);
        }

        private void RefreshGrid()
        {
            _dgvInteractions.DataSource = null;
            _dgvInteractions.DataSource = _kunde.InteraktionsHistorie
                .OrderByDescending(i => i.Datum)
                .Select(i => new
                {
                    Datum = i.Datum.ToString("dd.MM.yyyy HH:mm"),
                    i.Art,
                    Mitarbeiter = i.MitarbeiterName,
                    Notiz = i.Notiz
                }).ToList();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtNotiz.Text))
            {
                MessageBox.Show("Bitte geben Sie eine Notiz ein.", "Validierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var note = new Interaction
            {
                Datum = _dtpDatum.Value.Date == DateTime.Today ? DateTime.Now : _dtpDatum.Value.Date,
                Art = _cmbArt.Text,
                MitarbeiterName = _cmbMitarbeiter.Text,
                Notiz = _txtNotiz.Text.Trim()
            };

            _kunde.InteraktionsHistorie.Add(note);
            _txtNotiz.Clear();
            this.ChangesSaved = true;
            RefreshGrid();
            MessageBox.Show("Kontakt erfolgreich protokolliert.", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
