using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbotGUI
{
    public class QuizManager
    {
        private List<QuizQuestion> _questions;
        public IReadOnlyList<QuizQuestion> Questions => _questions.AsReadOnly();

        public int CurrentQuestionIndex => _currentQuestionIndex;

        private int _currentQuestionIndex = -1;

        public QuizQuestion CurrentQuestion { get; private set; }
        public int Score { get; private set; }

        public bool QuizComplete => _currentQuestionIndex >= _questions.Count - 1;
        public bool IsLastQuestion => _currentQuestionIndex == _questions.Count - 1;
        public int QuestionsRemaining => _questions.Count - (_currentQuestionIndex + 1);

        public QuizManager()
        {
            InitializeQuestions();
        }

        private void InitializeQuestions()
        {
            _questions = new List<QuizQuestion>
    {
        // Existing questions...
        new QuizQuestion(
            "What should you do if you receive an email asking for your password?",
            new List<string> {
                "Reply with your password",
                "Delete the email",
                "Report the email as phishing",
                "Ignore it"
            },
            2,
            "Legitimate organizations will never ask for your password via email. Always report such requests as phishing."),

        new QuizQuestion(
            "Which of these is the strongest password?",
            new List<string> {
                "password123",
                "JohnDoe1980",
                "P@ssw0rd!",
                "CorrectHorseBatteryStaple"
            },
            3,
            "Long passphrases are more secure than complex but short passwords. A 4-word random phrase is very strong."),

        // New questions:
        new QuizQuestion(
            "What is the purpose of Two-Factor Authentication (2FA)?",
            new List<string> {
                "To make logging in faster",
                "To provide a backup login method",
                "To add an extra layer of security",
                "To reduce password complexity requirements"
            },
            2,
            "2FA adds an extra verification step, making it harder for attackers to access accounts even if they have your password."),

        new QuizQuestion(
            "Which of these is NOT a common phishing technique?",
            new List<string> {
                "CEO fraud emails",
                "Fake login pages",
                "DNS caching",
                "Invoice scams"
            },
            2,
            "DNS caching is a normal network process, not an attack method. CEO fraud, fake logins, and invoice scams are all phishing techniques."),

        new QuizQuestion(
            "When using public WiFi, you should:",
            new List<string> {
                "Access your bank account normally",
                "Use a VPN for sensitive activities",
                "Disable your firewall for better speed",
                "Share your login credentials with the hotspot"
            },
            1,
            "Always use a VPN on public WiFi to encrypt your traffic and protect against snooping."),

        new QuizQuestion(
            "What does HTTPS indicate in a website URL?",
            new List<string> {
                "The site has high traffic",
                "The connection is encrypted",
                "The site is government-approved",
                "The content is verified as true"
            },
            1,
            "HTTPS means the connection is encrypted with SSL/TLS, protecting data in transit."),

        new QuizQuestion(
            "How often should you update your software?",
            new List<string> {
                "Only when new features are added",
                "When the manufacturer stops supporting it",
                "As soon as security updates are available",
                "Once every 5 years"
            },
            2,
            "Security updates should be installed immediately as they often patch critical vulnerabilities."),

        new QuizQuestion(
            "What is a security key benefit of using a password manager?",
            new List<string> {
                "It remembers passwords for you",
                "It generates and stores strong, unique passwords",
                "It automatically changes your passwords",
                "It prevents all hacking attempts"
            },
            1,
            "Password managers create and store strong, unique passwords for each account, eliminating password reuse risks."),

        new QuizQuestion(
            "Which of these is NOT a good practice for social media privacy?",
            new List<string> {
                "Reviewing privacy settings regularly",
                "Accepting friend requests from strangers",
                "Being cautious about location sharing",
                "Using strong, unique passwords"
            },
            1,
            "Never accept connections from people you don't know - this is a common social engineering tactic."),

        new QuizQuestion(
            "What should you do if you suspect malware infection?",
            new List<string> {
                "Continue working normally",
                "Disconnect from the network and scan your system",
                "Email the suspicious file to IT",
                "Restart your computer repeatedly"
            },
            1,
            "Disconnecting prevents spread while you run antivirus scans. Never forward suspected malware."),

        new QuizQuestion(
            "Why is backing up data important for security?",
            new List<string> {
                "It makes your computer faster",
                "It protects against ransomware and data loss",
                "It automatically removes viruses",
                "It replaces the need for antivirus software"
            },
            1,
            "Regular backups are your best defense against ransomware and hardware failures.")
    };
        }

        public int GetTotalQuestions() => _questions.Count;

        public void ResetQuiz()
        {
            _currentQuestionIndex = -1;
            Score = 0;
        }

        public bool MoveToNextQuestion()
        {
            if (QuizComplete) return false;

            _currentQuestionIndex++;
            CurrentQuestion = _questions[_currentQuestionIndex];
            return true;
        }

        // Add this new method
        public string GetProgressText() => $"{_currentQuestionIndex + 1}/{_questions.Count}";


        public bool CheckAnswer(int selectedIndex)
        {
            bool isCorrect = selectedIndex == CurrentQuestion.CorrectAnswerIndex;
            if (isCorrect) Score++;
            return isCorrect;
        }
    }

    public class QuizQuestion
    {
        public string QuestionText { get; }
        public List<string> Options { get; }
        public int CorrectAnswerIndex { get; }
        public string Explanation { get; }

        public QuizQuestion(string questionText, List<string> options,
                          int correctAnswerIndex, string explanation = null)
        {
            QuestionText = questionText;
            Options = options;
            CorrectAnswerIndex = correctAnswerIndex;
            Explanation = explanation;
        }
    }


}
