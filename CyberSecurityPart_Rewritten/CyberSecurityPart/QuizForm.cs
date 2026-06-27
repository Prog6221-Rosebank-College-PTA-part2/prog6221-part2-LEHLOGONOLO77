using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CyberSecurityAwarenessBot.Forms
{
   
    /// Modal quiz window. Presents one question at a time from QuizEngine.
   
    public class QuizForm : Form
    {
       
        private static readonly Color C_Bg = Color.FromArgb(14, 11, 28);
        private static readonly Color C_Card = Color.FromArgb(26, 21, 50);
        private static readonly Color C_Purple = Color.FromArgb(140, 80, 255);
        private static readonly Color C_Violet = Color.FromArgb(185, 130, 255);
        private static readonly Color C_Mint = Color.FromArgb(48, 220, 175);
        private static readonly Color C_TextHi = Color.FromArgb(242, 238, 255);
        private static readonly Color C_TextMid = Color.FromArgb(160, 150, 200);
        private static readonly Color C_Border = Color.FromArgb(50, 42, 85);
        private static readonly Color C_Correct = Color.FromArgb(30, 160, 80);
        private static readonly Color C_Wrong = Color.FromArgb(200, 50, 60);
        private static readonly Color C_OptionNorm = Color.FromArgb(30, 24, 56);
        private static readonly Color C_OptionHover = Color.FromArgb(52, 44, 90);

       
        private readonly string _userName;
        private readonly List<QuizQuestion> _questions;
        private int _currentIndex = 0;
        private bool _answered = false;

        public int Score { get; private set; } = 0;
        public int QuestionsAnswered { get; private set; } = 0;

        private Label _progressLabel = null!;
        private Label _questionLabel = null!;
        private Panel _optionsPanel = null!;
        private Label _explanationLbl = null!;
        private Button _nextBtn = null!;
        private Label _scoreLabel = null!;

        public QuizForm(string userName)
        {
            _userName = userName;
            _questions = QuizEngine.GetRound(10);   

            InitForm();
            ShowQuestion();
        }

        private void InitForm()
        {
            Text = "CyberBot Quiz";
            Size = new Size(620, 520);
            MinimumSize = new Size(500, 440);
            BackColor = C_Bg;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.FromArgb(20, 16, 40)
            };

            var title = new Label
            {
                Text = "🎯  CyberBot Quiz",
                Dock = DockStyle.Fill,
                ForeColor = C_Purple,
                Font = new Font("Segoe UI Black", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            _progressLabel = new Label
            {
                Dock = DockStyle.Right,
                Width = 110,
                ForeColor = C_TextMid,
                Font = new Font("Segoe UI", 9f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            header.Controls.Add(title);
            header.Controls.Add(_progressLabel);
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = C_Border });

            
            _scoreLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = $"Score: 0 / {_questions.Count}",
                ForeColor = C_Mint,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(18, 14, 36)
            };

           
            var questionCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = C_Card,
                Padding = new Padding(18, 12, 18, 12)
            };

            _questionLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            questionCard.Controls.Add(_questionLabel);

           
            _optionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_Bg,
                Padding = new Padding(16, 8, 16, 4)
            };


            _explanationLbl = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                Text = string.Empty,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = C_Mint,
                BackColor = Color.FromArgb(18, 14, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(12, 4, 12, 4),
                Visible = false
            };

            
            _nextBtn = new Button
            {
                Text = "Next  ›",
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = C_Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            _nextBtn.FlatAppearance.BorderSize = 0;
            _nextBtn.FlatAppearance.MouseOverBackColor = C_Violet;
            _nextBtn.Click += NextBtn_Click;

            Controls.Add(_optionsPanel);
            Controls.Add(_explanationLbl);
            Controls.Add(_nextBtn);
            Controls.Add(questionCard);
            Controls.Add(_scoreLabel);
            Controls.Add(header);
        }

        

        private void ShowQuestion()
        {
            if (_currentIndex >= _questions.Count) { ShowResults(); return; }

            var q = _questions[_currentIndex];
            _answered = false;

            _progressLabel.Text = $"Q {_currentIndex + 1} / {_questions.Count}";
            _questionLabel.Text = q.Question;
            _explanationLbl.Text = string.Empty;
            _explanationLbl.Visible = false;
            _nextBtn.Visible = false;

            _optionsPanel.Controls.Clear();

            
            var validOptions = new List<(string text, int index)>();
            for (int i = 0; i < q.Options.Length; i++)
                if (!string.IsNullOrWhiteSpace(q.Options[i]))
                    validOptions.Add((q.Options[i], i));

            int btnH = 46;
            int spacing = 8;
            int startY = 10;

            for (int vi = 0; vi < validOptions.Count; vi++)
            {
                var (optText, optIndex) = validOptions[vi];

                var btn = new Button
                {
                    Text = $"  {(char)('A' + vi)}.  {optText}",
                    Location = new Point(0, startY + vi * (btnH + spacing)),
                    Size = new Size(_optionsPanel.ClientSize.Width - 2, btnH),
                    BackColor = C_OptionNorm,
                    ForeColor = C_TextHi,
                    Font = new Font("Segoe UI", 10f),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Tag = optIndex    
                };
                btn.FlatAppearance.BorderColor = C_Border;
                btn.FlatAppearance.BorderSize = 1;
                btn.MouseEnter += (s, e) => { if (!_answered) btn.BackColor = C_OptionHover; };
                btn.MouseLeave += (s, e) => { if (!_answered) btn.BackColor = C_OptionNorm; };

                btn.Click += OptionBtn_Click;
                _optionsPanel.Controls.Add(btn);
            }
        }

        private void OptionBtn_Click(object? sender, EventArgs e)
        {
            if (_answered || sender is not Button clicked) return;
            _answered = true;

            var q = _questions[_currentIndex];
            int chosen = (int)clicked.Tag!;
            bool isCorrect = chosen == q.CorrectIndex;

            QuestionsAnswered++;
            if (isCorrect) Score++;

            
            foreach (Control ctrl in _optionsPanel.Controls)
            {
                if (ctrl is not Button b) continue;
                int idx = (int)b.Tag!;
                b.Enabled = false;

                if (idx == q.CorrectIndex)
                    b.BackColor = C_Correct;
                else if (b == clicked && !isCorrect)
                    b.BackColor = C_Wrong;
            }

            
            _scoreLabel.Text = $"Score: {Score} / {_questions.Count}";

          
            _explanationLbl.Text = "💡 " + q.Explanation;
            _explanationLbl.Visible = true;

            
            bool isLast = _currentIndex == _questions.Count - 1;
            _nextBtn.Text = isLast ? "See Results  ✓" : "Next  ›";
            _nextBtn.Visible = true;
        }

        private void NextBtn_Click(object? sender, EventArgs e)
        {
            _currentIndex++;
            if (_currentIndex >= _questions.Count)
                ShowResults();
            else
                ShowQuestion();
        }

        

        private void ShowResults()
        {
            _optionsPanel.Controls.Clear();
            _explanationLbl.Visible = false;
            _nextBtn.Visible = false;
            _progressLabel.Text = "Done!";

            string emoji = Score == _questions.Count ? "🏆" : Score >= _questions.Count / 2 ? "👍" : "📚";
            string verdict = Score == _questions.Count
                ? "Perfect score! You are a cybersecurity pro!"
                : Score >= _questions.Count / 2
                    ? "Good job! Keep exploring to sharpen your skills."
                    : "Keep learning! Use the topic buttons to brush up.";

            var resultLbl = new Label
            {
                Text = $"{emoji}\n\n{_userName}, you scored\n\n{Score} / {_questions.Count}\n\n{verdict}",
                Dock = DockStyle.Fill,
                ForeColor = C_TextHi,
                Font = new Font("Segoe UI", 12f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            _optionsPanel.Controls.Add(resultLbl);

            var closeBtn = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = C_Purple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = C_Violet;
            closeBtn.Click += (s, e) => Close();

            Controls.Add(closeBtn);
            closeBtn.BringToFront();
        }
    }
}