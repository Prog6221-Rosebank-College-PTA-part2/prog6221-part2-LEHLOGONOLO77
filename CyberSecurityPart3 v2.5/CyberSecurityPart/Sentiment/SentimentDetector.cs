namespace CyberSecurityAwarenessBot.Sentiment
{
    public enum SentimentType { Neutral, Worried, Frustrated, Curious, Happy, Confused }

    public class SentimentResult
    {
        public SentimentType Type { get; init; } = SentimentType.Neutral;
        public string Label { get; init; } = "Neutral";
    }

   
    /// Detects the user's emotional tone from keyword patterns
    /// This helps the bot to respond with empathy and the right tone.
   
    public class SentimentDetector
    {
        private static readonly string[] Worried = { "worried", "scared", "afraid", "nervous"};
        private static readonly string[] Frustrated = { "frustrated", "annoyed", "angry" };
        private static readonly string[] Curious = { "curious", "interested" };
        private static readonly string[] Happy = { "great", "awesome", "amazing" };
        private static readonly string[] Confused = { "confused", "don't understand"};

        public SentimentResult Detect(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new();
            string l = input.ToLower();
            if (Hit(l, Worried)) return new() { Type = SentimentType.Worried, Label = "Worried" };
            if (Hit(l, Frustrated)) return new() { Type = SentimentType.Frustrated, Label = "Frustrated" };
            if (Hit(l, Confused)) return new() { Type = SentimentType.Confused, Label = "Confused" };
            if (Hit(l, Curious)) return new() { Type = SentimentType.Curious, Label = "Curious" };
            if (Hit(l, Happy)) return new() { Type = SentimentType.Happy, Label = "Happy" };
            return new();
        }

        private static bool Hit(string input, string[] words) => words.Any(input.Contains);
    }
}
