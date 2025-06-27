using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbotGUI1
{
    public class TaskManager
    {
        public ObservableCollection<SecurityTask> Tasks { get; } = new();

        public class SecurityTask
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime? DueDate { get; set; }
            public bool IsCompleted { get; set; }
            public string Category { get; set; }


        }

        public void AddTask(string title, string description, DateTime? dueDate)
        {
            Tasks.Add(new SecurityTask
            {
                Title = title,
                Description = description,
                DueDate = dueDate
            });
        }



        public void CompleteTask(SecurityTask task)
        {
            task.IsCompleted = true;
        }

        public IEnumerable<SecurityTask> GetUpcomingTasks()
            => Tasks.Where(t => !t.IsCompleted &&
                              (t.DueDate == null || t.DueDate >= DateTime.Today))
                   .OrderBy(t => t.DueDate);
    }
}

