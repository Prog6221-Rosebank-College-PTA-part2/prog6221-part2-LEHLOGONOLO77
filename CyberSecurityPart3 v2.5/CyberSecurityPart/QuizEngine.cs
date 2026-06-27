using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBot.Forms
{
    public class QuizQuestion
    {
        public string Question { get; init; } = string.Empty;
        public string[] Options { get; init; } = new string[4];
        public int CorrectIndex { get; init; }
        public string Explanation { get; init; } = string.Empty;
    }
    //This brain for the quiz.
    // It contains a static list of questions and a method to get a random round of questions.
    public static class QuizEngine
    {
        private static readonly Random _rng = new();

        private static readonly List<QuizQuestion> _bank = new()
        {
            new QuizQuestion
            {
                Question     = "What is the minimum recommended password length?",
                Options      = new[] { "6 characters", "8 characters", "12 characters", "20 characters" },
                CorrectIndex = 2,
                Explanation  = "Security experts recommend at least 12 characters. Longer is always better!"
            },
            new QuizQuestion
            {
                Question     = "What does 'phishing' mean?",
                Options      = new[] { "A type of malware", "Tricking users into revealing personal info", "Encrypting files for ransom", "Scanning a network for open ports" },
                CorrectIndex = 1,
                Explanation  = "Phishing uses fake emails or websites to trick you into handing over credentials."
            },
            new QuizQuestion
            {
                Question     = "Which 2FA method is the MOST secure?",
                Options      = new[] { "SMS code", "Email code", "Authenticator app", "Hardware key (e.g. YubiKey)" },
                CorrectIndex = 3,
                Explanation  = "Hardware keys are phishing-resistant and cannot be intercepted remotely."
            },
            new QuizQuestion
            {
                Question     = "What does HTTPS indicate about a website?",
                Options      = new[] { "The site is 100% safe", "The connection is encrypted", "The site is government-approved", "The site has no malware" },
                CorrectIndex = 1,
                Explanation  = "HTTPS means the connection is encrypted — but the site itself could still be malicious."
            },
            new QuizQuestion
            {
                Question     = "What is ransomware?",
                Options      = new[] { "Software that steals passwords", "A firewall bypass tool", "Malware that locks your files and demands payment", "A social engineering technique" },
                CorrectIndex = 2,
                Explanation  = "Ransomware encrypts your files and demands a ransom. Regular backups are your best defence."
            },
            new QuizQuestion
            {
                Question     = "Which is the safest action on public Wi-Fi?",
                Options      = new[] { "Log into your bank account", "Use a reputable VPN", "Leave auto-connect enabled", "Use HTTP sites only" },
                CorrectIndex = 1,
                Explanation  = "A VPN encrypts all your traffic, making it unreadable even if intercepted."
            },
            new QuizQuestion
            {
                Question     = "What is social engineering?",
                Options      = new[] { "Hacking through software bugs", "Manipulating people to reveal confidential info", "Encrypting data in transit", "Setting up fake Wi-Fi hotspots" },
                CorrectIndex = 1,
                Explanation  = "Social engineering exploits human psychology rather than technical vulnerabilities."
            },
            new QuizQuestion
            {
                Question     = "What does a password manager do?",
                Options      = new[] { "Resets your passwords automatically", "Remembers and generates strong unique passwords", "Blocks brute-force attacks", "Stores passwords in a text file" },
                CorrectIndex = 1,
                Explanation  = "A password manager generates and stores unique passwords so you only need one master password."
            },
            new QuizQuestion
            {
                Question     = "Which site lets you check if your email was in a data breach?",
                Options      = new[] { "darkwebscan.com", "haveibeenpwned.com", "breachcheck.net", "emailsecurity.org" },
                CorrectIndex = 1,
                Explanation  = "haveibeenpwned.com by Troy Hunt is the most trusted breach-checking service."
            },
            new QuizQuestion
            {
                Question     = "What encryption standard do most governments use?",
                Options      = new[] { "DES-128", "MD5", "AES-256", "SHA-1" },
                CorrectIndex = 2,
                Explanation  = "AES-256 is the gold standard for symmetric encryption used globally."
            },
           
            new QuizQuestion
            {
                Question     = "True or False: HTTPS guarantees a website is completely trustworthy.",
                Options      = new[] { "True", "False", string.Empty, string.Empty },
                CorrectIndex = 1,
                Explanation  = "HTTPS only encrypts the connection. A phishing site can still use HTTPS."
            },
            new QuizQuestion
            {
                Question     = "Which of the following is a sign of a phishing email?",
                Options      = new[] { "Sent from your bank's official domain", "Uses your full name correctly", "Urgent language and a suspicious link", "Has no attachments" },
                CorrectIndex = 2,
                Explanation  = "Urgency and suspicious links are classic phishing red flags."
            },
            new QuizQuestion
            {
                Question     = "What is a VPN primarily used for?",
                Options      = new[] { "Speeding up your internet", "Encrypting your internet traffic and hiding your IP", "Blocking ads on websites", "Scanning for viruses" },
                CorrectIndex = 1,
                Explanation  = "A VPN tunnels your data through an encrypted server, masking your IP and activity."
            },
            new QuizQuestion
            {
                Question     = "What is 'smishing'?",
                Options      = new[] { "Phishing carried out via SMS text messages", "A type of firewall", "A malware that spreads over Bluetooth", "An encryption algorithm" },
                CorrectIndex = 0,
                Explanation  = "Smishing is phishing delivered through text messages — never click surprise links in SMS."
            },
            new QuizQuestion
            {
                Question     = "Which backup strategy is recommended to defend against ransomware?",
                Options      = new[] { "1 copy on the same PC", "The 3-2-1 rule: 3 copies, 2 media types, 1 offsite", "No backups, just strong passwords", "Cloud sync only, deleted after a week" },
                CorrectIndex = 1,
                Explanation  = "The 3-2-1 rule ensures at least one backup survives even if your main device is compromised."
            },
            new QuizQuestion
            {
                Question     = "What should you do if you receive an unexpected request for your password, even from 'IT support'?",
                Options      = new[] { "Send it immediately to be helpful", "Never share it — verify the request through an official channel first", "Share only the first half", "Change it and send the new one" },
                CorrectIndex = 1,
                Explanation  = "Legitimate IT staff never need your actual password. Always verify suspicious requests independently."
            },
            new QuizQuestion
            {
                Question     = "What is the main risk of reusing the same password across multiple sites?",
                Options      = new[] { "Slower login times", "One breached site can expose all your other accounts", "It uses more storage", "Nothing, as long as it is long" },
                CorrectIndex = 1,
                Explanation  = "Credential stuffing attacks reuse leaked passwords across many sites — unique passwords stop the spread."
            },
        };

       
        public static List<QuizQuestion> GetRound(int count = 5)
        {
            var shuffled = _bank.OrderBy(_ => _rng.Next()).ToList();
            return count <= 0 ? shuffled : shuffled.Take(count).ToList();
        }
    }
}
