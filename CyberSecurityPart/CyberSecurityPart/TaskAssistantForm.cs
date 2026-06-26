using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CyberSecurityAwarenessBot.Forms
{
   
    /// cybersecurity tasks with optional date reminders — all persisted to MySQL.
   
    public class TaskAssistantForm : Form
    {
       
        private static readonly Color C_Bg = Color.FromArgb(14, 11, 28);
        private static readonly Color C_Card = Color.FromArgb(26, 21, 50);
        private static readonly Color C_Panel = Color.FromArgb(20, 16, 40);
        private static readonly Color C_Purple = Color.FromArgb(140, 80, 255);
        private static readonly Color C_Violet = Color.FromArgb(185, 130, 255);
        private static readonly Color C_Mint = Color.FromArgb(48, 220, 175);
        private static readonly Color C_Pink = Color.FromArgb(255, 75, 180);
        private static readonly Color C_TextHi = Color.FromArgb(242, 238, 255);
        private static readonly Color C_TextMid = Color.FromArgb(160, 150, 200);
        private static readonly Color C_TextLo = Color.FromArgb(95, 85, 135);
        private static readonly Color C_Border = Color.FromArgb(50, 42, 85);
        private static readonly Color C_RowEven = Color.FromArgb(22, 18, 44);
        private static readonly Color C_RowOdd = Color.FromArgb(30, 24, 56);
        private static readonly Color C_BtnDanger = Color.FromArgb(160, 40, 55);
        private static readonly Color C_BtnSuccess = Color.FromArgb(30, 130, 70);

        private ListView _listView = null!;
        private TextBox _titleBox = null!;
        private TextBox _descBox = null!;
        private CheckBox _reminderChk = null!;
        private DateTimePicker _reminderPicker = null!;
        private Button _addBtn = null!;
        private Button _completeBtn = null!;
        private Button _deleteBtn = null!;
        private Label _statusLabel = null!;

        
        private readonly string _userName;

        public TaskAssistantForm(string userName)
        {
            _userName = userName;
            InitForm();
            LoadTasks();
        }

        private void InitForm()
        {
            Text = "CyberBot  |  Task Assistant";
            Size = new Size(860, 620);
            MinimumSize = new Size(720, 520);
            BackColor = C_Bg;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);
            FormBorderStyle = FormBorderStyle.Sizable;

           
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(20, 16, 40)
            };
            var headerTitle = new Label
            {
                Text = "📋  Cybersecurity Task Assistant",
                Dock = DockStyle.Fill,
                ForeColor = C_Purple,
                Font = new Font("Segoe UI Black", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            header.Controls.Add(headerTitle);
            header.Controls.Add(new Panel
            { Dock = DockStyle.Bottom, Height = 1, BackColor = C_Border });

           
            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                Text = "Ready",
                ForeColor = C_TextMid,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(16, 12, 32),
                Padding = new Padding(10, 0, 0, 0)
            };

            
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 260,
                BackColor = C_Panel,
                Padding = new Padding(14, 10, 14, 10)
            };

            var formTitle = new Label
            {
                Text = "➕  Add New Task",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = C_Mint,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            var titleLbl = MakeFormLabel("Task Title *");
            _titleBox = new TextBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 24, 56),
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "e.g. Enable two-factor authentication"
            };

            var descLbl = MakeFormLabel("Description");
            _descBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 68,
                Multiline = true,
                BackColor = Color.FromArgb(30, 24, 56),
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Optional — extra details about this task",
                ScrollBars = ScrollBars.Vertical
            };

            _reminderChk = new CheckBox
            {
                Text = "Set a reminder",
                Dock = DockStyle.Top,
                Height = 26,
                ForeColor = C_TextMid,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.Transparent,
                Checked = false
            };
            _reminderChk.CheckedChanged += (s, e) =>
                _reminderPicker.Visible = _reminderChk.Checked;

            _reminderPicker = new DateTimePicker
            {
                Dock = DockStyle.Top,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today.AddDays(1),
                Value = DateTime.Today.AddDays(7),
                BackColor = Color.FromArgb(30, 24, 56),
                ForeColor = C_TextHi,
                CalendarForeColor = C_TextHi,
                CalendarMonthBackground = C_Card,
                Visible = false
            };

            _addBtn = MakeButton("  ✚  Add Task", C_Purple);
            _addBtn.Click += AddBtn_Click;

            var sep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = C_Border,
                Margin = new Padding(0, 6, 0, 6)
            };

            // Stack controls — DockStyle.Top renders bottom-to-top so add in reverse
            rightPanel.Controls.Add(_addBtn);
            rightPanel.Controls.Add(MakeSpacer(6));
            rightPanel.Controls.Add(_reminderPicker);
            rightPanel.Controls.Add(_reminderChk);
            rightPanel.Controls.Add(MakeSpacer(4));
            rightPanel.Controls.Add(_descBox);
            rightPanel.Controls.Add(descLbl);
            rightPanel.Controls.Add(MakeSpacer(4));
            rightPanel.Controls.Add(_titleBox);
            rightPanel.Controls.Add(titleLbl);
            rightPanel.Controls.Add(MakeSpacer(8));
            rightPanel.Controls.Add(formTitle);

            
            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BackColor = C_Bg,
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.None,
                MultiSelect = false,
                OwnerDraw = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _listView.Columns.Add("#", 36);
            _listView.Columns.Add("Title", 190);
            _listView.Columns.Add("Description", 220);
            _listView.Columns.Add("Reminder", 100);
            _listView.Columns.Add("Status", 90);
            _listView.Columns.Add("Created", 90);

            _listView.DrawColumnHeader += ListView_DrawColumnHeader;
            _listView.DrawItem += ListView_DrawItem;
            _listView.DrawSubItem += ListView_DrawSubItem;

            
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = C_Panel,
                Padding = new Padding(8, 6, 8, 6)
            };
            bottomBar.Controls.Add(new Panel
            { Dock = DockStyle.Top, Height = 1, BackColor = C_Border });

            _completeBtn = MakeButton("  ✔  Mark Completed", C_BtnSuccess);
            _completeBtn.Dock = DockStyle.Left;
            _completeBtn.Width = 170;
            _completeBtn.Click += CompleteBtn_Click;

            _deleteBtn = MakeButton("  🗑  Delete Task", C_BtnDanger);
            _deleteBtn.Dock = DockStyle.Left;
            _deleteBtn.Width = 150;
            _deleteBtn.Click += DeleteBtn_Click;

            var refreshBtn = MakeButton("  ↺  Refresh", Color.FromArgb(40, 34, 76));
            refreshBtn.Dock = DockStyle.Right;
            refreshBtn.Width = 110;
            refreshBtn.Click += (s, e) => LoadTasks();

            bottomBar.Controls.Add(refreshBtn);
            bottomBar.Controls.Add(_deleteBtn);
            bottomBar.Controls.Add(MakeSpacer(8, horizontal: true));
            bottomBar.Controls.Add(_completeBtn);

            Controls.Add(_listView);
            Controls.Add(rightPanel);
            Controls.Add(bottomBar);
            Controls.Add(_statusLabel);
            Controls.Add(header);
        }

        

        private void LoadTasks()
        {
            _listView.Items.Clear();
            try
            {
                var tasks = DatabaseHelper.GetAllTasks();
                int row = 0;
                foreach (var t in tasks)
                {
                    var item = new ListViewItem((row + 1).ToString());
                    item.SubItems.Add(t.Title);
                    item.SubItems.Add(t.Description.Length > 60
                        ? t.Description[..60] + "…"
                        : t.Description);
                    item.SubItems.Add(t.ReminderDisplay);
                    item.SubItems.Add(t.StatusDisplay);
                    item.SubItems.Add(t.CreatedAt.ToString("dd MMM yy"));
                    item.Tag = t.Id;
                    item.BackColor = (row % 2 == 0) ? C_RowEven : C_RowOdd;
                    item.ForeColor = t.Status == TaskStatus.Completed
                                        ? Color.FromArgb(100, 180, 100)
                                        : C_TextHi;
                    _listView.Items.Add(item);
                    row++;
                }
                SetStatus($"Loaded {tasks.Count} task(s).");
            }
            catch (Exception ex)
            {
                SetStatus($"DB error: {ex.Message}");
                MessageBox.Show(
                    "Could not connect to the database.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddBtn_Click(object? sender, EventArgs e)
        {
            string title = _titleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                SetStatus("⚠ Please enter a task title.");
                _titleBox.Focus();
                return;
            }

            var task = new CyberTask
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(_descBox.Text)
                                ? AutoDescription(title)
                                : _descBox.Text.Trim(),
                ReminderAt = _reminderChk.Checked ? _reminderPicker.Value.Date : null,
                Status = TaskStatus.Pending,
                CreatedAt = DateTime.Now
            };

            try
            {
                int newId = DatabaseHelper.AddTask(task);
                task.Id = newId;

                string logMsg = $"Task added: '{task.Title}'";
                if (task.ReminderAt.HasValue)
                    logMsg += $" (Reminder set for {task.ReminderAt.Value:dd MMM yyyy})";
                ActivityLog.Instance.Add(logMsg);

                if (task.ReminderAt.HasValue)
                    ActivityLog.Instance.Add(
                        $"Reminder set: '{task.Title}' on {task.ReminderAt.Value:dd MMM yyyy}");

                _titleBox.Clear();
                _descBox.Clear();
                _reminderChk.Checked = false;
                _reminderPicker.Value = DateTime.Today.AddDays(7);

                LoadTasks();
                SetStatus($"✔ Task '{task.Title}' added successfully.");
            }
            catch (Exception ex)
            {
                SetStatus($"DB error: {ex.Message}");
                MessageBox.Show("Failed to save task:\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CompleteBtn_Click(object? sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0)
            { SetStatus("⚠ Select a task first."); return; }

            int id = (int)_listView.SelectedItems[0].Tag!;
            string ttl = _listView.SelectedItems[0].SubItems[1].Text;

            try
            {
                DatabaseHelper.MarkCompleted(id);
                ActivityLog.Instance.Add($"Task completed: '{ttl}'");
                LoadTasks();
                SetStatus($"✔ '{ttl}' marked as completed.");
            }
            catch (Exception ex)
            {
                SetStatus($"DB error: {ex.Message}");
            }
        }

        private void DeleteBtn_Click(object? sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0)
            { SetStatus("⚠ Select a task to delete."); return; }

            string ttl = _listView.SelectedItems[0].SubItems[1].Text;
            int id = (int)_listView.SelectedItems[0].Tag!;

            var confirm = MessageBox.Show(
                $"Permanently delete '{ttl}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                DatabaseHelper.DeleteTask(id);
                ActivityLog.Instance.Add($"Task deleted: '{ttl}'");
                LoadTasks();
                SetStatus($"🗑 '{ttl}' deleted.");
            }
            catch (Exception ex)
            {
                SetStatus($"DB error: {ex.Message}");
            }
        }

        

        private static string AutoDescription(string title)
        {
            string t = title.ToLower();
            if (t.Contains("2fa") || t.Contains("two-factor") || t.Contains("two factor"))
                return "Enable two-factor authentication to add an extra layer of account security.";
            if (t.Contains("password"))
                return "Review and update your passwords to ensure they are strong and unique.";
            if (t.Contains("privacy"))
                return "Review account privacy settings to ensure your data is protected.";
            if (t.Contains("backup"))
                return "Back up important data to protect against ransomware and data loss.";
            if (t.Contains("update") || t.Contains("patch"))
                return "Apply the latest security updates and patches to your software.";
            if (t.Contains("vpn"))
                return "Set up a VPN to encrypt your internet traffic on public networks.";
            if (t.Contains("antivirus") || t.Contains("malware"))
                return "Install or update antivirus/anti-malware software to protect your device.";
            return $"Complete the cybersecurity task: {title}.";
        }

        private void SetStatus(string msg) => _statusLabel.Text = "  " + msg;

        private static Button MakeButton(string text, Color bg)
        {
            var btn = new Button
            {
                Text = text,
                Height = 34,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static Label MakeFormLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 20,
            ForeColor = Color.FromArgb(140, 80, 255),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            BackColor = Color.Transparent
        };

        private static Panel MakeSpacer(int height, bool horizontal = false) =>
            new Panel
            {
                Dock = horizontal ? DockStyle.Left : DockStyle.Top,
                Height = horizontal ? 1 : height,
                Width = horizontal ? height : 1,
                BackColor = Color.Transparent
            };

       

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 26, 60)), e.Bounds);
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen,
                e.Bounds.Left, e.Bounds.Bottom - 1,
                e.Bounds.Right, e.Bounds.Bottom - 1);
            TextRenderer.DrawText(e.Graphics, e.Header!.Text, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                e.Bounds, Color.FromArgb(185, 130, 255),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void ListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
        }

        private void ListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = e.Item!.Selected;
            Color bg = selected
                ? Color.FromArgb(60, 44, 110)
                : (e.ItemIndex % 2 == 0 ? C_RowEven : C_RowOdd);

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

            Color fg = e.Item.ForeColor;
            if (e.ColumnIndex == 4) 
            {
                fg = e.SubItem!.Text.StartsWith("✔") ? Color.FromArgb(80, 200, 120) : C_Mint;
            }

            TextRenderer.DrawText(e.Graphics, e.SubItem!.Text,
                new Font("Segoe UI", 9.5f), e.Bounds, fg,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
