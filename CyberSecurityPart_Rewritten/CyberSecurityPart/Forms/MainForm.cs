using CyberSecurityAwarenessBot.Helpers;
using CyberSecurityAwarenessBot.Memory;
using CyberSecurityAwarenessBot.Responses;
using CyberSecurityAwarenessBot.Sentiment;

namespace CyberSecurityAwarenessBot.Forms
{
  
    public class MainForm : Form
    {
        private Panel _sidebar = null!;
        private Panel _mainArea = null!;
        private Panel _topBar = null!;
        private Panel _inputBar = null!;
        private Panel _chatScroll = null!;
        private FlowLayoutPanel _messages = null!;

       
        private Label _userCard = null!;
        private FlowLayoutPanel _topicBtns = null!;
        private Label _moodLabel = null!;
        private TextBox _inputBox = null!;
        private Button _sendBtn = null!;
        private readonly ResponseEngine _engine;
        private readonly UserMemory _memory;
        private readonly SentimentDetector _detector;
        private readonly AudioHelper _audio;

        
        private readonly System.Windows.Forms.Timer _reminderTimer = new() { Interval = 60_000 }; 
        private readonly NotifyIcon _trayIcon = new() { Icon = SystemIcons.Information, Visible = false };
        private readonly HashSet<int> _notifiedTaskIds = new();

        
        private bool _nameCollected = false;
        private string _lastTopic = string.Empty;

        private bool _awaitingTaskDesc = false;
        private string _pendingTaskTitle = string.Empty;
        private bool _awaitingReminder = false;
        private int _pendingTaskId = -1;

       
        private static readonly Color C_PageBg = Color.FromArgb(14, 11, 28);
        private static readonly Color C_SidebarBg = Color.FromArgb(20, 16, 40);
        private static readonly Color C_TopBarBg = Color.FromArgb(26, 21, 50);
        private static readonly Color C_InputBg = Color.FromArgb(20, 16, 40);
        private static readonly Color C_BotBubble = Color.FromArgb(32, 26, 60);
        private static readonly Color C_UserBubble = Color.FromArgb(90, 46, 186);
        private static readonly Color C_Purple = Color.FromArgb(140, 80, 255);
        private static readonly Color C_Violet = Color.FromArgb(185, 130, 255);
        private static readonly Color C_Mint = Color.FromArgb(48, 220, 175);
        private static readonly Color C_TextHi = Color.FromArgb(242, 238, 255);
        private static readonly Color C_TextMid = Color.FromArgb(160, 150, 200);
        private static readonly Color C_TextLo = Color.FromArgb(95, 85, 135);
        private static readonly Color C_Border = Color.FromArgb(50, 42, 85);
        private static readonly Color C_BtnHover = Color.FromArgb(52, 44, 90);
        private static readonly Color C_QuizBtn = Color.FromArgb(30, 90, 180);
        private static readonly Color C_QuizBtnHover = Color.FromArgb(50, 120, 220);
        private static readonly Color C_TaskBtn = Color.FromArgb(20, 110, 80);
        private static readonly Color C_TaskBtnHover = Color.FromArgb(30, 150, 100);

        private static readonly string[] ShortcutTopics =
        {
            "Passwords", "Phishing", "Malware", "Ransomware",
            "2FA", "VPN", "Encryption", "Privacy",
            "Safe Browsing", "Data Breach", "Social Engineering", "Help"
        };

        public MainForm()
        {
            _engine = new ResponseEngine();
            _memory = new UserMemory();
            _detector = new SentimentDetector();
            _audio = new AudioHelper();

            InitForm();
            Task.Run(() => _audio.PlayVoiceGreeting());

            
            Task.Run(() =>
            {
                try { DatabaseHelper.Initialise(); }
                catch { /* silently handled; user sees error only when they open Tasks */ }
            });

            AddBotBubble(
                "Hey! I am CyberBot, your personal cybersecurity guide.\n\n" +
                "I can help you stay safe online — ask me anything, pick a topic\n" +
                "from the sidebar, open the Task Assistant, or take a quiz!\n\n" +
                "What is your name?");

            
            _reminderTimer.Tick += (s, e) => CheckReminders();
            _reminderTimer.Start();
            FormClosed += (s, e) =>
            {
                _reminderTimer.Stop();
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            };
        }

        
        /// Looks for pending tasks whose reminder date has arrived and notifies
        /// the user exactly once per task (tracked via <see cref="_notifiedTaskIds"/>),
        /// via both a desktop tray balloon and a chat bubble.
       
        private void CheckReminders()
        {
            if (!_nameCollected) return;

            Task.Run(() =>
            {
                List<CyberTask>? due = null;
                try { due = DatabaseHelper.GetDueReminders(); }
                catch { /* DB unreachable — skip silently, will retry next tick */ }
                if (due == null) return;

                var fresh = due.Where(t => !_notifiedTaskIds.Contains(t.Id)).ToList();
                if (fresh.Count == 0) return;

                foreach (var t in fresh) _notifiedTaskIds.Add(t.Id);

                if (IsHandleCreated)
                {
                    Invoke(() =>
                    {
                        string list = string.Join("\n", fresh.Select((t, i) => $"  {i + 1}. {t.Title}"));
                        AddBotBubble(
                            $"⏰ Reminder! {(fresh.Count == 1 ? "A task" : $"{fresh.Count} tasks")} " +
                            $"{(fresh.Count == 1 ? "is" : "are")} due today:\n{list}\n\n" +
                            "Open the 📋 Task Assistant to complete or reschedule them.");

                        _trayIcon.Visible = true;
                        _trayIcon.BalloonTipTitle = "CyberBot — Task Reminder";
                        _trayIcon.BalloonTipText = fresh.Count == 1
                            ? $"Reminder: \"{fresh[0].Title}\" is due today."
                            : $"You have {fresh.Count} security tasks due today.";
                        _trayIcon.ShowBalloonTip(6000);

                        ActivityLog.Instance.Add(
                            $"Reminder triggered for: {string.Join(", ", fresh.Select(t => $"'{t.Title}'"))}");
                    });
                }
            });
        }

        

        private void InitForm()
        {
            Text = "CyberBot  |  Security Awareness Assistant";
            Size = new Size(1040, 700);
            MinimumSize = new Size(780, 560);
            BackColor = C_PageBg;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);

            BuildTopBar();
            BuildSidebar();
            BuildMainArea();
            BuildInputBar();

            Controls.Add(_mainArea);
            Controls.Add(_sidebar);
            Controls.Add(_inputBar);
            Controls.Add(_topBar);
            _inputBox.Select();
        }

        private void BuildTopBar()
        {
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = C_TopBarBg,
                Padding = new Padding(12, 0, 16, 0)
            };

            var avatar = new Label
            {
                Text = "CB",
                Size = new Size(34, 34),
                Location = new Point(14, 9),
                BackColor = C_Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var name = new Label
            {
                Text = "CyberBot",
                Location = new Point(57, 7),
                Size = new Size(220, 20),
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            var sub = new Label
            {
                Text = "Online  •  Cybersecurity ChatBot",
                Location = new Point(58, 28),
                Size = new Size(280, 16),
                ForeColor = C_Mint,
                Font = new Font("Segoe UI", 8f),
                BackColor = Color.Transparent
            };
            _moodLabel = new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Right,
                Width = 240,
                ForeColor = C_Violet,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 4, 0)
            };
            var border = new Panel
            { Dock = DockStyle.Bottom, Height = 1, BackColor = C_Border };

            _topBar.Controls.AddRange(new Control[]
                { _moodLabel, border, sub, name, avatar });
        }

        private void BuildSidebar()
        {
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = C_SidebarBg
            };

            // Logo
            var logo = new Label
            {
                Text = "⬡ CYBER\n    BOT",
                Dock = DockStyle.Top,
                Height = 68,
                ForeColor = C_Purple,
                Font = new Font("Segoe UI Black", 15f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(16, 12, 32)
            };

            // User info card
            _userCard = new Label
            {
                Text = "Welcome!\nStart chatting below.",
                Dock = DockStyle.Top,
                Height = 54,
                ForeColor = C_TextMid,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(26, 20, 50),
                Padding = new Padding(6)
            };

            var sep1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = C_Border };

            var heading = new Label
            {
                Text = "  QUICK TOPICS",
                Dock = DockStyle.Top,
                Height = 26,
                ForeColor = C_TextLo,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            var bottomSep = new Panel
            { Dock = DockStyle.Bottom, Height = 1, BackColor = C_Border };

            var quizBtn = MakeSidebarActionButton("🎯  Take a Quiz", C_QuizBtn, C_QuizBtnHover);
            quizBtn.Click += QuizBtn_Click;

            var taskBtn = MakeSidebarActionButton("📋  Task Assistant", C_TaskBtn, C_TaskBtnHover);
            taskBtn.Click += TaskBtn_Click;

            var logBtn = MakeSidebarActionButton("📜  Activity Log", Color.FromArgb(70, 50, 110),
                Color.FromArgb(100, 70, 160));
            logBtn.Click += (s, e) =>
            {
                if (!_nameCollected) { AddBotBubble("Please tell me your name first!"); return; }
                ActivityLog.Instance.Add("Activity log viewed by user");
                using var logForm = new ActivityLogForm();
                logForm.ShowDialog(this);
            };

            
            _topicBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(6, 4, 6, 4)
            };

            foreach (string topic in ShortcutTopics)
            {
                var btn = new Button
                {
                    Text = "  " + topic,
                    Size = new Size(182, 30),
                    BackColor = Color.FromArgb(30, 24, 56),
                    ForeColor = C_TextMid,
                    Font = new Font("Segoe UI", 9f),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 2, 0, 2),
                    Tag = topic
                };
                btn.FlatAppearance.BorderColor = C_Border;
                btn.FlatAppearance.BorderSize = 1;
                btn.MouseEnter += (s, e) => { btn.BackColor = C_BtnHover; btn.ForeColor = C_Violet; };
                btn.MouseLeave += (s, e) =>
                { btn.BackColor = Color.FromArgb(30, 24, 56); btn.ForeColor = C_TextMid; };
                btn.Click += TopicBtn_Click;
                _topicBtns.Controls.Add(btn);
            }

            
            _sidebar.Controls.Add(_topicBtns);   
            _sidebar.Controls.Add(bottomSep);     
            _sidebar.Controls.Add(logBtn);        
            _sidebar.Controls.Add(quizBtn);       
            _sidebar.Controls.Add(taskBtn);       
            _sidebar.Controls.Add(heading);       
            _sidebar.Controls.Add(sep1);          
            _sidebar.Controls.Add(_userCard);     
            _sidebar.Controls.Add(logo);          
        }

        private static Button MakeSidebarActionButton(string text, Color bg, Color hover)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hover;
            return btn;
        }

        private void BuildMainArea()
        {
            _mainArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PageBg,
                Padding = new Padding(0)
            };
            _chatScroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PageBg,
                AutoScroll = true,
                Padding = new Padding(14, 10, 14, 10)
            };
            _messages = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = C_PageBg,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0)
            };
            _chatScroll.Controls.Add(_messages);
            _mainArea.Controls.Add(_chatScroll);
        }

        private void BuildInputBar()
        {
            _inputBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = C_InputBg,
                Padding = new Padding(14, 10, 14, 10)
            };
            var topLine = new Panel
            { Dock = DockStyle.Top, Height = 1, BackColor = C_Border };

            _sendBtn = new Button
            {
                Text = "Send  ➤",
                Dock = DockStyle.Right,
                Width = 96,
                BackColor = C_Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _sendBtn.FlatAppearance.BorderSize = 0;
            _sendBtn.FlatAppearance.MouseOverBackColor = C_Violet;
            _sendBtn.Click += SendBtn_Click;

            _inputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 24, 56),
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.None,
                PlaceholderText = "Type a message or pick a topic..."
            };
            _inputBox.KeyDown += InputBox_KeyDown;

            _inputBar.Controls.Add(_inputBox);
            _inputBar.Controls.Add(_sendBtn);
            _inputBar.Controls.Add(topLine);
        }

        

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ProcessInput(); }
        }

        private void SendBtn_Click(object? sender, EventArgs e) => ProcessInput();

        private void TopicBtn_Click(object? sender, EventArgs e)
        {
            if (sender is Button b && b.Tag is string t)
            { _inputBox.Text = t; ProcessInput(); }
        }

        private void QuizBtn_Click(object? sender, EventArgs e)
        {
            if (!_nameCollected)
            { AddBotBubble("Please tell me your name first, then we can start the quiz! 😊"); return; }

            ActivityLog.Instance.Add("Quiz started by user");
            using var quiz = new QuizForm(_memory.Name);
            quiz.ShowDialog(this);

            if (quiz.QuestionsAnswered > 0)
            {
                ActivityLog.Instance.Add(
                    $"Quiz completed — Score: {quiz.Score}/{quiz.QuestionsAnswered}");

                AddBotBubble(
                    $"Great effort, {_memory.Name}! You scored " +
                    $"{quiz.Score} / {quiz.QuestionsAnswered}.\n\n" +
                    (quiz.Score == quiz.QuestionsAnswered
                        ? "Perfect score — you are a cybersecurity pro! 🏆"
                        : quiz.Score >= quiz.QuestionsAnswered / 2
                            ? "Solid work! Keep exploring the topics on the left to sharpen up. 💪"
                            : "No worries — use the topic buttons to brush up and try again! 📚"));
            }
        }

        private void TaskBtn_Click(object? sender, EventArgs e)
        {
            if (!_nameCollected)
            { AddBotBubble("Please tell me your name first so I can personalise your task list!"); return; }

            ActivityLog.Instance.Add("Task Assistant opened by user");
            using var tasks = new TaskAssistantForm(_memory.Name);
            tasks.ShowDialog(this);
            ActivityLog.Instance.Add("Task Assistant closed");
        }

        
        //  CORE INPUT PROCESSING (NLP routing lives here)
        

        private void ProcessInput()
        {
            string raw = _inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(raw)) return;
            _inputBox.Clear();

            // Exit command
            if (raw.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                AddUserBubble(raw);
                AddBotBubble($"Take care, {_memory.Name}! Stay safe online. 👋");
                return;
            }

            AddUserBubble(raw);

            // ── Name collection (first interaction) ───────────────────────
            if (!_nameCollected)
            {
                _memory.SetName(raw);
                _nameCollected = true;
                RefreshUserCard();

                
                CheckReminders();

                AddBotBubble(
                    $"Great to meet you, {_memory.Name}! \n\n" +
                    "Here is what I can do:\n" +
                    "• Answer cybersecurity questions (pick a topic or just type)\n" +
                    "• Manage your security tasks with reminders\n" +
                    "• Quiz you on cybersecurity knowledge\n" +
                    "• Show your activity log (type \"show activity log\")\n\n" +
                    "What would you like to do?");
                return;
            }

          
            if (_awaitingTaskDesc)
            {
                _awaitingTaskDesc = false;
                string desc = raw;
                var task = new CyberTask
                {
                    Title = _pendingTaskTitle,
                    Description = desc,
                    Status = TaskStatus.Pending,
                    CreatedAt = DateTime.Now
                };
                try
                {
                    _pendingTaskId = DatabaseHelper.AddTask(task);
                    ActivityLog.Instance.Add($"Task added: '{_pendingTaskTitle}'");
                    _awaitingReminder = true;
                    AddBotBubble(
                        $"Task added: \"{_pendingTaskTitle}\" ✔\n\n" +
                        "Would you like a reminder? Reply with:\n" +
                        "• A number of days (e.g. \"7\" or \"remind me in 3 days\")\n" +
                        "• A specific date (e.g. \"15 July\")\n" +
                        "• Or \"no\" to skip");
                }
                catch (Exception ex)
                {
                    AddBotBubble($"I could not save the task to the database.\nError: {ex.Message}");
                }
                return;
            }

            
            if (_awaitingReminder)
            {
                _awaitingReminder = false;
                string lower = raw.ToLower();

                if (lower.Contains("no") || lower.Contains("skip") || lower.Contains("none"))
                {
                    AddBotBubble("No reminder set. Your task is saved — open the Task Assistant to manage it.");
                    return;
                }

                DateTime? reminderDate = ParseReminderDate(raw);
                if (reminderDate.HasValue && _pendingTaskId > 0)
                {
                    try
                    {
                        DatabaseHelper.UpdateReminder(_pendingTaskId, reminderDate.Value);
                        ActivityLog.Instance.Add(
                            $"Reminder set: '{_pendingTaskTitle}' on {reminderDate.Value:dd MMM yyyy}");
                        AddBotBubble(
                            $"Got it! I'll remind you on {reminderDate.Value:dd MMM yyyy}. ✔\n\n" +
                            "You can view and manage all tasks via the 📋 Task Assistant button.");
                    }
                    catch (Exception ex)
                    {
                        AddBotBubble($"Could not set reminder in database:\n{ex.Message}");
                    }
                }
                else
                {
                    AddBotBubble(
                        "I could not parse that date. Open the Task Assistant to set a reminder manually.");
                }
                return;
            }

            //NLP Keyword detection 
            string input = raw.ToLower();
            bool handled = false;

            // Activity log commands
            if (NlpMatch(input, "show activity log", "what have you done", "activity log",
                                "recent actions", "what did you do", "show log"))
            {
                AddBotBubble(ActivityLog.Instance.FormatRecent(5) +
                             "\n\nOpening the full Activity Log window for you…");
                ActivityLog.Instance.Add("Activity log viewed by user");
                using var logForm = new ActivityLogForm();
                logForm.ShowDialog(this);
                handled = true;
            }

            // Quiz launch via chat
            else if (NlpMatch(input, "start quiz", "take quiz", "play quiz", "quiz me",
                                     "begin quiz", "cybersecurity quiz", "test my knowledge"))
            {
                ActivityLog.Instance.Add("Quiz started via chat command");
                var quiz = new QuizForm(_memory.Name);
                quiz.ShowDialog(this);
                if (quiz.QuestionsAnswered > 0)
                {
                    ActivityLog.Instance.Add(
                        $"Quiz completed — Score: {quiz.Score}/{quiz.QuestionsAnswered}");
                    AddBotBubble(
                        $"You scored {quiz.Score} / {quiz.QuestionsAnswered}. " +
                        (quiz.Score == quiz.QuestionsAnswered ? "Perfect! 🏆" : "Keep it up! 💪"));
                }
                handled = true;
            }

            // Task assistant via chat
            else if (NlpMatch(input, "show tasks", "my tasks", "view tasks",
                                     "open task assistant", "task list", "manage tasks"))
            {
                ActivityLog.Instance.Add("Task Assistant opened via chat command");
                var form = new TaskAssistantForm(_memory.Name);
                form.ShowDialog(this);
                handled = true;
            }

            
            else if (NlpMatch(input, "add task", "create task", "new task", "add a task",
                                     "remind me to", "remind me about", "set a reminder",
                                     "i need to", "don't let me forget"))
            {
                string title = ExtractTaskTitle(raw);
                if (string.IsNullOrWhiteSpace(title))
                {
                    AddBotBubble("What would you like to add as a task? Please type the task title.");
                    _awaitingTaskDesc = false;   
                    _awaitingReminderDirectly = true;
                    _awaitingTaskTitle = true;
                }
                else
                {
                    _pendingTaskTitle = title;
                    _awaitingTaskDesc = true;
                    AddBotBubble(
                        $"Task: \"{title}\"\n\n" +
                        "Please give me a short description, or type \"skip\" to use the default.");
                }
                ActivityLog.Instance.Add($"NLP: task-add intent detected — \"{raw}\"");
                handled = true;
            }

            if (!handled)
            {
                //Follow-up check 
                if (IsFollowUp(input)) { HandleFollowUp(); return; }

                //Sentiment 
                SentimentResult mood = _detector.Detect(raw);
                RefreshMoodLabel(mood);
                string prefix = SentimentPrefix(mood);

                
                string reply = _engine.GetResponse(raw, _memory.Name, _memory);
                ActivityLog.Instance.Add($"Response given for: \"{TruncateLog(raw)}\"");

                string? topic = _engine.LastDetectedTopic;
                if (topic != null)
                {
                    _lastTopic = topic;
                    _memory.SetFavouriteTopic(topic);
                    RefreshUserCard();
                }

                AddBotBubble(string.IsNullOrEmpty(prefix) ? reply : prefix + "\n\n" + reply);
            }
        }

        private bool _awaitingTaskTitle = false;
        private bool _awaitingReminderDirectly = false;

        
        //  NLP HELPERS
        

        /// <summary>Returns true if <paramref name="input"/> contains ANY of the keywords.</summary>
        private static bool NlpMatch(string input, params string[] keywords) =>
            keywords.Any(k => input.Contains(k, StringComparison.OrdinalIgnoreCase));

       
        private static string ExtractTaskTitle(string raw)
        {
            string s = raw;

            // Strip leading command phrases
            string[] prefixes =
            {
                "add task to", "add a task to", "create task to", "create a task to",
                "new task to", "add task", "create task", "new task",
                "remind me to", "remind me about", "set a reminder for",
                "i need to", "don't let me forget to", "don't let me forget",
                "add a reminder to", "add reminder to"
            };

            foreach (string p in prefixes)
            {
                if (s.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    s = s[p.Length..].Trim(' ', '-', ':');
                    break;
                }
            }

            // Capitalise first letter
            if (s.Length == 0) return string.Empty;
            return char.ToUpper(s[0]) + s[1..];
        }

        
        private static DateTime? ParseReminderDate(string raw)
        {
            string s = raw.Trim().ToLower();

            if (s == "tomorrow") return DateTime.Today.AddDays(1);
            if (s == "next week") return DateTime.Today.AddDays(7);
            if (s == "next month") return DateTime.Today.AddMonths(1);

            
            var daysMatch = System.Text.RegularExpressions.Regex.Match(s,
                @"(?:in\s+)?(\d+)\s*(?:days?)?");
            if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out int d))
                return DateTime.Today.AddDays(d);

            
            if (DateTime.TryParse(raw, out DateTime dt))
                return dt;

            return null;
        }

        private static string TruncateLog(string s, int max = 50) =>
            s.Length > max ? s[..max] + "…" : s;

        

        private static bool IsFollowUp(string s) =>
            s.Contains("tell me more") || s.Contains("explain more") ||
            s.Contains("more info") || s.Contains("another tip") ||
            s.Contains("go on") || s.Contains("continue") ||
            s.Contains("expand") || s.Contains("keep going");

        private void HandleFollowUp()
        {
            if (string.IsNullOrEmpty(_lastTopic))
            {
                AddBotBubble(
                    $"Not sure which topic to expand on, {_memory.Name}.\n" +
                    "Click a topic on the left or ask about something specific.");
                return;
            }
            AddBotBubble(
                $"More on {_lastTopic}:\n\n" +
                _engine.GetFollowUp(_lastTopic, _memory.Name));
        }

        

        private string SentimentPrefix(SentimentResult s) => s.Type switch
        {
            SentimentType.Worried =>
                $"It is completely understandable to feel worried, {_memory.Name}. " +
                "You are taking the right step by learning about this.",
            SentimentType.Frustrated =>
                $"I hear you, {_memory.Name} — cybersecurity can feel overwhelming. " +
                "Let us break this down together.",
            SentimentType.Curious =>
                $"Love the curiosity, {_memory.Name}! Here is what you need to know.",
            SentimentType.Happy => $"Great energy, {_memory.Name}!",
            SentimentType.Confused =>
                $"No problem at all, {_memory.Name}. " +
                "Let me explain this as clearly as possible.",
            _ => string.Empty
        };

        private void RefreshMoodLabel(SentimentResult s)
        {
            _moodLabel.Text = s.Type == SentimentType.Neutral
                ? string.Empty
                : $"Mood: {s.Label}";
        }

       
        private void RefreshUserCard()
        {
            _userCard.Text =
                $"{_memory.Name}\n" +
                $"Interest: {_memory.FavouriteTopic ?? "None yet"}\n" +
                $"Messages: {_memory.MessageCount}";
        }

        //  CHAT BUBBLE RENDERERS
       
        private void AddUserBubble(string text)
        {
            string time = DateTime.Now.ToString("HH:mm");

            var container = new Panel
            {
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 4),
                Width = _messages.ClientSize.Width - 10
            };

            var timeLabel = new Label
            {
                Text = $"{_memory.Name}  {time}",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 170, 255),
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            var msgLabel = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = Color.White,
                BackColor = C_UserBubble,
                AutoSize = true,
                MaximumSize = new Size(520, 0),
                Padding = new Padding(14, 10, 14, 10),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Margin = new Padding(0, 2, 0, 0)
            };

            container.Controls.Add(msgLabel);
            container.Controls.Add(timeLabel);

            int w = container.Width;
            container.PerformLayout();
            int h = timeLabel.Height + msgLabel.Height + 6;
            container.Height = h;
            timeLabel.Location = new Point(w - timeLabel.Width - 2, 0);
            msgLabel.Location = new Point(w - msgLabel.Width - 2, timeLabel.Height + 3);

            _messages.Controls.Add(container);
            ScrollToBottom();
        }

        private void AddBotBubble(string text)
        {
            string time = DateTime.Now.ToString("HH:mm");

            var container = new Panel
            {
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 4),
                Width = _messages.ClientSize.Width - 10
            };

            var timeLabel = new Label
            {
                Text = $"CyberBot  {time}",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = C_Mint,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(6, 0)
            };

            var accent = new Panel
            {
                BackColor = C_Purple,
                Width = 3,
                Location = new Point(0, 0)
            };

            var msgLabel = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = C_TextHi,
                BackColor = C_BotBubble,
                AutoSize = true,
                MaximumSize = new Size(580, 0),
                Padding = new Padding(14, 10, 14, 10),
                Location = new Point(6, 0),
                Margin = new Padding(0, 2, 0, 0)
            };

            container.Controls.Add(msgLabel);
            container.Controls.Add(accent);
            container.Controls.Add(timeLabel);

            container.PerformLayout();
            int msgTop = timeLabel.Height + 3;
            int totalH = msgTop + msgLabel.Height + 4;
            container.Height = totalH;
            timeLabel.Location = new Point(6, 0);
            accent.Location = new Point(0, msgTop);
            accent.Height = msgLabel.Height;
            msgLabel.Location = new Point(6, msgTop);

            _messages.Controls.Add(container);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            _chatScroll.ScrollControlIntoView(
                _messages.Controls.Count > 0
                    ? _messages.Controls[_messages.Controls.Count - 1]
                    : _messages);
        }
    }
}