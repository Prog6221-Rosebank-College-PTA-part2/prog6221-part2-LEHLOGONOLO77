using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBot
{
    public sealed class ActivityLog
    {
        private static readonly ActivityLog _instance = new();
        public static ActivityLog Instance => _instance;

        private readonly List<LogEntry> _entries = new();

        private ActivityLog() { }

       // Public API 

        public void Add(string description)
        {
            _entries.Add(new LogEntry(DateTime.Now, description));
        }

        /// Returns the most recent <paramref name="count"/> entries, newest first.
        public IReadOnlyList<LogEntry> GetRecent(int count = 10) =>
            _entries.AsEnumerable().Reverse().Take(count).ToList().AsReadOnly();

        public IReadOnlyList<LogEntry> GetAll() =>
            _entries.AsReadOnly();

        public int TotalCount => _entries.Count;

       
        /// Formats the last <paramref name="count"/> entries as a numbered string
       
        public string FormatRecent(int count = 10)
        {
            var recent = GetRecent(count).ToList();
            if (recent.Count == 0)
                return "No actions recorded yet. Start chatting or try the quiz!";

            var lines = new System.Text.StringBuilder();
            lines.AppendLine($"Here are my last {recent.Count} action(s):\n");
            for (int i = 0; i < recent.Count; i++)
                lines.AppendLine($"{i + 1}. [{recent[i].Timestamp:HH:mm}]  {recent[i].Description}");

            return lines.ToString().TrimEnd();
        }
    }

    public record LogEntry(DateTime Timestamp, string Description);
}
