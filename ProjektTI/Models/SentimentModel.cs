using System;
using System.ComponentModel.DataAnnotations;
namespace WebAppAI.Models
{
    public class SentimentModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // np. "GRU v2", "LSTM demo"

        public string Version { get; set; }

        public ICollection<MessageAnalysisRecord> MessageAnalyses { get; set; }
    }

}
