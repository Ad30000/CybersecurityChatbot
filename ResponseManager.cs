using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CybersecurityChatbotGUI1
{
    public class ResponseManager
    {
        private readonly Dictionary<string, string> _directResponses = new(StringComparer.OrdinalIgnoreCase)
        {
            {"hello", "Welcome back! How can I help with cybersecurity today?"},
            {"what can you do", "I can:\n- Teach cybersecurity basics\n- Manage your security tasks\n- Test your knowledge with quizzes"},
            {"thank you", "You're welcome! Stay safe online."}
        };

        private readonly Dictionary<string, string> _keywordResponses = new(StringComparer.OrdinalIgnoreCase)
        {
            {"password", "Strong passwords should:\n- Be 12+ characters\n- Include numbers & symbols\n- Be unique for each account"},
            {"phishing", "Watch for:\n➔ Urgent-sounding messages\n➔ Suspicious links\n➔ Requests for personal info"},
            {"privacy", "Privacy tips:\n• Review app permissions\n• Use private browsing\n• Enable 2FA"},

            // New added keywords
            {"social engineering", "Social engineering attacks:\n⚠️ Never share credentials over phone\n⚠️ Verify caller identities\n⚠️ Beware of 'urgent' requests"},
            {"wifi", "Public WiFi safety:\n🔒 Use VPN services\n🔒 Avoid accessing sensitive accounts\n🔒 Verify network names with staff"}
        };

        // NLP-style pattern matching
        public (string response, string command) ProcessNlpInput(string input)
        {
            input = input.ToLower().Trim();

            if (Regex.IsMatch(input, @"(add|create|make).*task"))
            {
                var match = Regex.Match(input, @"(add|create|make).*task.*?(?:about|for|to)?(.*)");
                string title = match.Groups[2].Value.Trim();
                return (string.IsNullOrEmpty(title)
                    ? "I'll help create a task. What should we title it?"
                    : $"I'll create a task for '{title}'. Please enter the description.",
                    $"addtask {title}");
            }

            // Task completion patterns
            if (Regex.IsMatch(input, @"(remind me|set reminder).*?(?:about|for|to)?(.*)"))
            {
                return ("I can set a reminder. When should I remind you?", $"setreminder {input}");
            }

            // Quiz patterns
            if (Regex.IsMatch(input, @"(start|begin|take).*quiz"))
            {
                return ("Starting the cybersecurity quiz...", "startquiz");
            }

            // Fallback to existing keyword system
            return (null, null);
        }

        public (string response, bool isKeyword) GetResponse(string input)
        {
            // 1. Check direct responses first
            if (_directResponses.TryGetValue(input.ToLower(), out var directResponse))
                return (directResponse, true);

            // 2. Check for security keywords
            foreach (var keyword in _keywordResponses.Keys)
            {
                if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return (_keywordResponses[keyword], true);
                }
            }

            // 3. Return empty for unrecognized input
            return ("", false);
        }
    }
}

