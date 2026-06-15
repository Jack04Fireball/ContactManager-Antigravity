using System;
using System.Drawing;
using System.Windows.Forms;
using ContactManager.Models;

namespace ContactManager.Forms
{
    public class MitarbeiterEditForm : Form
    {
        private readonly Mitarbeiter? _existingMitarbeiter;
        private readonly string _currentUser;
        private readonly Services.DataManager _dataManager;

        // Form Controls
        private TextBox _txtVorname = null!;
        private TextBox _txtNachname = null!;
        private DateTimePicker _dtpGeburtsdatum = null!;
        private TextBox _txtNationalität = null!;
        private TextBox _txtMobil = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtAdresse = null!;
        private TextBox _txtPLZ = null!;
        private TextBox _txtWohnort = null!;
        private CheckBox _chkActive = null!;

        private TextBox _txtMitarbeiterNummer = null!;
        private TextBox _txtAbteilung = null!;
        private TextBox _txtRolle = null!;
        private TextBox _txtAhvNummer = null!;
        private DateTimePicker _dtpEintrittsdatum = null!;
        private CheckBox _chkAustrittEnabled = null!;
        private DateTimePicker _dtpAustrittsdatum = null!;
        private NumericUpDown _numBeschäftigung = null!;
        private ComboBox _cmbKaderstufe = null!;
        private TextBox _txtGeschäftsadresse = null!;

        private CheckBox _chkIsLernender = null!;
        private NumericUpDown _numLehrjahre = null!;
        private NumericUpDown _numAktLehrjahr = null!;
        private TabPage _tabAusbildung = null!;
        private TabControl _tabControl = null!;

        public Mitarbeiter SavedMitarbeiter { get; private set; } = null!;

        public MitarbeiterEditForm(string currentUser, Services.DataManager dataManager, Mitarbeiter? existingMitarbeiter = null)
        {
            _currentUser = currentUser;
            _dataManager = dataManager;
            _existingMitarbeiter = existingMitarbeiter;
            InitializeComponent();
            if (_existingMitarbeiter != null)
            {
                LoadMitarbeiterData();
            }
            else
            {
                // Auto generate mitarbeiter number
                _txtMitarbeiterNummer.Text = _dataManager.GetNextMitarbeiterNummer();
            }
            ToggleApprenticeTab();
        }

        private void InitializeComponent()
        {
            this.Text = _existingMitarbeiter == null ? "Mitarbeiter hinzufügen" : "Mitarbeiter bearbeiten";
            this.Size = new Size(500, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 246, 250);

            _tabControl = new TabControl { Location = new Point(12, 12), Size = new Size(460, 390) };
            this.Controls.Add(_tabControl);

            // Tab 1: Persönlich
            var tabPersonal = new TabPage("Persönlich") { BackColor = Color.White };
            _tabControl.TabPages.Add(tabPersonal);
            BuildPersonalTab(tabPersonal);

            // Tab 2: Anstellung
            var tabAnstellung = new TabPage("Anstellung") { BackColor = Color.White };
            _tabControl.TabPages.Add(tabAnstellung);
            BuildAnstellungTab(tabAnstellung);

            // Tab 3: Ausbildung
            _tabAusbildung = new TabPage("Ausbildung") { BackColor = Color.White };
            BuildAusbildungTab(_tabAusbildung);

            // Buttons
            var btnSave = new Button
            {
                Text = "Speichern",
                Location = new Point(230, 430),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(235, 94, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new Point(360, 430),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void BuildPersonalTab(TabPage tab)
        {
            int startY = 15;
            int gapY = 32;
            int labelX = 20;
            int inputX = 160;
            int inputWidth = 260;

            CreateLabel(tab, "Vorname *:", labelX, startY);
            _txtVorname = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtVorname);

            startY += gapY;
            CreateLabel(tab, "Nachname *:", labelX, startY);
            _txtNachname = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtNachname);

            startY += gapY;
            CreateLabel(tab, "Geburtsdatum:", labelX, startY);
            _dtpGeburtsdatum = new DateTimePicker { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), Format = DateTimePickerFormat.Short };
            tab.Controls.Add(_dtpGeburtsdatum);

            startY += gapY;
            CreateLabel(tab, "Nationalität:", labelX, startY);
            _txtNationalität = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtNationalität);

            startY += gapY;
            CreateLabel(tab, "Mobiltelefon:", labelX, startY);
            _txtMobil = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtMobil);

            startY += gapY;
            CreateLabel(tab, "E-Mail-Adresse:", labelX, startY);
            _txtEmail = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtEmail);

            startY += gapY;
            CreateLabel(tab, "Adresse / PLZ / Ort:", labelX, startY);
            _txtAdresse = new TextBox { Location = new Point(inputX, startY), Size = new Size(110, 23), PlaceholderText = "Strasse & Nr." };
            _txtPLZ = new TextBox { Location = new Point(inputX + 115, startY), Size = new Size(50, 23), PlaceholderText = "PLZ" };
            _txtWohnort = new TextBox { Location = new Point(inputX + 170, startY), Size = new Size(90, 23), PlaceholderText = "Wohnort" };
            tab.Controls.Add(_txtAdresse);
            tab.Controls.Add(_txtPLZ);
            tab.Controls.Add(_txtWohnort);

            startY += gapY;
            _chkActive = new CheckBox { Text = "Mitarbeiter ist aktiv", Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), Checked = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            tab.Controls.Add(_chkActive);

            startY += gapY;
            _chkIsLernender = new CheckBox { Text = "Ist Lernender (Auszubildender)", Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            _chkIsLernender.CheckedChanged += (s, e) => ToggleApprenticeTab();
            tab.Controls.Add(_chkIsLernender);
        }

        private void BuildAnstellungTab(TabPage tab)
        {
            int startY = 15;
            int gapY = 32;
            int labelX = 20;
            int inputX = 160;
            int inputWidth = 260;

            CreateLabel(tab, "Personalnummer *:", labelX, startY);
            _txtMitarbeiterNummer = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), ReadOnly = (_existingMitarbeiter != null) };
            tab.Controls.Add(_txtMitarbeiterNummer);

            startY += gapY;
            CreateLabel(tab, "Abteilung:", labelX, startY);
            _txtAbteilung = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtAbteilung);

            startY += gapY;
            CreateLabel(tab, "Rolle / Funktion:", labelX, startY);
            _txtRolle = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtRolle);

            startY += gapY;
            CreateLabel(tab, "AHV-Nummer (756.X.X.X):", labelX, startY);
            _txtAhvNummer = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), PlaceholderText = "756.XXXX.XXXX.XX" };
            tab.Controls.Add(_txtAhvNummer);

            startY += gapY;
            CreateLabel(tab, "Eintrittsdatum:", labelX, startY);
            _dtpEintrittsdatum = new DateTimePicker { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), Format = DateTimePickerFormat.Short };
            tab.Controls.Add(_dtpEintrittsdatum);

            startY += gapY;
            _chkAustrittEnabled = new CheckBox { Text = "Austrittsdatum erfassen", Location = new Point(labelX, startY), Size = new Size(140, 23) };
            _chkAustrittEnabled.CheckedChanged += (s, e) => _dtpAustrittsdatum.Enabled = _chkAustrittEnabled.Checked;
            tab.Controls.Add(_chkAustrittEnabled);

            _dtpAustrittsdatum = new DateTimePicker { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), Format = DateTimePickerFormat.Short, Enabled = false };
            tab.Controls.Add(_dtpAustrittsdatum);

            startY += gapY;
            CreateLabel(tab, "Beschäftigungsgrad (%):", labelX, startY);
            _numBeschäftigung = new NumericUpDown { Location = new Point(inputX, startY), Size = new Size(80, 23), Minimum = 10, Maximum = 100, Value = 100 };
            tab.Controls.Add(_numBeschäftigung);

            CreateLabel(tab, "Kaderstufe (0-5):", inputX + 90, startY);
            _cmbKaderstufe = new ComboBox { Location = new Point(inputX + 190, startY), Size = new Size(70, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbKaderstufe.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5" });
            _cmbKaderstufe.SelectedIndex = 0;
            tab.Controls.Add(_cmbKaderstufe);

            startY += gapY;
            CreateLabel(tab, "Geschäftsadresse:", labelX, startY);
            _txtGeschäftsadresse = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            tab.Controls.Add(_txtGeschäftsadresse);
        }

        private void BuildAusbildungTab(TabPage tab)
        {
            int startY = 15;
            int gapY = 32;
            int labelX = 20;
            int inputX = 160;

            CreateLabel(tab, "Lehrjahre (gesamt):", labelX, startY);
            _numLehrjahre = new NumericUpDown { Location = new Point(inputX, startY), Size = new Size(80, 23), Minimum = 1, Maximum = 4, Value = 3 };
            tab.Controls.Add(_numLehrjahre);

            startY += gapY;
            CreateLabel(tab, "Aktuelles Lehrjahr:", labelX, startY);
            _numAktLehrjahr = new NumericUpDown { Location = new Point(inputX, startY), Size = new Size(80, 23), Minimum = 1, Maximum = 4, Value = 1 };
            tab.Controls.Add(_numAktLehrjahr);
        }

        private void CreateLabel(TabPage tab, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(135, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(37, 42, 52),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tab.Controls.Add(lbl);
        }

        private void ToggleApprenticeTab()
        {
            bool isLernender = _chkIsLernender.Checked;
            if (isLernender && !_tabControl.TabPages.Contains(_tabAusbildung))
            {
                _tabControl.TabPages.Add(_tabAusbildung);
            }
            else if (!isLernender && _tabControl.TabPages.Contains(_tabAusbildung))
            {
                _tabControl.TabPages.Remove(_tabAusbildung);
            }
        }

        private void LoadMitarbeiterData()
        {
            if (_existingMitarbeiter == null) return;

            _txtVorname.Text = _existingMitarbeiter.Vorname;
            _txtNachname.Text = _existingMitarbeiter.Nachname;
            _dtpGeburtsdatum.Value = _existingMitarbeiter.Geburtsdatum;
            _txtNationalität.Text = _existingMitarbeiter.Nationalität;
            _txtMobil.Text = _existingMitarbeiter.Mobiltelefonnummer;
            _txtEmail.Text = _existingMitarbeiter.EmailAdresse;
            _txtAdresse.Text = _existingMitarbeiter.Adresse;
            _txtPLZ.Text = _existingMitarbeiter.Postleitzahl;
            _txtWohnort.Text = _existingMitarbeiter.Wohnort;
            _chkActive.Checked = _existingMitarbeiter.IsActive;

            _txtMitarbeiterNummer.Text = _existingMitarbeiter.MitarbeiterNummer;
            _txtAbteilung.Text = _existingMitarbeiter.Abteilung;
            _txtRolle.Text = _existingMitarbeiter.Rolle;
            _txtAhvNummer.Text = _existingMitarbeiter.AhvNummer;
            _dtpEintrittsdatum.Value = _existingMitarbeiter.Eintrittsdatum;

            if (_existingMitarbeiter.Austrittsdatum.HasValue)
            {
                _chkAustrittEnabled.Checked = true;
                _dtpAustrittsdatum.Enabled = true;
                _dtpAustrittsdatum.Value = _existingMitarbeiter.Austrittsdatum.Value;
            }
            else
            {
                _chkAustrittEnabled.Checked = false;
                _dtpAustrittsdatum.Enabled = false;
            }

            _numBeschäftigung.Value = (decimal)_existingMitarbeiter.Beschäftigungsgrad;
            _cmbKaderstufe.SelectedItem = _existingMitarbeiter.Kaderstufe.ToString();
            _txtGeschäftsadresse.Text = _existingMitarbeiter.Geschäftsadresse;

            if (_existingMitarbeiter is Lernender l)
            {
                _chkIsLernender.Checked = true;
                _numLehrjahre.Value = l.Lehrjahre;
                _numAktLehrjahr.Value = l.AktuellesLehrjahr;
            }
            else
            {
                _chkIsLernender.Checked = false;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtVorname.Text) || string.IsNullOrWhiteSpace(_txtNachname.Text) || string.IsNullOrWhiteSpace(_txtMitarbeiterNummer.Text))
            {
                MessageBox.Show("Bitte füllen Sie alle Pflichtfelder (*) aus.", "Validierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate AHV
            string ahv = _txtAhvNummer.Text.Trim();
            if (!string.IsNullOrWhiteSpace(ahv) && !Mitarbeiter.ValidateAhvNummer(ahv))
            {
                MessageBox.Show("Die eingegebene AHV-Nummer ist ungültig.\nFormat: 756.XXXX.XXXX.XX", "Validierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if MitarbeiterNummer is unique for new employee
            if (_existingMitarbeiter == null)
            {
                foreach (var p in _dataManager.Contacts)
                {
                    if (p is Mitarbeiter m && m.MitarbeiterNummer.Equals(_txtMitarbeiterNummer.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Die Personalnummer existiert bereits.", "Validierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            // Instantiate correctly depending on Lernender status
            Mitarbeiter result;
            if (_chkIsLernender.Checked)
            {
                var l = (_existingMitarbeiter as Lernender) ?? new Lernender();
                
                if (_existingMitarbeiter != null)
                {
                    LogMutationIfChanged(l, "Lehrjahre", l.Lehrjahre.ToString(), _numLehrjahre.Value.ToString());
                    LogMutationIfChanged(l, "AktuellesLehrjahr", l.AktuellesLehrjahr.ToString(), _numAktLehrjahr.Value.ToString());
                }

                l.Lehrjahre = (int)_numLehrjahre.Value;
                l.AktuellesLehrjahr = (int)_numAktLehrjahr.Value;
                result = l;
            }
            else
            {
                result = (_existingMitarbeiter is Lernender) ? new Mitarbeiter() : (_existingMitarbeiter ?? new Mitarbeiter());
            }

            if (_existingMitarbeiter != null)
            {
                // Track changes for mutation logs
                LogMutationIfChanged(result, "Vorname", result.Vorname, _txtVorname.Text);
                LogMutationIfChanged(result, "Nachname", result.Nachname, _txtNachname.Text);
                LogMutationIfChanged(result, "Geburtsdatum", result.Geburtsdatum.ToString("dd.MM.yyyy"), _dtpGeburtsdatum.Value.ToString("dd.MM.yyyy"));
                LogMutationIfChanged(result, "Nationalität", result.Nationalität, _txtNationalität.Text);
                LogMutationIfChanged(result, "Mobiltelefonnummer", result.Mobiltelefonnummer, _txtMobil.Text);
                LogMutationIfChanged(result, "EmailAdresse", result.EmailAdresse, _txtEmail.Text);
                LogMutationIfChanged(result, "Adresse", result.Adresse, _txtAdresse.Text);
                LogMutationIfChanged(result, "Postleitzahl", result.Postleitzahl, _txtPLZ.Text);
                LogMutationIfChanged(result, "Wohnort", result.Wohnort, _txtWohnort.Text);
                LogMutationIfChanged(result, "IsActive", result.IsActive.ToString(), _chkActive.Checked.ToString());
                LogMutationIfChanged(result, "Abteilung", result.Abteilung, _txtAbteilung.Text);
                LogMutationIfChanged(result, "Rolle", result.Rolle, _txtRolle.Text);
                LogMutationIfChanged(result, "AhvNummer", result.AhvNummer, _txtAhvNummer.Text);
                LogMutationIfChanged(result, "Eintrittsdatum", result.Eintrittsdatum.ToString("dd.MM.yyyy"), _dtpEintrittsdatum.Value.ToString("dd.MM.yyyy"));
                
                string oldExit = result.Austrittsdatum.HasValue ? result.Austrittsdatum.Value.ToString("dd.MM.yyyy") : "Keines";
                string newExit = _chkAustrittEnabled.Checked ? _dtpAustrittsdatum.Value.ToString("dd.MM.yyyy") : "Keines";
                LogMutationIfChanged(result, "Austrittsdatum", oldExit, newExit);

                LogMutationIfChanged(result, "Beschäftigungsgrad", result.Beschäftigungsgrad.ToString(), _numBeschäftigung.Value.ToString());
                LogMutationIfChanged(result, "Kaderstufe", result.Kaderstufe.ToString(), _cmbKaderstufe.Text);
                LogMutationIfChanged(result, "Geschäftsadresse", result.Geschäftsadresse, _txtGeschäftsadresse.Text);
            }

            // Save details
            result.Vorname = _txtVorname.Text.Trim();
            result.Nachname = _txtNachname.Text.Trim();
            result.Geburtsdatum = _dtpGeburtsdatum.Value.Date;
            result.Nationalität = _txtNationalität.Text.Trim();
            result.Mobiltelefonnummer = _txtMobil.Text.Trim();
            result.EmailAdresse = _txtEmail.Text.Trim();
            result.Adresse = _txtAdresse.Text.Trim();
            result.Postleitzahl = _txtPLZ.Text.Trim();
            result.Wohnort = _txtWohnort.Text.Trim();
            result.IsActive = _chkActive.Checked;

            result.MitarbeiterNummer = _txtMitarbeiterNummer.Text.Trim();
            result.Abteilung = _txtAbteilung.Text.Trim();
            result.Rolle = _txtRolle.Text.Trim();
            result.AhvNummer = _txtAhvNummer.Text.Trim();
            result.Eintrittsdatum = _dtpEintrittsdatum.Value.Date;
            result.Austrittsdatum = _chkAustrittEnabled.Checked ? _dtpAustrittsdatum.Value.Date : (DateTime?)null;
            result.Beschäftigungsgrad = (double)_numBeschäftigung.Value;
            result.Kaderstufe = int.Parse(_cmbKaderstufe.Text);
            result.Geschäftsadresse = _txtGeschäftsadresse.Text.Trim();

            this.SavedMitarbeiter = result;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void LogMutationIfChanged(Person p, string fieldName, string oldVal, string newVal)
        {
            if (oldVal != newVal)
            {
                p.MutationsHistorie.Add(new MutationLogEntry
                {
                    Zeitpunkt = DateTime.Now,
                    FeldName = fieldName,
                    AlterWert = oldVal,
                    NeuerWert = newVal,
                    Benutzer = _currentUser
                });
            }
        }
    }
}
