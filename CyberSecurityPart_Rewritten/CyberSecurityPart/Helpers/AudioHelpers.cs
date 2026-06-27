namespace CyberSecurityAwarenessBot.Helpers
{
    // This is the audio that will welcome the user.  
    public class AudioHelper
    {
        private const string WavFile = "GReeting.wav";

        public void PlayVoiceGreeting()
        {
            try
            {
               
                string path = Path.Combine(AppContext.BaseDirectory, WavFile);
                if (!File.Exists(path)) return;
                if (OperatingSystem.IsWindows()) PlayWav(path);
            }
            catch { /* fail silently — a missing greeting should never crash the bot */ }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private static void PlayWav(string path)
        {
            using var p = new System.Media.SoundPlayer(path);
            p.PlaySync();
        }
    }
}