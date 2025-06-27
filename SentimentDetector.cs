using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbotGUI1
{
    public class SentimentDetector
    {
        private readonly Dictionary<string, string> _sentimentResponses = new()
        {
            {"angry", "I understand this is frustrating. Let's tackle this together."},
            {"worried", "It's normal to feel concerned about online security. I'll help."},
            {"happy", "Great to see you enthusiastic about cybersecurity!"},
            {"confused", "No worries, I'll explain this clearly."}
        };

        public string DetectSentiment(string input)
        {
            input = input.ToLower();

            if (input.Contains("angry") || input.Contains("furious"))
                return "angry";
            if (input.Contains("worried") || input.Contains("concerned"))
                return "worried";
            if (input.Contains("happy") || input.Contains("excited"))
                return "happy";
            if (input.Contains("confused") || input.Contains("unsure"))
                return "confused";

            return "neutral";
        }

        public string GetResponse(string sentiment)
            => _sentimentResponses.TryGetValue(sentiment, out var response)
                ? response
                : "How can I assist you with cybersecurity today?";
        
    }
}
    
