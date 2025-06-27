using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbotGUI1
{
    public class ActivityLog
    {
        public DateTime Timestamp { get; }
        public string Action { get; }
        public string Details { get; }

        public ActivityLog(string action, string details)
        {
            Timestamp = DateTime.Now;
            Action = action;
            Details = details;
        }

        public override string ToString() =>
            $"[{Timestamp:HH:mm:ss}] {Action}: {Details}";
    }
}
