using CyberSecurityAwarenessBot.Memory;

namespace CyberSecurityAwarenessBot.Responses
{
   
    public class ResponseEngine
    {
        private readonly Random _rng = new();

        
        public string? LastDetectedTopic { get; private set; }

        //Topic model
        private class Topic
        {
            public string Name { get; init; } = string.Empty;
            public string[] Keywords { get; init; } = Array.Empty<string>();
            public string[] Responses { get; init; } = Array.Empty<string>();
            public string FollowUp { get; init; } = string.Empty;
        }

        private readonly List<Topic> _topics;
        public ResponseEngine() => _topics = Build();

        //Public API

        public string GetResponse(string input, string name, UserMemory memory)
        {
            memory.IncrementMessageCount();
            if (string.IsNullOrWhiteSpace(input))
                return "I didn't quite understand that. Could you rephrase?";

            string lower = input.ToLower();
            foreach (var t in _topics)
            {
                if (t.Keywords.Any(k => lower.Contains(k)))
                {
                    LastDetectedTopic = t.Name;
                    string r = t.Responses[_rng.Next(t.Responses.Length)].Replace("{n}", name);
                    string recall = memory.GetMemoryRecall();
                    if (!string.IsNullOrEmpty(recall) && memory.FavouriteTopic != t.Name)
                        r += $"\n\n  Tip: {recall}";
                    return r;
                }
            }
            LastDetectedTopic = null;
            return $"I didn't quite understand that, {name}. " +
                   "Try clicking a topic on the left, or ask about passwords, phishing, malware, 2FA, and more.";
        }

        public string GetFollowUp(string topicName, string name)
        {
            var t = _topics.FirstOrDefault(x => x.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));
            return t == null
                ? $"No extra detail available for '{topicName}' right now, {name}."
                : t.FollowUp.Replace("{n}", name);
        }

        //Topics

        private static List<Topic> Build() => new()
        {
            new Topic
            {
                Name = "greeting",
                Keywords = new[] { "hello","hi","hey","good morning","good afternoon" },
                Responses = new[]
                {
                    "Hi {n}! What cybersecurity topic can I help you with today?",
                    "Hey {n}! Ready to level up your online safety? What would you like to know?",
                    "Hello {n}! Use the topics on the left or just ask me anything about staying safe online."
                },
                FollowUp = "Just pick a topic or type your question, {n}!"
            },
            new Topic
            {
                Name = "how are you",
                Keywords = new[] { "how are you","how do you do" },
                Responses = new[]
                {
                    "Doing great, {n}! Ready to help you stay safe online. What would you like to know?",
                    "All good here, {n}! Cybersecurity never sleeps — what can I help you with?",
                    "Running smoothly! How can I help you today, {n}?"
                },
                FollowUp = "I am always here to help, {n}!"
            },
            new Topic
            {
                Name = "purpose",
                Keywords = new[] { "purpose","what are you","what do you do","what can you do","help","what can i ask","topics" },
                Responses = new[]
                {
                    "I am your cybersecurity awareness guide, {n}!\n\n" +
                    "You can ask me about:\n  Passwords  •  Phishing  •  Malware  •  Ransomware\n" +
                    "  2FA  •  Encryption  •  VPNs  •  Safe Browsing\n" +
                    "  Data Breaches  •  Privacy  •  Social Engineering\n\n" +
                    "Or just click a topic button on the left!",
                    "My goal is to help you practice safer online habits, {n}. " +
                    "Click any topic on the left or type your question below!"
                },
                FollowUp = "Feel free to ask about any cybersecurity topic, {n}!"
            },
            new Topic
            {
                Name = "passwords",
                Keywords = new[] { "password","passcode","credentials","passphrase" },
                Responses = new[]
                {
                    "Password safety for {n}:\n\n" +
                    "  • Use at least 12 characters per password.\n" +
                    "  • Mix uppercase, lowercase, numbers and symbols.\n" +
                    "  • Never reuse passwords across sites.\n" +
                    "  • Consider a password manager like Bitwarden.\n" +
                    "  • Enable 2FA wherever possible.",

                    "Strong passwords are your first line of defence, {n}!\n\n" +
                    "  • A passphrase like 'PurpleCat$Runs9' is strong AND memorable.\n" +
                    "  • Avoid personal details like birthdays or pet names.\n" +
                    "  • Change passwords immediately if you suspect a breach.\n" +
                    "  • Never share your password — not even with support staff.",

                    "'Password123' is one of the most hacked passwords, {n}!\n\n" +
                    "  • The longer the password the harder it is to crack.\n" +
                    "  • Use a different password for every account.\n" +
                    "  • A password manager generates and stores them for you.\n" ,
                   
                },
                
            },
            new Topic
            {
                Name = "password manager",
                Keywords = new[] { "manager","bitwarden","lastpass","1password","keepass" },
                Responses = new[]
                {
                    "Password managers are a game changer, {n}!\n\n" +
                    "  • They generate and store strong unique passwords for every site.\n" +
                    "  • You only need to remember ONE master password.\n" +
                    "  • Bitwarden is free and open-source — highly recommended.\n" +
                    "  • Always protect the manager itself with 2FA.",

                    "Using a password manager is one of the best things you can do, {n}!\n\n" +
                    "  • Top options: Bitwarden (free), 1Password, KeePass.\n" +
                    "  • They autofill passwords — no more mistyping.\n" +
                    "  • They flag duplicate or weak passwords automatically.\n" +
                    "  • Your vault is encrypted — even the company cannot see it."
                },
                
            },
            new Topic
            {
                Name = "phishing",
                Keywords = new[] { "phishing","phish","scam","fake email","suspicious email","spam","smishing" },
                Responses = new[]
                {
                    "Phishing watch-list for {n}:\n\n" +
                    "  • Urgent language — 'Act NOW or your account is closed!'\n" +
                    "  • Sender email does not match the real organisation.\n" +
                    "  • Links that do not go where they claim (hover first!).\n" +
                    "  • Unexpected attachments — especially .exe or .zip files.\n" +
                    "  When in doubt: do not click. Verify with the company directly.",

                    "Phishing tricks you into giving up info, {n}!\n\n" +
                    "  • Legitimate banks NEVER ask for your PIN or password by email.\n" +
                    "  • Check for subtle misspellings: 'Micros0ft', 'Paypa1'.\n" +
                    "  • SMS phishing is called 'smishing' — never click surprise text links.\n" +
                    "  • Phone call phishing is 'vishing' — hang up and call back officially.",

                    "Spear phishing targets YOU specifically, {n}!\n\n" +
                    "  • Attackers use your real name, employer, or recent activity to seem legit.\n" +
                    "  • If an email creates panic or unusual urgency — that is a red flag.\n" +
                    "  • Use email filtering and report phishing to your provider.\n" +
                    "  • Enable 2FA so stolen credentials alone cannot log in."
                },
                
            },
            new Topic
            {
                Name = "malware",
                Keywords = new[] { "malware","virus","antivirus" },
                Responses = new[]
                {
                    "Malware defence for {n}:\n\n" +
                    "  • Viruses spread by attaching to files; worms spread over networks.\n" +
                    "  • Trojans disguise themselves as legitimate software.\n" +
                    "  • Spyware silently monitors your activity and steals data.\n" +
                    "  • Keep antivirus updated and never open unexpected attachments.",

                    "Protecting against malware, {n}:\n\n" +
                    "  • Windows Defender is a decent free built-in option.\n" +
                    "  • Scan USB drives before opening any files from them.\n" +
                    "  • Keep your OS updated — patches close the doors malware exploits.\n" +
                    "  • Avoid pirated software — commonly bundled with malware."
                },
              
            },
            new Topic
            {
                Name = "ransomware",
                Keywords = new[] { "ransomware","ransom","files locked","encrypted files","pay hackers" },
                Responses = new[]
                {
                    "Ransomware locks your files and demands payment, {n}!\n\n" +
                    "  • Do NOT pay — it does not guarantee your files return.\n" +
                    "  • Disconnect the infected machine from the network immediately.\n" +
                    "  • Prevention is everything: keep backups and software updated.\n" +
                    "  • Report the attack to your country's cybercrime unit.",

                    "Ransomware protection tips, {n}:\n\n" +
                    "  • Offline backups are your best defence — ransomware cannot reach them.\n" +
                    "  • Never open attachments from unknown senders.\n" +
                    "  • Disable macros in Office documents unless you specifically need them.\n" +
                    "  • Follow the 3-2-1 backup rule: 3 copies, 2 media types, 1 offsite."
                },
               
            },
            new Topic
            {
                Name = "2FA",
                Keywords = new[] {"two factor","two-factor","mfa","multi factor","authenticator" },
                Responses = new[]
                {
                    "2FA doubles your account security, {n}!\n\n" +
                    "  • Even with your password stolen, attackers need your 2FA code too.\n" +
                    "  • Use an authenticator app (Google Authenticator, Authy) over SMS.\n" +
                    "  • SMS codes can be hijacked via SIM-swapping attacks.\n" +
                    "  • Hardware keys like YubiKey offer the strongest protection.",

                    "Enable 2FA everywhere, {n}!\n\n" +
                    "  • Start with email, banking and social media.\n" +
                    "  • Store backup codes safely offline when you set up 2FA.\n" +
                    "  • Most major sites now support 2FA — check Settings > Security.\n" +
                    "  • Even basic SMS 2FA is far better than no 2FA at all."
                },
               
            },
            new Topic
            {
                Name = "VPN",
                Keywords = new[] { "vpn","public wifi","public wi-fi","free wifi"},
                Responses = new[]
                {
                    "Public Wi-Fi safety for {n}:\n\n" +
                    "  • Anyone on the same network can intercept unencrypted traffic.\n" +
                    "  • Use a reputable VPN (ProtonVPN, Mullvad) to encrypt everything.\n" +
                    "  • Avoid banking or sensitive logins on public Wi-Fi.\n" +
                    "  • Turn off auto-connect to open networks on your phone.",

                    "VPN tips for {n}:\n\n" +
                    "  • Free VPNs often sell your data — avoid them entirely.\n" +
                    "  • Evil twin attacks create fake hotspots with convincing names.\n" +
                    "  • Your personal phone hotspot is safer than coffee shop Wi-Fi.\n" +
                    "  • A VPN also hides your IP address from websites you visit."
                },
                
            },
            new Topic
            {
                Name = "encryption",
                Keywords = new[] { "encryption","encrypt","encrypted" },
                Responses = new[]
                {
                    "Encryption keeps your data unreadable to attackers, {n}!\n\n" +
                    "  • HTTPS encrypts data between you and a website — always check for it.\n" +
                    "  • End-to-end encryption means only you and the recipient can read messages.\n" +
                    "  • Enable full-disk encryption: BitLocker (Windows) or FileVault (Mac).\n" +
                    "  • Encrypted stolen data is useless without the decryption key.",

                    "Encryption essentials for {n}:\n\n" +
                    "  • AES-256 is the gold standard — used by governments worldwide.\n" +
                    "  • Signal and WhatsApp use E2EE for messages.\n" +
                    "  • Your phone's storage is encrypted by default on modern devices.\n" +
                    "  • VPNs encrypt your internet traffic on public networks."
                },

            },
            new Topic
            {
                Name = "safe browsing",
                Keywords = new[] { "browsing","safe browsing","https","website","browser","secure site","url","link" },
                Responses = new[]
                {
                    "Safe browsing habits for {n}:\n\n" +
                    "  • Always check for HTTPS (padlock icon) before entering sensitive data.\n" +
                    "  • Keep your browser updated — updates patch security vulnerabilities.\n" +
                    "  • Use uBlock Origin to block malicious ads and trackers.\n" +
                    "  • Be wary of pop-ups claiming your device is infected.",

                    "Browse smarter, {n}!\n\n" +
                    "  • Brave and Firefox offer better privacy than Chrome by default.\n" +
                    "  • Use DuckDuckGo to avoid Google tracking your searches.\n" +
                    "  • Only download software from official websites or trusted stores.\n" +
                    "  • Verify URLs carefully — watch for lookalike domains like 'Paypa1.com'."
                },
                
            },
            new Topic
            {
                Name = "data breach",
                Keywords = new[] { "breach","data breach","leaked","dark web","darkweb" },
                Responses = new[]
                {
                    "Data breach action plan for {n}:\n\n" +
                    "  • Visit haveibeenpwned.com to check if your email appeared in a breach.\n" +
                    "  • If compromised, change that password on every site you used it.\n" +
                    "  • Enable breach alerts to be notified of future incidents.\n" +
                    "  • Unique passwords per site stop one breach cascading to others.",

                    "Breach awareness for {n}:\n\n" +
                    "  • Breached data is sold on the dark web — often within hours.\n" +
                    "  • Common data exposed: emails, passwords, phone numbers, addresses.\n" +
                    "  • Weak hashed passwords can still be cracked — use strong unique ones.\n" +
                    "  • Enable 2FA so a leaked password alone cannot compromise your account."
                },
               
            },
            new Topic
            {
                Name = "privacy",
                Keywords = new[] { "privacy","personal data","data privacy","tracking","surveillance","cookies" },
                Responses = new[]
                {
                    "Online privacy for {n}:\n\n" +
                    "  • Review privacy settings on all your social media accounts regularly.\n" +
                    "  • Limit what apps can access — location, contacts, microphone, camera.\n" +
                    "  • Use a privacy-focused browser like Firefox or Brave.\n" +
                    "  • DuckDuckGo does not track your searches.",

                    "Privacy tips, {n}:\n\n" +
                    "  • Advertisers build profiles from your browsing and app usage.\n" +
                    "  • Read app permissions before installing anything.\n" +
                    "  • Third-party cookies track you across websites — clear them regularly.\n" +
                    "  • Consider ProtonMail for encrypted private email."
                },
                
            },
            new Topic
            {
                Name = "social engineering",
                Keywords = new[] { "social engineering","manipulation","impersonation" },
                Responses = new[]
                {
                    "Social engineering exploits people, not systems, {n}!\n\n" +
                    "  • Pretexting: attacker creates a fake scenario to earn your trust.\n" +
                    "  • Baiting: leaving infected USB drives where victims find them.\n" +
                    "  • Tailgating: following someone into a secure building.\n" +
                    "  • Always verify identities before sharing ANY sensitive information.",

                    "Humans are the weakest link — and attackers know it, {n}!\n\n" +
                    "  • Be sceptical of unexpected urgent requests, even from apparent colleagues.\n" +
                    "  • If IT calls asking for your password, call them back on the official number.\n" +
                    "  • Authority and urgency are the top social engineering pressure tactics.\n" +
                    "  • Security awareness training dramatically reduces attack success rates."
                },
                
            },
            new Topic
            {
                Name = "goodbye",
                Keywords = new[] { "thank","thanks","bye","goodbye" },
                Responses = new[]
                {
                    "You are very welcome, {n}! Stay safe out there.",
                    "Happy to help, {n}! Good habits are the best security tool. Take care!",
                    "Goodbye, {n}! You are now better equipped to stay safe online."
                },
                FollowUp = "Come back anytime, {n}!"
            }
        };
    }
}
