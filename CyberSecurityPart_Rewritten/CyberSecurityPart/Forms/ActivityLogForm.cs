using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CyberSecurityAwarenessBot.Forms
{
    /// <summary>
    /// Standalone window that presents the bot's full activity history.
    /// Shows the most recent entries first, in pages of 10, with a
    /// "Show more" button to reveal older entries on demand.
    /// </summary>
    public class ActivityLogForm : Form
    {
        private const int PageSize = 10;

        private static readonly Color C_Bg = Color.FromArgb(14, 11, 28);
        private static readonly Color C_Panel = Color.FromArgb(20, 16, 40);
        private static readonly Color C_Card = Color.FromArgb(26, 21, 50);
        private static readonly Color C_Purple = Color.FromArgb(140, 80, 255);
        private static readonly Color C_Violet = Color.FromArgb(185, 130, 255);
        private static readonly Color C_Mint = Color.FromArgb(48, 220, 175);
        private static readonly Color C_TextHi = Color.FromArgb(242, 238, 255);
        private static readonly Color C_TextMid = Color.FromArgb(160, 150, 200);
        private static readonly Color C_Border = Color.FromArgb(50, 42, 85);
        private static readonly Color C_RowEven = Color.FromArgb(22, 18, 44);
        private static readonly Color C_RowOdd = Color.FromArgb(30, 24, 56);

        private ListView _listView = null!;
        private Button _showMoreBtn = null!;
        private Label _countLabel = null!;

        private int _visibleCount = PageSize;

        public ActivityLogForm()
        {
            InitForm();
            Populate();
        }

        private void InitForm()
        {
            Text = "CyberBot  |  Activity Log";
            Size = new Size(680, 560);
            MinimumSize = new Size(520, 420);
            BackColor = C_Bg;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);

            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = C_Panel };
            var headerTitle = new Label
            {
                Text = "📜  Activity Log",
                Dock = DockStyle.Fill,
                ForeColor = C_Purple,
                Font = new Font("Segoe UI Black", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            header.Controls.Add(headerTitle);
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = C_Border });

            _countLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = C_TextMid,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = C_Card
            };

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
            _listView.Columns.Add("Time", 130);
            _listView.Columns.Add("Action", 440);
            _listView.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 26, 60)), e.Bounds);
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                TextRenderer.DrawText(e.Graphics, e.Header!.Text, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    e.Bounds, C_Violet, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            };
            _listView.DrawItem += (s, e) => e.DrawBackground();
            _listView.DrawSubItem += (s, e) =>
            {
                Color bg = e.Item!.Selected ? Color.FromArgb(60, 44, 110) : (e.ItemIndex % 2 == 0 ? C_RowEven : C_RowOdd);
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.SubItem!.Text, new Font("Segoe UI", 9.5f), e.Bounds,
                    e.ColumnIndex == 1 ? C_Mint : C_TextHi,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            };

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = C_Panel, Padding = new Padding(10) };
            bottomBar.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = C_Border });

            _showMoreBtn = new Button
            {
                Text = "  ⌄  Show more",
                Dock = DockStyle.Left,
                Width = 150,
                Height = 34,
                BackColor = C_Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _showMoreBtn.FlatAppearance.BorderSize = 0;
            _showMoreBtn.FlatAppearance.MouseOverBackColor = C_Violet;
            _showMoreBtn.Click += (s, e) => { _visibleCount += PageSize; Populate(); };

            var refreshBtn = new Button
            {
                Text = "  ↺  Refresh",
                Dock = DockStyle.Right,
                Width = 110,
                Height = 34,
                BackColor = Color.FromArgb(40, 34, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.Click += (s, e) => Populate();

            bottomBar.Controls.Add(refreshBtn);
            bottomBar.Controls.Add(_showMoreBtn);

            Controls.Add(_listView);
            Controls.Add(bottomBar);
            Controls.Add(_countLabel);
            Controls.Add(header);
        }

        /// <summary>Redraws the list with the current page size, newest entries first.</summary>
        private void Populate()
        {
            var all = ActivityLog.Instance.GetAll().Reverse().ToList(); // newest first
            var page = all.Take(_visibleCount).ToList();

            _listView.Items.Clear();
            for (int i = 0; i < page.Count; i++)
            {
                var entry = page[i];
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(entry.Timestamp.ToString("dd MMM, HH:mm"));
                item.SubItems.Add(entry.Description);
                item.BackColor = (i % 2 == 0) ? C_RowEven : C_RowOdd;
                item.ForeColor = C_TextHi;
                _listView.Items.Add(item);
            }

            _countLabel.Text = all.Count == 0
                ? "No actions recorded yet — start chatting, take a quiz, or add a task!"
                : $"Showing {page.Count} of {all.Count} action(s)";

            _showMoreBtn.Visible = page.Count < all.Count;
        }
    }
}
