using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbotGUI1
{
    public class UserMemory
    {
        public string UserName { get; set; } = "";
        public string FavoriteTopic { get; set; } = "";
        public DateTime LastActive { get; set; } = DateTime.Now;

        public bool HasFavoriteTopic()
        {
            return !string.IsNullOrEmpty(FavoriteTopic);
        }

        public void RememberTopic(string topic)
        {
            FavoriteTopic = topic;
            LastActive = DateTime.Now;
        }

        public string GetPersonalizedResponse()
        {
            if (string.IsNullOrEmpty(FavoriteTopic))
                return "How can I help you with cybersecurity today?";

            return FavoriteTopic switch
            {
                "privacy" => $"Since you care about privacy, {UserName}, remember to review app permissions regularly.",
                "phishing" => $"As someone interested in phishing, {UserName}, always verify sender email addresses.",
                "password" => $"For password security, {UserName}, consider using a password manager.",
                _ => $"I remember you're interested in {FavoriteTopic}. Would you like more tips?"
            };
        }
    }
}


