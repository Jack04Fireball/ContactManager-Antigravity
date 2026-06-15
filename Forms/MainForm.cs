using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ContactManager.Models;
using ContactManager.Services;

namespace ContactManager.Forms
{
    public class MainForm : Form
    {
        private readonly DataManager _dataManager;
        private readonly AuthService _authService;
        private readonly string _currentUser;

        // UI elements
        private Panel _pnlSidebar = null!;
        private Panel _pnlContentContainer = null!;
        private ToolStripStatusLabel _lblStatusText = null!;

        // Swappable Panels
        private Panel _pnlDashboard = null!;
        private Panel _pnlKunden = null!;
        private Panel _pnlMitarbeiter = null!;
        private Panel _pnlImport = null!;

        // Sidebar Navigation Buttons
        private Button _btnNavDashboard = null!;
        private Button _btnNavKunden = null!;
        private Button _btnNavMitarbeiter = null!;
        private Button _btnNavImport = null!;

        // Dashboard elements
        private Label _lblDashKunden = null!;
        private Label _lblDashMitarbeiter = null!;
        private Label _lblDashInteractions = null!;
        private DataGridView _dgvDashBirthdays = null!;
        private DataGridView _dgvDashInteractions = null!;

        // Kunden elements
        private TextBox _txtKundenSearch = null!;
        private ComboBox _cmbKundenFilter = null!;
        private DataGridView _dgvKunden = null!;

        // Mitarbeiter elements
        private TextBox _txtMitarbeiterSearch = null!;
        private ComboBox _cmbMitarbeiterAbteilung = null!;
        private ComboBox _cmbMitarbeiterTyp = null!;
        private DataGridView _dgvMitarbeiter = null!;

        // Import elements
        private TextBox _txtCsvPath = null!;
        private DataGridView _dgvImportPreview = null!;
        private Label _lblImportStatus = null!;
        private List<Person> _previewList = new();

        public MainForm(DataManager dataManager, AuthService authService, string currentUser)
        {
            _dataManager = dataManager;
            _authService = authService;
            _currentUser = currentUser;
            InitializeComponent();
            SetupDashboard();
            SetupKunden();
            SetupMitarbeiter();
            SetupImport();
            
            // Start with Dashboard
            ShowPanel(_pnlDashboard, _btnNavDashboard);
        }

        private void InitializeComponent()
        {
            this.Text = "Contact Manager - Semesterprojekt PF2";
            this.Size = new Size(1024, 700);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 246, 250);

            // 1. Sidebar Panel
            _pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(37, 42, 52)
            };
            this.Controls.Add(_pnlSidebar);

            // App title in sidebar
            var lblLogo = new Label
            {
                Text = "CONTACT MANAGER",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 94, 40),
                Location = new Point(15, 20),
                Size = new Size(190, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlSidebar.Controls.Add(lblLogo);

            // Nav buttons
            int navY = 80;
            int navHeight = 40;
            int navGap = 10;

            _btnNavDashboard = CreateNavButton("Dashboard", navY);
            _btnNavDashboard.Click += (s, e) => { RefreshDashboardData(); ShowPanel(_pnlDashboard, _btnNavDashboard); };
            _pnlSidebar.Controls.Add(_btnNavDashboard);

            navY += navHeight + navGap;
            _btnNavKunden = CreateNavButton("Kunden verwalten", navY);
            _btnNavKunden.Click += (s, e) => { RefreshKundenGrid(); ShowPanel(_pnlKunden, _btnNavKunden); };
            _pnlSidebar.Controls.Add(_btnNavKunden);

            navY += navHeight + navGap;
            _btnNavMitarbeiter = CreateNavButton("Mitarbeiter verwalten", navY);
            _btnNavMitarbeiter.Click += (s, e) => { RefreshMitarbeiterGrid(); ShowPanel(_pnlMitarbeiter, _btnNavMitarbeiter); };
            _pnlSidebar.Controls.Add(_btnNavMitarbeiter);

            navY += navHeight + navGap;
            _btnNavImport = CreateNavButton("CSV-Import", navY);
            _btnNavImport.Click += (s, e) => { ShowPanel(_pnlImport, _btnNavImport); };
            _pnlSidebar.Controls.Add(_btnNavImport);

            // User Info & Logout at the bottom of sidebar
            var lblUser = new Label
            {
                Text = $"Angemeldet: {_currentUser}",
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(15, 560),
                Size = new Size(190, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _pnlSidebar.Controls.Add(lblUser);

            var btnLogout = new Button
            {
                Text = "Abmelden",
                Location = new Point(15, 590),
                Size = new Size(190, 30),
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;
            _pnlSidebar.Controls.Add(btnLogout);

            // 2. Status Bar
            var statusStrip = new StatusStrip();
            _lblStatusText = new ToolStripStatusLabel { Text = "Bereit. Alle Daten geladen." };
            statusStrip.Items.Add(_lblStatusText);
            this.Controls.Add(statusStrip);

            // 3. Content Container Panel
            _pnlContentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 246, 250),
                Padding = new Padding(15)
            };
            this.Controls.Add(_pnlContentContainer);
        }

        private Button CreateNavButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(10, y),
                Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 42, 52),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void ShowPanel(Panel panelToShow, Button activeNavButton)
        {
            // Reset nav colors
            _btnNavDashboard.BackColor = Color.FromArgb(37, 42, 52);
            _btnNavKunden.BackColor = Color.FromArgb(37, 42, 52);
            _btnNavMitarbeiter.BackColor = Color.FromArgb(37, 42, 52);
            _btnNavImport.BackColor = Color.FromArgb(37, 42, 52);

            activeNavButton.BackColor = Color.FromArgb(235, 94, 40);

            // Swap visibility
            _pnlDashboard.Visible = false;
            _pnlKunden.Visible = false;
            _pnlMitarbeiter.Visible = false;
            _pnlImport.Visible = false;

            panelToShow.Visible = true;
            panelToShow.BringToFront();
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry; // Signals program to reopen login form
            this.Close();
        }

        #region Dashboard Setup & Logic
        private void SetupDashboard()
        {
            _pnlDashboard = new Panel { Dock = DockStyle.Fill, Visible = false };
            _pnlContentContainer.Controls.Add(_pnlDashboard);

            // Title
            var lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(300, 30),
                ForeColor = Color.FromArgb(37, 42, 52)
            };
            _pnlDashboard.Controls.Add(lblTitle);

            // 3 KPI Cards Panel
            var pnlCards = new TableLayoutPanel
            {
                Location = new Point(0, 45),
                Size = new Size(760, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 3,
                RowCount = 1
            };
            pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            _pnlDashboard.Controls.Add(pnlCards);

            // Card 1: Kunden
            var pnlCardKunden = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(9, 132, 227), Margin = new Padding(5) };
            _lblDashKunden = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            pnlCardKunden.Controls.Add(_lblDashKunden);
            pnlCards.Controls.Add(pnlCardKunden, 0, 0);

            // Card 2: Mitarbeiter
            var pnlCardMitarbeiter = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 184, 148), Margin = new Padding(5) };
            _lblDashMitarbeiter = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            pnlCardMitarbeiter.Controls.Add(_lblDashMitarbeiter);
            pnlCards.Controls.Add(pnlCardMitarbeiter, 1, 0);

            // Card 3: Interaktionen
            var pnlCardInteractions = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(214, 48, 49), Margin = new Padding(5) };
            _lblDashInteractions = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            pnlCardInteractions.Controls.Add(_lblDashInteractions);
            pnlCards.Controls.Add(pnlCardInteractions, 2, 0);

            // Lower Section: Left Grid (Birthdays), Right Grid (Recent Contacts)
            var split = new SplitContainer
            {
                Location = new Point(0, 160),
                Size = new Size(760, 460),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Orientation = Orientation.Vertical,
                SplitterDistance = 370
            };
            _pnlDashboard.Controls.Add(split);

            // Birthdays Panel
            var pnlBirthdays = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            split.Panel1.Controls.Add(pnlBirthdays);

            var lblBdayTitle = new Label { Text = "Anstehende Geburtstage (nächste 30 Tage)", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Dock = DockStyle.Top, Height = 25 };
            pnlBirthdays.Controls.Add(lblBdayTitle);

            _dgvDashBirthdays = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            pnlBirthdays.Controls.Add(_dgvDashBirthdays);
            _dgvDashBirthdays.BringToFront();

            // Interactions Panel
            var pnlInteractions = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            split.Panel2.Controls.Add(pnlInteractions);

            var lblIntTitle = new Label { Text = "Letzte Kundenkontakte", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Dock = DockStyle.Top, Height = 25 };
            pnlInteractions.Controls.Add(lblIntTitle);

            _dgvDashInteractions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            pnlInteractions.Controls.Add(_dgvDashInteractions);
            _dgvDashInteractions.BringToFront();

            RefreshDashboardData();
        }

        private void RefreshDashboardData()
        {
            int totalKunden = _dataManager.Contacts.OfType<Kunde>().Count();
            int activeKunden = _dataManager.Contacts.OfType<Kunde>().Count(k => k.IsActive);
            _lblDashKunden.Text = $"KUNDEN\nGesamt: {totalKunden}\nAktiv: {activeKunden}\nPassiv: {totalKunden - activeKunden}";

            int totalMitarbeiter = _dataManager.Contacts.OfType<Mitarbeiter>().Count();
            int apprentices = _dataManager.Contacts.OfType<Lernender>().Count();
            int activeMitarbeiter = _dataManager.Contacts.OfType<Mitarbeiter>().Count(m => m.IsActive);
            _lblDashMitarbeiter.Text = $"MITARBEITER\nGesamt: {totalMitarbeiter}\nAktiv: {activeMitarbeiter}\nLernende: {apprentices}";

            var allInteractions = _dataManager.Contacts.OfType<Kunde>().SelectMany(k => k.InteraktionsHistorie).ToList();
            int totalInts = allInteractions.Count;
            int recentInts = allInteractions.Count(i => (DateTime.Now - i.Datum).TotalDays <= 30);
            _lblDashInteractions.Text = $"KONTAKTE\nTotal: {totalInts}\nLetzte 30 Tage: {recentInts}";

            // Birthdays logic (next 30 days)
            var upcomingBdays = _dataManager.Contacts.Select(p => {
                var nextBday = new DateTime(DateTime.Today.Year, p.Geburtsdatum.Month, p.Geburtsdatum.Day);
                if (nextBday < DateTime.Today) nextBday = nextBday.AddYears(1);
                int daysLeft = (nextBday - DateTime.Today).Days;
                int age = nextBday.Year - p.Geburtsdatum.Year;
                return new { Person = p, DaysLeft = daysLeft, Age = age };
            })
            .Where(x => x.DaysLeft <= 30)
            .OrderBy(x => x.DaysLeft)
            .Select(x => new
            {
                Name = x.Person.NameDisplay,
                Typ = x.Person.TypDisplay,
                Geburtsdatum = x.Person.Geburtsdatum.ToString("dd.MM.yyyy"),
                Alter = x.Age,
                Tage = x.DaysLeft
            }).ToList();

            _dgvDashBirthdays.DataSource = upcomingBdays;

            // Recent interactions logic (last 5)
            var recentLogs = _dataManager.Contacts.OfType<Kunde>()
                .SelectMany(k => k.InteraktionsHistorie.Select(i => new { Kunde = k.NameDisplay, i.Datum, i.Art, i.MitarbeiterName, i.Notiz }))
                .OrderByDescending(x => x.Datum)
                .Take(5)
                .Select(x => new
                {
                    Datum = x.Datum.ToString("dd.MM.yyyy"),
                    Kunde = x.Kunde,
                    Art = x.Art,
                    Mitarbeiter = x.MitarbeiterName,
                    Notiz = x.Notiz
                }).ToList();

            _dgvDashInteractions.DataSource = recentLogs;
        }
        #endregion

        #region Kunden Setup & Logic
        private void SetupKunden()
        {
            _pnlKunden = new Panel { Dock = DockStyle.Fill, Visible = false };
            _pnlContentContainer.Controls.Add(_pnlKunden);

            // Title
            var lblTitle = new Label { Text = "Kundenverwaltung", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(0, 0), Size = new Size(300, 30) };
            _pnlKunden.Controls.Add(lblTitle);

            // Top Bar: Search and Status Filter
            var pnlTop = new Panel { Location = new Point(0, 40), Size = new Size(760, 45), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _pnlKunden.Controls.Add(pnlTop);

            var lblSearch = new Label { Text = "Suchen:", Location = new Point(0, 10), Size = new Size(50, 20), TextAlign = ContentAlignment.MiddleLeft };
            pnlTop.Controls.Add(lblSearch);

            _txtKundenSearch = new TextBox { Location = new Point(55, 9), Size = new Size(200, 23) };
            _txtKundenSearch.TextChanged += (s, e) => RefreshKundenGrid();
            pnlTop.Controls.Add(_txtKundenSearch);

            var lblFilter = new Label { Text = "Status Filter:", Location = new Point(280, 10), Size = new Size(80, 20), TextAlign = ContentAlignment.MiddleLeft };
            pnlTop.Controls.Add(lblFilter);

            _cmbKundenFilter = new ComboBox { Location = new Point(365, 9), Size = new Size(120, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbKundenFilter.Items.AddRange(new object[] { "Alle", "Aktiv", "Passiv" });
            _cmbKundenFilter.SelectedIndex = 0;
            _cmbKundenFilter.SelectedIndexChanged += (s, e) => RefreshKundenGrid();
            pnlTop.Controls.Add(_cmbKundenFilter);

            // Grid
            _dgvKunden = new DataGridView
            {
                Location = new Point(0, 95),
                Size = new Size(760, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _pnlKunden.Controls.Add(_dgvKunden);

            // Action Panel (Bottom)
            var pnlActions = new FlowLayoutPanel
            {
                Location = new Point(0, 525),
                Size = new Size(760, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight
            };
            _pnlKunden.Controls.Add(pnlActions);

            var btnAdd = CreateActionButton("Hinzufügen", Color.FromArgb(0, 184, 148));
            btnAdd.Click += BtnKundenAdd_Click;
            pnlActions.Controls.Add(btnAdd);

            var btnEdit = CreateActionButton("Bearbeiten", Color.FromArgb(9, 132, 227));
            btnEdit.Click += BtnKundenEdit_Click;
            pnlActions.Controls.Add(btnEdit);

            var btnToggle = CreateActionButton("Status umschalten", Color.FromArgb(108, 92, 231));
            btnToggle.Click += BtnKundenToggle_Click;
            pnlActions.Controls.Add(btnToggle);

            var btnDelete = CreateActionButton("Löschen", Color.FromArgb(214, 48, 49));
            btnDelete.Click += BtnKundenDelete_Click;
            pnlActions.Controls.Add(btnDelete);

            var btnInteractions = CreateActionButton("Kontakte...", Color.FromArgb(235, 94, 40));
            btnInteractions.Click += BtnKundenInteractions_Click;
            pnlActions.Controls.Add(btnInteractions);

            var btnHistory = CreateActionButton("Mutationshistorie...", Color.FromArgb(80, 80, 80));
            btnHistory.Click += BtnKundenHistory_Click;
            pnlActions.Controls.Add(btnHistory);
        }

        private Button CreateActionButton(string text, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(115, 30),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void RefreshKundenGrid()
        {
            string query = _txtKundenSearch.Text;
            var filtered = _dataManager.Search(query).OfType<Kunde>().ToList();

            string statusFilter = _cmbKundenFilter.SelectedItem?.ToString() ?? "Alle";
            if (statusFilter == "Aktiv")
            {
                filtered = filtered.Where(k => k.IsActive).ToList();
            }
            else if (statusFilter == "Passiv")
            {
                filtered = filtered.Where(k => !k.IsActive).ToList();
            }

            _dgvKunden.DataSource = null;
            _dgvKunden.DataSource = filtered.Select(k => new
            {
                k.Id,
                Anrede = k.Anrede,
                Titel = k.Titel,
                Nachname = k.Nachname,
                Vorname = k.Vorname,
                Geburtsdatum = k.Geburtsdatum.ToString("dd.MM.yyyy"),
                Geschlecht = k.Geschlecht,
                Mobil = k.Mobiltelefonnummer,
                EMail = k.EmailAdresse,
                Status = k.IsActive ? "Aktiv" : "Passiv"
            }).ToList();

            if (_dgvKunden.Columns.Count > 0)
            {
                _dgvKunden.Columns["Id"].Visible = false; // Hide ID
            }
        }

        private void BtnKundenAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new KundenEditForm(_currentUser))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _dataManager.Contacts.Add(form.SavedKunde);
                    _dataManager.SaveData();
                    RefreshKundenGrid();
                    _lblStatusText.Text = $"Kunde '{form.SavedKunde.NameDisplay}' hinzugefügt.";
                }
            }
        }

        private void BtnKundenEdit_Click(object? sender, EventArgs e)
        {
            if (_dgvKunden.SelectedRows.Count == 0) return;
            string? id = _dgvKunden.SelectedRows[0].Cells["Id"].Value?.ToString();
            var kunde = _dataManager.Contacts.OfType<Kunde>().FirstOrDefault(k => k.Id == id);
            if (kunde == null) return;

            using (var form = new KundenEditForm(_currentUser, kunde))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _dataManager.SaveData();
                    RefreshKundenGrid();
                    _lblStatusText.Text = $"Kunde '{kunde.NameDisplay}' aktualisiert.";
                }
            }
        }

        private void BtnKundenToggle_Click(object? sender, EventArgs e)
        {
            if (_dgvKunden.SelectedRows.Count == 0) return;
            string? id = _dgvKunden.SelectedRows[0].Cells["Id"].Value?.ToString();
            var kunde = _dataManager.Contacts.OfType<Kunde>().FirstOrDefault(k => k.Id == id);
            if (kunde == null) return;

            string oldStatus = kunde.IsActive ? "Aktiv" : "Passiv";
            kunde.IsActive = !kunde.IsActive;
            string newStatus = kunde.IsActive ? "Aktiv" : "Passiv";

            _dataManager.LogMutation(kunde, "IsActive", oldStatus, newStatus, _currentUser);
            _dataManager.SaveData();
            RefreshKundenGrid();
            _lblStatusText.Text = $"Kunde '{kunde.NameDisplay}' Status auf {newStatus} gesetzt.";
        }

        private void BtnKundenDelete_Click(object? sender, EventArgs e)
        {
            if (_dgvKunden.SelectedRows.Count == 0) return;
            string? id = _dgvKunden.SelectedRows[0].Cells["Id"].Value?.ToString();
            var kunde = _dataManager.Contacts.OfType<Kunde>().FirstOrDefault(k => k.Id == id);
            if (kunde == null) return;

            var res = MessageBox.Show($"Möchten Sie den Kunden '{kunde.NameDisplay}' wirklich löschen?", "Löschen bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                _dataManager.Contacts.Remove(kunde);
                _dataManager.SaveData();
                RefreshKundenGrid();
                _lblStatusText.Text = $"Kunde '{kunde.NameDisplay}' gelöscht.";
            }
        }

        private void BtnKundenInteractions_Click(object? sender, EventArgs e)
        {
            if (_dgvKunden.SelectedRows.Count == 0) return;
            string? id = _dgvKunden.SelectedRows[0].Cells["Id"].Value?.ToString();
            var kunde = _dataManager.Contacts.OfType<Kunde>().FirstOrDefault(k => k.Id == id);
            if (kunde == null) return;

            using (var form = new InteractionForm(kunde, _dataManager.Contacts, _currentUser))
            {
                form.ShowDialog();
                if (form.ChangesSaved)
                {
                    _dataManager.SaveData();
                }
            }
        }

        private void BtnKundenHistory_Click(object? sender, EventArgs e)
        {
            if (_dgvKunden.SelectedRows.Count == 0) return;
            string? id = _dgvKunden.SelectedRows[0].Cells["Id"].Value?.ToString();
            var kunde = _dataManager.Contacts.OfType<Kunde>().FirstOrDefault(k => k.Id == id);
            if (kunde == null) return;

            using (var form = new MutationHistoryForm(kunde))
            {
                form.ShowDialog();
            }
        }
        #endregion

        #region Mitarbeiter Setup & Logic
        private void SetupMitarbeiter()
        {
            _pnlMitarbeiter = new Panel { Dock = DockStyle.Fill, Visible = false };
            _pnlContentContainer.Controls.Add(_pnlMitarbeiter);

            var lblTitle = new Label { Text = "Mitarbeiterverwaltung", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(0, 0), Size = new Size(300, 30) };
            _pnlMitarbeiter.Controls.Add(lblTitle);

            // Top Bar
            var pnlTop = new Panel { Location = new Point(0, 40), Size = new Size(760, 45), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _pnlMitarbeiter.Controls.Add(pnlTop);

            var lblSearch = new Label { Text = "Suchen:", Location = new Point(0, 10), Size = new Size(50, 20), TextAlign = ContentAlignment.MiddleLeft };
            pnlTop.Controls.Add(lblSearch);

            _txtMitarbeiterSearch = new TextBox { Location = new Point(55, 9), Size = new Size(160, 23) };
            _txtMitarbeiterSearch.TextChanged += (s, e) => RefreshMitarbeiterGrid();
            pnlTop.Controls.Add(_txtMitarbeiterSearch);

            var lblDept = new Label { Text = "Abteilung:", Location = new Point(230, 10), Size = new Size(65, 20), TextAlign = ContentAlignment.MiddleLeft };
            pnlTop.Controls.Add(lblDept);

            _cmbMitarbeiterAbteilung = new ComboBox { Location = new Point(300, 9), Size = new Size(110, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbMitarbeiterAbteilung.Items.Add("Alle");
            _cmbMitarbeiterAbteilung.SelectedIndex = 0;
            _cmbMitarbeiterAbteilung.SelectedIndexChanged += (s, e) => RefreshMitarbeiterGrid();
            pnlTop.Controls.Add(_cmbMitarbeiterAbteilung);

            var lblType = new Label { Text = "Typ:", Location = new Point(425, 10), Size = new Size(35, 20), TextAlign = ContentAlignment.MiddleLeft };
            pnlTop.Controls.Add(lblType);

            _cmbMitarbeiterTyp = new ComboBox { Location = new Point(465, 9), Size = new Size(110, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbMitarbeiterTyp.Items.AddRange(new object[] { "Alle", "Mitarbeiter", "Lernende" });
            _cmbMitarbeiterTyp.SelectedIndex = 0;
            _cmbMitarbeiterTyp.SelectedIndexChanged += (s, e) => RefreshMitarbeiterGrid();
            pnlTop.Controls.Add(_cmbMitarbeiterTyp);

            // Grid
            _dgvMitarbeiter = new DataGridView
            {
                Location = new Point(0, 95),
                Size = new Size(760, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _pnlMitarbeiter.Controls.Add(_dgvMitarbeiter);

            // Actions (Bottom)
            var pnlActions = new FlowLayoutPanel
            {
                Location = new Point(0, 525),
                Size = new Size(760, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight
            };
            _pnlMitarbeiter.Controls.Add(pnlActions);

            var btnAdd = CreateActionButton("Hinzufügen", Color.FromArgb(0, 184, 148));
            btnAdd.Click += BtnMitarbeiterAdd_Click;
            pnlActions.Controls.Add(btnAdd);

            var btnEdit = CreateActionButton("Bearbeiten", Color.FromArgb(9, 132, 227));
            btnEdit.Click += BtnMitarbeiterEdit_Click;
            pnlActions.Controls.Add(btnEdit);

            var btnToggle = CreateActionButton("Status umschalten", Color.FromArgb(108, 92, 231));
            btnToggle.Click += BtnMitarbeiterToggle_Click;
            pnlActions.Controls.Add(btnToggle);

            var btnDelete = CreateActionButton("Löschen", Color.FromArgb(214, 48, 49));
            btnDelete.Click += BtnMitarbeiterDelete_Click;
            pnlActions.Controls.Add(btnDelete);

            var btnHistory = CreateActionButton("Mutationshistorie...", Color.FromArgb(80, 80, 80));
            btnHistory.Click += BtnMitarbeiterHistory_Click;
            pnlActions.Controls.Add(btnHistory);
        }

        private void PopulateAbteilungDropdown()
        {
            var currentSelected = _cmbMitarbeiterAbteilung.SelectedItem?.ToString() ?? "Alle";
            
            _cmbMitarbeiterAbteilung.Items.Clear();
            _cmbMitarbeiterAbteilung.Items.Add("Alle");

            var depts = _dataManager.Contacts.OfType<Mitarbeiter>()
                .Select(m => m.Abteilung)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList();

            foreach (var d in depts)
            {
                _cmbMitarbeiterAbteilung.Items.Add(d);
            }

            if (_cmbMitarbeiterAbteilung.Items.Contains(currentSelected))
            {
                _cmbMitarbeiterAbteilung.SelectedItem = currentSelected;
            }
            else
            {
                _cmbMitarbeiterAbteilung.SelectedIndex = 0;
            }
        }

        private void RefreshMitarbeiterGrid()
        {
            PopulateAbteilungDropdown();

            string query = _txtMitarbeiterSearch.Text;
            var filtered = _dataManager.Search(query).OfType<Mitarbeiter>().ToList();

            // Apply department filter
            string deptFilter = _cmbMitarbeiterAbteilung.SelectedItem?.ToString() ?? "Alle";
            if (deptFilter != "Alle")
            {
                filtered = filtered.Where(m => m.Abteilung.Equals(deptFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply type filter
            string typeFilter = _cmbMitarbeiterTyp.SelectedItem?.ToString() ?? "Alle";
            if (typeFilter == "Mitarbeiter")
            {
                filtered = filtered.Where(m => !(m is Lernender)).ToList();
            }
            else if (typeFilter == "Lernende")
            {
                filtered = filtered.OfType<Lernender>().Cast<Mitarbeiter>().ToList();
            }

            _dgvMitarbeiter.DataSource = null;
            _dgvMitarbeiter.DataSource = filtered.Select(m => new
            {
                m.Id,
                MitarbeiterNr = m.MitarbeiterNummer,
                Nachname = m.Nachname,
                Vorname = m.Vorname,
                Abteilung = m.Abteilung,
                Rolle = m.Rolle,
                Eintritt = m.Eintrittsdatum.ToString("dd.MM.yyyy"),
                BesGrad = m.Beschäftigungsgrad.ToString("0") + "%",
                Typ = m is Lernender ? "Lernender" : "Regular",
                Status = m.IsActive ? "Aktiv" : "Passiv"
            }).ToList();

            if (_dgvMitarbeiter.Columns.Count > 0)
            {
                _dgvMitarbeiter.Columns["Id"].Visible = false; // Hide ID
            }
        }

        private void BtnMitarbeiterAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new MitarbeiterEditForm(_currentUser, _dataManager))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _dataManager.Contacts.Add(form.SavedMitarbeiter);
                    _dataManager.SaveData();
                    RefreshMitarbeiterGrid();
                    _lblStatusText.Text = $"Mitarbeiter '{form.SavedMitarbeiter.NameDisplay}' hinzugefügt.";
                }
            }
        }

        private void BtnMitarbeiterEdit_Click(object? sender, EventArgs e)
        {
            if (_dgvMitarbeiter.SelectedRows.Count == 0) return;
            string? id = _dgvMitarbeiter.SelectedRows[0].Cells["Id"].Value?.ToString();
            var mitarbeiter = _dataManager.Contacts.OfType<Mitarbeiter>().FirstOrDefault(m => m.Id == id);
            if (mitarbeiter == null) return;

            using (var form = new MitarbeiterEditForm(_currentUser, _dataManager, mitarbeiter))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Check if object type changed (Lernender <-> Mitarbeiter)
                    // If we save a new object (e.g. upgraded/downgraded), we replace it in the database
                    if (form.SavedMitarbeiter != mitarbeiter)
                    {
                        int idx = _dataManager.Contacts.IndexOf(mitarbeiter);
                        if (idx >= 0)
                        {
                            _dataManager.Contacts[idx] = form.SavedMitarbeiter;
                        }
                    }

                    _dataManager.SaveData();
                    RefreshMitarbeiterGrid();
                    _lblStatusText.Text = $"Mitarbeiter '{form.SavedMitarbeiter.NameDisplay}' aktualisiert.";
                }
            }
        }

        private void BtnMitarbeiterToggle_Click(object? sender, EventArgs e)
        {
            if (_dgvMitarbeiter.SelectedRows.Count == 0) return;
            string? id = _dgvMitarbeiter.SelectedRows[0].Cells["Id"].Value?.ToString();
            var mit = _dataManager.Contacts.OfType<Mitarbeiter>().FirstOrDefault(m => m.Id == id);
            if (mit == null) return;

            string oldStatus = mit.IsActive ? "Aktiv" : "Passiv";
            mit.IsActive = !mit.IsActive;
            string newStatus = mit.IsActive ? "Aktiv" : "Passiv";

            _dataManager.LogMutation(mit, "IsActive", oldStatus, newStatus, _currentUser);
            _dataManager.SaveData();
            RefreshMitarbeiterGrid();
            _lblStatusText.Text = $"Mitarbeiter '{mit.NameDisplay}' Status auf {newStatus} gesetzt.";
        }

        private void BtnMitarbeiterDelete_Click(object? sender, EventArgs e)
        {
            if (_dgvMitarbeiter.SelectedRows.Count == 0) return;
            string? id = _dgvMitarbeiter.SelectedRows[0].Cells["Id"].Value?.ToString();
            var mit = _dataManager.Contacts.OfType<Mitarbeiter>().FirstOrDefault(m => m.Id == id);
            if (mit == null) return;

            var res = MessageBox.Show($"Möchten Sie den Mitarbeiter '{mit.NameDisplay}' wirklich löschen?", "Löschen bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                _dataManager.Contacts.Remove(mit);
                _dataManager.SaveData();
                RefreshMitarbeiterGrid();
                _lblStatusText.Text = $"Mitarbeiter '{mit.NameDisplay}' gelöscht.";
            }
        }

        private void BtnMitarbeiterHistory_Click(object? sender, EventArgs e)
        {
            if (_dgvMitarbeiter.SelectedRows.Count == 0) return;
            string? id = _dgvMitarbeiter.SelectedRows[0].Cells["Id"].Value?.ToString();
            var mit = _dataManager.Contacts.OfType<Mitarbeiter>().FirstOrDefault(m => m.Id == id);
            if (mit == null) return;

            using (var form = new MutationHistoryForm(mit))
            {
                form.ShowDialog();
            }
        }
        #endregion

        #region CSV Import Setup & Logic
        private void SetupImport()
        {
            _pnlImport = new Panel { Dock = DockStyle.Fill, Visible = false };
            _pnlContentContainer.Controls.Add(_pnlImport);

            var lblTitle = new Label { Text = "CSV-Dateien importieren", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(0, 0), Size = new Size(300, 30) };
            _pnlImport.Controls.Add(lblTitle);

            // Instructions
            var lblInstruct = new Label
            {
                Text = "Hinweis: Die CSV-Datei muss eine Kopfzeile mit mindestens 'Typ', 'Vorname' und 'Nachname' haben. " +
                       "Optional: Anrede, Geschlecht, Titel, Mobiltelefonnummer, E-Mail-Adresse, Abteilung, Rolle, AHV-Nummer, Eintrittsdatum, Beschäftigungsgrad, Kaderstufe etc.",
                Location = new Point(0, 35),
                Size = new Size(760, 40),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray
            };
            _pnlImport.Controls.Add(lblInstruct);

            // Selection Bar
            var pnlTop = new Panel { Location = new Point(0, 75), Size = new Size(760, 45), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _pnlImport.Controls.Add(pnlTop);

            var lblFile = new Label { Text = "CSV-Datei:", Location = new Point(0, 10), Size = new Size(70, 20), TextAlign = ContentAlignment.MiddleLeft };
            pnlTop.Controls.Add(lblFile);

            _txtCsvPath = new TextBox { Location = new Point(75, 9), Size = new Size(380, 23) };
            pnlTop.Controls.Add(_txtCsvPath);

            var btnBrowse = new Button
            {
                Text = "Durchsuchen...",
                Location = new Point(465, 8),
                Size = new Size(110, 25),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnImportBrowse_Click;
            pnlTop.Controls.Add(btnBrowse);

            var btnPreview = new Button
            {
                Text = "Vorschau laden",
                Location = new Point(585, 8),
                Size = new Size(110, 25),
                BackColor = Color.FromArgb(9, 132, 227),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPreview.FlatAppearance.BorderSize = 0;
            btnPreview.Click += BtnImportPreview_Click;
            pnlTop.Controls.Add(btnPreview);

            // Grid
            _dgvImportPreview = new DataGridView
            {
                Location = new Point(0, 130),
                Size = new Size(760, 380),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _pnlImport.Controls.Add(_dgvImportPreview);

            // Bottom controls
            var pnlBottom = new Panel
            {
                Location = new Point(0, 520),
                Size = new Size(760, 45),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _pnlImport.Controls.Add(pnlBottom);

            _lblImportStatus = new Label
            {
                Text = "Keine Datei geladen.",
                Location = new Point(0, 10),
                Size = new Size(500, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnlBottom.Controls.Add(_lblImportStatus);

            var btnRun = new Button
            {
                Text = "Import ausführen",
                Location = new Point(600, 7),
                Size = new Size(150, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(235, 94, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Click += BtnImportRun_Click;
            pnlBottom.Controls.Add(btnRun);
            
            // Link button state to private flag
            this.Tag = btnRun;
        }

        private void BtnImportBrowse_Click(object? sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "CSV Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtCsvPath.Text = dialog.FileName;
                }
            }
        }

        private void BtnImportPreview_Click(object? sender, EventArgs e)
        {
            string path = _txtCsvPath.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine CSV-Datei aus.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var (imported, errors) = CsvImporter.ImportContacts(path, _dataManager);
            _previewList = imported;

            // Update preview grid
            _dgvImportPreview.DataSource = null;
            _dgvImportPreview.DataSource = _previewList.Select(p => new
            {
                Typ = p.TypDisplay,
                Nachname = p.Nachname,
                Vorname = p.Vorname,
                Geburtsdatum = p.Geburtsdatum.ToString("dd.MM.yyyy"),
                Mobil = p.Mobiltelefonnummer,
                EMail = p.EmailAdresse
            }).ToList();

            // Update status label
            if (errors.Any())
            {
                string errorSummary = string.Join("\n", errors.Take(3));
                if (errors.Count > 3) errorSummary += $"\n... und {errors.Count - 3} weitere Fehler.";
                MessageBox.Show($"Es gab Validierungsfehler beim Parsen:\n\n{errorSummary}", "Import Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _lblImportStatus.Text = $"{_previewList.Count} Kontakte geladen. Bereit zum Import.";
            
            var btnRun = this.Tag as Button;
            if (btnRun != null)
            {
                btnRun.Enabled = _previewList.Any();
            }
        }

        private void BtnImportRun_Click(object? sender, EventArgs e)
        {
            if (!_previewList.Any()) return;

            int count = 0;
            foreach (var p in _previewList)
            {
                _dataManager.Contacts.Add(p);
                _dataManager.LogMutation(p, "Erstellt", "Nicht vorhanden", "Durch CSV-Import importiert", _currentUser);
                count++;
            }

            _dataManager.SaveData();
            
            MessageBox.Show($"{count} Kontakte erfolgreich in die Datenbank importiert.", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            _previewList.Clear();
            _dgvImportPreview.DataSource = null;
            _lblImportStatus.Text = $"{count} Kontakte importiert.";
            _txtCsvPath.Clear();

            var btnRun = this.Tag as Button;
            if (btnRun != null)
            {
                btnRun.Enabled = false;
            }

            RefreshDashboardData();
        }
        #endregion
    }
}
