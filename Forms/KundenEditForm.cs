using System;
using System.Drawing;
using System.Windows.Forms;
using ContactManager.Models;

namespace ContactManager.Forms
{
    public class KundenEditForm : Form
    {
        private readonly Kunde? _existingKunde;
        private readonly string _currentUser;
        
        private ComboBox _cmbAnrede = null!;
        private TextBox _txtTitel = null!;
        private TextBox _txtVorname = null!;
        private TextBox _txtNachname = null!;
        private DateTimePicker _dtpGeburtsdatum = null!;
        private ComboBox _cmbGeschlecht = null!;
        private TextBox _txtMobil = null!;
        private TextBox _txtPhone = null!;
        private TextBox _txtEmail = null!;
        private CheckBox _chkActive = null!;

        public Kunde SavedKunde { get; private set; } = null!;

        public KundenEditForm(string currentUser, Kunde? existingKunde = null)
        {
            _currentUser = currentUser;
            _existingKunde = existingKunde;
            InitializeComponent();
            if (_existingKunde != null)
            {
                LoadKundeData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _existingKunde == null ? "Kunde hinzufügen" : "Kunde bearbeiten";
            this.Size = new Size(450, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 246, 250);

            int startY = 20;
            int gapY = 35;
            int labelX = 20;
            int inputX = 160;
            int inputWidth = 240;

            // Anrede
            CreateLabel("Anrede:", labelX, startY);
            _cmbAnrede = new ComboBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbAnrede.Items.AddRange(new object[] { "Herr", "Frau", "Divers" });
            _cmbAnrede.SelectedIndex = 0;
            this.Controls.Add(_cmbAnrede);

            // Titel
            startY += gapY;
            CreateLabel("Titel:", labelX, startY);
            _txtTitel = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            this.Controls.Add(_txtTitel);

            // Vorname
            startY += gapY;
            CreateLabel("Vorname *:", labelX, startY);
            _txtVorname = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            this.Controls.Add(_txtVorname);

            // Nachname
            startY += gapY;
            CreateLabel("Nachname *:", labelX, startY);
            _txtNachname = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            this.Controls.Add(_txtNachname);

            // Geburtsdatum
            startY += gapY;
            CreateLabel("Geburtsdatum:", labelX, startY);
            _dtpGeburtsdatum = new DateTimePicker 
            { 
                Location = new Point(inputX, startY), 
                Size = new Size(inputWidth, 23),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(_dtpGeburtsdatum);

            // Geschlecht
            startY += gapY;
            CreateLabel("Geschlecht:", labelX, startY);
            _cmbGeschlecht = new ComboBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbGeschlecht.Items.AddRange(new object[] { "Männlich", "Weiblich", "Divers" });
            _cmbGeschlecht.SelectedIndex = 0;
            this.Controls.Add(_cmbGeschlecht);

            // Mobiltelefon
            startY += gapY;
            CreateLabel("Mobiltelefon:", labelX, startY);
            _txtMobil = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            this.Controls.Add(_txtMobil);

            // Telefon Geschäft
            startY += gapY;
            CreateLabel("Telefon Geschäft:", labelX, startY);
            _txtPhone = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            this.Controls.Add(_txtPhone);

            // E-Mail
            startY += gapY;
            CreateLabel("E-Mail-Adresse:", labelX, startY);
            _txtEmail = new TextBox { Location = new Point(inputX, startY), Size = new Size(inputWidth, 23) };
            this.Controls.Add(_txtEmail);

            // Status (Aktiv)
            startY += gapY;
            _chkActive = new CheckBox 
            { 
                Text = "Kunde ist aktiv", 
                Location = new Point(inputX, startY), 
                Size = new Size(inputWidth, 23),
                Checked = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            this.Controls.Add(_chkActive);

            // Buttons
            var btnSave = new Button
            {
                Text = "Speichern",
                Location = new Point(180, 390),
                Size = new Size(100, 30),
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
                Location = new Point(300, 390),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void CreateLabel(string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(130, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(37, 42, 52),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lbl);
        }

        private void LoadKundeData()
        {
            if (_existingKunde == null) return;

            _cmbAnrede.SelectedItem = _existingKunde.Anrede;
            _txtTitel.Text = _existingKunde.Titel;
            _txtVorname.Text = _existingKunde.Vorname;
            _txtNachname.Text = _existingKunde.Nachname;
            _dtpGeburtsdatum.Value = _existingKunde.Geburtsdatum;
            _cmbGeschlecht.SelectedItem = _existingKunde.Geschlecht;
            _txtMobil.Text = _existingKunde.Mobiltelefonnummer;
            _txtPhone.Text = _existingKunde.TelefonnummerGeschäft;
            _txtEmail.Text = _existingKunde.EmailAdresse;
            _chkActive.Checked = _existingKunde.IsActive;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtVorname.Text) || string.IsNullOrWhiteSpace(_txtNachname.Text))
            {
                MessageBox.Show("Bitte füllen Sie alle Pflichtfelder (*) aus.", "Validierungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = _existingKunde ?? new Kunde();

            if (_existingKunde != null)
            {
                // Track changes for mutation history
                LogMutationIfChanged(result, "Anrede", result.Anrede, _cmbAnrede.Text);
                LogMutationIfChanged(result, "Titel", result.Titel, _txtTitel.Text);
                LogMutationIfChanged(result, "Vorname", result.Vorname, _txtVorname.Text);
                LogMutationIfChanged(result, "Nachname", result.Nachname, _txtNachname.Text);
                LogMutationIfChanged(result, "Geburtsdatum", result.Geburtsdatum.ToString("dd.MM.yyyy"), _dtpGeburtsdatum.Value.ToString("dd.MM.yyyy"));
                LogMutationIfChanged(result, "Geschlecht", result.Geschlecht, _cmbGeschlecht.Text);
                LogMutationIfChanged(result, "Mobiltelefonnummer", result.Mobiltelefonnummer, _txtMobil.Text);
                LogMutationIfChanged(result, "TelefonnummerGeschäft", result.TelefonnummerGeschäft, _txtPhone.Text);
                LogMutationIfChanged(result, "EmailAdresse", result.EmailAdresse, _txtEmail.Text);
                LogMutationIfChanged(result, "IsActive", result.IsActive.ToString(), _chkActive.Checked.ToString());
            }

            result.Anrede = _cmbAnrede.Text;
            result.Titel = _txtTitel.Text;
            result.Vorname = _txtVorname.Text.Trim();
            result.Nachname = _txtNachname.Text.Trim();
            result.Geburtsdatum = _dtpGeburtsdatum.Value.Date;
            result.Geschlecht = _cmbGeschlecht.Text;
            result.Mobiltelefonnummer = _txtMobil.Text.Trim();
            result.TelefonnummerGeschäft = _txtPhone.Text.Trim();
            result.EmailAdresse = _txtEmail.Text.Trim();
            result.IsActive = _chkActive.Checked;

            this.SavedKunde = result;
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
