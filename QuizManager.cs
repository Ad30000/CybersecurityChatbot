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
        new QuizQuestion(
            "What should you do if you receive an email asking for your password?",
            new List<string> {
                "Reply with your password",
                "Delete the email",
                "Report the email as phishing",
                "Ignore it"
            },
            2, // Correct answer index
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
        
        // Add more questions with explanations...
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
