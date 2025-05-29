using System;
using System.ComponentModel.DataAnnotations;

namespace WebAppAI.Models
{
    public class MessageAnalysisRecord
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public string Sentiment { get; set; }

        public double Confidence { get; set; }

        public int WordCount { get; set; }

        public int CharCount { get; set; }

        // RELACJE
        public int UserId { get; set; }
        public string UserIp { get; set; } // adres IP użytkownika

        public User User { get; set; }

        public int SentimentModelId { get; set; }
        public SentimentModel SentimentModel { get; set; }
    }

}
