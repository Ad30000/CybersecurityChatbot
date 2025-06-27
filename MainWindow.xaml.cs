using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CybersecurityChatbotGUI;
using static CybersecurityChatbotGUI1.TaskManager;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using System.Diagnostics;


namespace CybersecurityChatbotGUI1
{
    public partial class MainWindow : Window
    {
        // Core components
        private readonly UserMemory _userMemory = new();
        private readonly ResponseManager _responseManager = new();
        private readonly TaskManager _taskManager = new();
        private readonly SentimentDetector _sentimentDetector = new();
        private readonly QuizManager _quizManager = new();
        private readonly ObservableCollection<ActivityLog> _activityLogs = new();
        private DispatcherTimer _reminderTimer;
        private bool _waitingForContinue = false;
        private bool _isAddingTaskFromChat = false;
        private string _pendingTaskTitle = string.Empty;
        private bool _awaitingTaskDescription = false;




        public MainWindow()
        {
            InitializeComponent();
            ActivityLogListView.ItemsSource = _activityLogs;
            SetupBindings();
            InitializeSystems();
            PlayWelcomeSound();
            LogStartup();
            ClearTaskInputs();
            ResetQuizButtonColors();

        }

        #region Initialization
        private void SetupBindings()
        {
            TasksListView.ItemsSource = _taskManager.Tasks;
            ActivityLogListView.ItemsSource = _activityLogs;
        }

        private void InitializeSystems()
        {
            InitializeReminderTimer();
            InitializeQuiz();
            RefreshTaskList();
        }

        private void PlayWelcomeSound()
        {
            try
            {
                var uri = new Uri("greeting sound 1.wav", UriKind.RelativeOrAbsolute);
                var player = new MediaPlayer();
                player.Open(uri);
                player.Play();

                // Optional: Release resources when done
                player.MediaEnded += (s, e) => player.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sound error: {ex.Message}");
            }
        }

        private void LogStartup()
        {
            LogActivity("System", "Application started");
            ShowWelcomeMessage();
        }
        #endregion

        #region Chat Functionality
        private void ShowWelcomeMessage()
        {
            ChatBox.Items.Add("Bot: Welcome to Cybersecurity Assistant!");
            if (!string.IsNullOrEmpty(_userMemory.UserName))
                ChatBox.Items.Add($"Bot: Welcome back, {_userMemory.UserName}!");
        }

        // This method logs an activity in the activity log
        private void LogActivity(string action, string details)
        {
            _activityLogs.Add(new ActivityLog(action, details));
            ActivityLogListView.ScrollIntoView(_activityLogs.Last());
        }

        private void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = $"security_log_{DateTime.Now:yyyyMMdd_HHmm}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllLines(saveDialog.FileName,
                    _activityLogs.Select(log => log.ToString()));
                LogActivity("System", $"Log exported to {saveDialog.FileName}");
            }
        }

        // This method clears the activity log
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Clear all activity logs?", "Confirm",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _activityLogs.Clear();
                LogActivity("System", "Activity log cleared");
            }
        }

        // This method handles the user input from the chat box
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = UserInput.Text;
            if (string.IsNullOrWhiteSpace(userInput)) return;

            ChatBox.Items.Add($"You: {userInput}");
            LogActivity("Chat", $"User message: {userInput}");
            UserInput.Clear();

            ProcessUserInput(userInput);
            ChatBox.ScrollIntoView(ChatBox.Items[ChatBox.Items.Count - 1]);
        }

        // This method processes the user input and generates a response
        private void ProcessUserInput(string input)
        {
            if (_awaitingTaskDescription)
            {
                CompleteTaskCreation(input);
                return;
            }

            var (nlpResponse, command) = _responseManager.ProcessNlpInput(input);
            if (!string.IsNullOrEmpty(nlpResponse))
            {
                ChatBox.Items.Add($"Bot: {nlpResponse}");
                HandleCommand(command, input);  // Now passing both parameters
                return;
            }

            var response = GetBotResponse(input);
            ChatBox.Items.Add($"Bot: {response}");
        }

        // This method generates a response based on user input, checking for NLP commands and sentiment first
        private string GetBotResponse(string userInput)
        {
            var (nlpResponse, command) = _responseManager.ProcessNlpInput(userInput);
            if (!string.IsNullOrEmpty(nlpResponse))
            {
                HandleCommand(command,userInput);
                return nlpResponse;
            }

            // First check for sentiment
            string sentiment = _sentimentDetector.DetectSentiment(userInput);
            if (!string.IsNullOrEmpty(sentiment) && sentiment != "neutral")
            {
                return _sentimentDetector.GetResponse(sentiment);
            }

            // If no sentiment detected, proceed with normal responses
            var (response, isKeyword) = _responseManager.GetResponse(userInput);

            if (!string.IsNullOrEmpty(response))
            {
                // Add personalized response if available
                if (_userMemory.HasFavoriteTopic())
                {
                    response += $"\n\n{_userMemory.GetPersonalizedResponse()}";
                }
                return response;
            }

            // Fallback for completely unrecognized input
            return "I'm not sure I understand. Try asking about:\n- Passwords\n- Phishing\n- Privacy";
        }
        #endregion

        #region Task Management
        // This method adds a new task based on user input
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleBox.Text))
            {
                MessageBox.Show("Please enter a task title");
                return;
            }

            var newTask = new SecurityTask
            {
                Title = TaskTitleBox.Text,
                Description = TaskDescriptionBox.Text,
                DueDate = ReminderCheckBox.IsChecked == true ? ReminderDatePicker.SelectedDate : null
            };

            _taskManager.AddTask(
             title: TaskTitleBox.Text,
             description: TaskDescriptionBox.Text,
             dueDate: ReminderCheckBox.IsChecked == true ? ReminderDatePicker.SelectedDate : null
            );
            LogActivity("Task", $"Added: {newTask.Title}" +
                (newTask.DueDate.HasValue ? $" (Due: {newTask.DueDate:MM/dd/yyyy})" : ""));

            TaskTitleBox.Clear();
            TaskDescriptionBox.Clear();
            ReminderCheckBox.IsChecked = false;
            RefreshTaskList();
        }

        // This method handles the Enter key for task input
        private void RefreshTaskList()
        {
            TasksListView.Items.Refresh();
            LogActivity("Task", $"Task list refreshed - {_taskManager.Tasks.Count} tasks");
        }

        // This method handles the Enter key for task input
        private void ClearTaskInputs()
        {
            TaskTitleBox.Clear();
            TaskDescriptionBox.Clear();
            ReminderCheckBox.IsChecked = false;
        }

        // This method handles the Enter key for task input
        private void InitializeReminderTimer()
        {
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _reminderTimer.Tick += CheckReminders;
            _reminderTimer.Start();
        }

        // This method checks for tasks that are due and shows reminders
        private void CheckReminders(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var dueTasks = _taskManager.Tasks
                .Where(t => t.DueDate.HasValue &&
                           !t.IsCompleted &&
                           t.DueDate.Value.Date == now.Date &&
                           t.DueDate.Value.Hour == now.Hour &&
                           t.DueDate.Value.Minute == now.Minute)
                .ToList();

            foreach (var task in dueTasks)
            {
                ShowReminderNotification(task);
            }
        }

        // This method shows a reminder notification for a task
        private void ShowReminderNotification(TaskManager.SecurityTask task)
        {
            MessageBox.Show($"REMINDER: {task.Title}\n\n{task.Description}",
                           "Task Reminder",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
            LogActivity("Reminder", $"Triggered for: {task.Title}");
        }
        #endregion

        #region Quiz System
        // This method initializes the quiz system
        private void InitializeQuiz()
        {
            _quizManager.ResetQuiz();
            _waitingForContinue = false;
            LoadNextQuestion();
            LogActivity("Quiz", "New quiz session started");
        }

        //  This method loads the next question in the quiz
        private void LoadNextQuestion()
        {
            if (_quizManager.MoveToNextQuestion())
            {
                DisplayCurrentQuestion();
                UpdateQuizProgress();
            }
            else
            {
                EndQuiz();
            }
        }

        // This method resets the quiz button colors    
        private void ResetQuizButtonColors()
        {
            if (QuizOptionsPanel == null) return;

            foreach (Button btn in QuizOptionsPanel.Children.OfType<Button>())
            {
                btn.Background = SystemColors.ControlBrush;
                btn.Foreground = SystemColors.ControlTextBrush;
                btn.IsEnabled = true;
            }
        }

        // This method displays the current question in the quiz
        private void DisplayCurrentQuestion()
        {
            ResetQuizButtonColors();
            QuizQuestion.Text = _quizManager.CurrentQuestion.QuestionText;

            QuizOptionsPanel.Children.Clear();
            for (int i = 0; i < _quizManager.CurrentQuestion.Options.Count; i++)
            {
                var button = new Button
                {
                    Content = _quizManager.CurrentQuestion.Options[i],
                    Tag = i,
                    Margin = new Thickness(5),
                    MinWidth = 200
                };
                button.Click += AnswerButton_Click;
                QuizOptionsPanel.Children.Add(button);
            }

            UpdateQuizProgress();
            NextQuestionButton.Content = _quizManager.IsLastQuestion ? "Finish Quiz" : "Next Question";
        }

        // This method handles the answer button click event
        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedButton = (Button)sender;
            int selectedIndex = (int)selectedButton.Tag;
            bool isCorrect = _quizManager.CheckAnswer(selectedIndex);

            // Highlight answers
            int correctIndex = _quizManager.CurrentQuestion.CorrectAnswerIndex;
            var correctButton = (Button)QuizOptionsPanel.Children[correctIndex];
            correctButton.Background = Brushes.LightGreen;
            correctButton.Foreground = Brushes.Black;

            if (!isCorrect)
            {
                selectedButton.Background = Brushes.LightPink;
                selectedButton.Foreground = Brushes.Black;
            }

            // Disable all answer buttons
            foreach (Button btn in QuizOptionsPanel.Children.OfType<Button>())
            {
                btn.IsEnabled = false;
            }

            // Show explanation if available
            if (!string.IsNullOrEmpty(_quizManager.CurrentQuestion.Explanation))
            {
                QuizQuestion.Text += $"\n\nExplanation: {_quizManager.CurrentQuestion.Explanation}";
            }

            // Change button to "Continue" mode
            NextQuestionButton.Content = "Continue";
            _waitingForContinue = true;

            LogActivity("Quiz", $"Answered Q{_quizManager.CurrentQuestionIndex + 1}: {(isCorrect ? "Correct" : "Incorrect")}");
        }

        // This method updates the quiz progress text   
        private void UpdateQuizProgress()
        {
            QuizProgressText.Text = _quizManager.GetProgressText();
        }

        // This method ends the quiz and displays the final score
        private void EndQuiz()
        {
            QuizQuestion.Text = $"Quiz Complete!\nFinal Score: {_quizManager.Score}/{_quizManager.GetTotalQuestions()}";
            QuizOptionsPanel.Children.Clear();
            NextQuestionButton.Content = "Restart Quiz";
            _waitingForContinue = false;
            LogActivity("Quiz", $"Quiz completed with score: {_quizManager.Score}/{_quizManager.GetTotalQuestions()}");
        }


        // This method handles the Next Question button click event
        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_waitingForContinue)
            {
                // Currently showing answer - proceed to next question
                _waitingForContinue = false;
                LoadNextQuestion();
            }
            else if (_quizManager.QuizComplete)
            {
                InitializeQuiz(); // Restart if quiz is done
            }
            else
            {
                LoadNextQuestion(); // Load next question
            }
        }
        #endregion

        // This method handles the Enter key for user input
        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_quizManager.MoveToNextQuestion())
            {
                DisplayCurrentQuestion();
            }
            else
            {
                EndQuizSession();
            }
        }

        // This method ends the quiz session and displays the final score
        private void EndQuizSession()
        {
            QuizQuestion.Text = $"Quiz Complete!\nFinal Score: {_quizManager.Score}/{_quizManager.GetTotalQuestions()}";
            NextQuestionButton.Content = "Restart Quiz";
            LogActivity("Quiz", $"Completed with score: {_quizManager.Score}/{_quizManager.GetTotalQuestions()}");
            _quizManager.ResetQuiz();
        }


        #region Activity Log

        #endregion

        #region Helper Methods
        // This method handles the Enter key for user input 
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendButton_Click(sender, e);
        }

        // This method handles the Enter key for task input
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SecurityTask task)
            {
                if (MessageBox.Show($"Delete task '{task.Title}'?", "Confirm",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _taskManager.Tasks.Remove(task);
                    LogActivity("Task", $"Deleted: {task.Title}");
                    StatusText.Text = $"Task '{task.Title}' deleted!";
                    RefreshTaskList();
                }
            }
        }

        // This method handles the Enter key for task input 
        private void TaskCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox { DataContext: TaskManager.SecurityTask task })
            {
                task.IsCompleted = !task.IsCompleted;
                var status = task.IsCompleted ? "completed" : "incomplete";
                StatusText.Text = $"Task '{task.Title}' marked as {status}!";
                LogActivity("Task", $"Marked '{task.Title}' as {status}");
            }
        }

        // This method handles the Enter key for task input
        private void HandleCommand(string command, string userInput)
        {
            var parts = command.Split(' ');
            switch (parts[0].ToLower())
            {
                case "addtask":
                    _pendingTaskTitle = string.Join(" ", parts.Skip(1));
                    if (string.IsNullOrWhiteSpace(_pendingTaskTitle))
                    {
                        _pendingTaskTitle = userInput.Replace("add task", "").Replace("create task", "").Trim();
                    }
                    ChatBox.Items.Add($"Bot: Please enter the description for '{_pendingTaskTitle}'");
                    _awaitingTaskDescription = true;
                    break;
                case "startquiz":
                    tabControl.SelectedIndex = 2;
                    InitializeQuiz();
                    break;
            }
        }
        #endregion
        // This method completes the task creation process
        private void CompleteTaskCreation(string description)
        {
            _taskManager.AddTask(_pendingTaskTitle, description, null);
            ChatBox.Items.Add($"Bot: Task '{_pendingTaskTitle}' has been created!");
            LogActivity("Task", $"Added via chat: {_pendingTaskTitle}");
            _awaitingTaskDescription = false;
            _pendingTaskTitle = string.Empty;
            RefreshTaskList();
        }


    }
}
// Part 1

/*
* Author: OpenAI 
* Version: 3.5
* URL:https://chatgpt.com/c/6802b0bb-4470-800d-b635-f7ccf609fb6b
* Accessed on: 19 April 2025*/

/* 
* Author: OpenAI 
* Version: 3.5
* URL:https://chatgpt.com/c/68040a9f-a8dc-800d-9cbe-1467cc315ee2
* Accessed on: 19 April 2025*/

/*  
 * Author: OpenAI 
 * Version: 3.5
 * URL:https://chatgpt.com/c/6802b256-4b84-800d-bab5-93cd57584298
 * Accessed on: 19 April 2025*/

/* 
* Author: OpenAI 
* Version: 3.5
* URL:https://chatgpt.com/c/680616d1-5524-800d-9894-e4a51d329095
* Accessed on: 19 April 2025*/

/* 
 * GeeksforGeeks
 * Author: (N/A) 
 * URL:https://www.geeksforgeeks.org/what-is-cyber-security-awareness/
 * Accessed on: 20 April 2025*/

/* TechTarget
 * Author: M, Smith
 * URL:https://www.techtarget.com/searchsecurity/tip/How-to-create-a-strong-passphrase-with-examples
 * Accessed on: 20 April 2025*/

/* Phishing.org
 * Author: (N/A)
 * URL:https://www.techtarget.com/searchsecurity/tip/How-to-create-a-strong-passphrase-with-examples
 * Accessed on: 20 April 2025*/

// Part 2

/* 
* Author: OpenAI 
* Version: 3.5
* URL:https://chatgpt.com/c/682e4fea-1a18-800d-a7ef-8b89d639e309
* Accessed on: 20 May 2025*/

// Part 3

/*
 * Author: IAmTimCorey   
 * Title: C# user interface 
 * Type: YouTube Playlist  
 * Platform: YouTube  
 * URL:https://www.youtube.com/results?search_query=gui+crash+course+c%23 
 * Published on:  3 Jun 2019  
 * Number of Videos: 22 videos  
 */
