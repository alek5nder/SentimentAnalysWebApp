using System;
using System.ComponentModel.DataAnnotations;

namespace WebAppAI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string UniqueClientId { get; set; } // np. z ciasteczka

        public ICollection<MessageAnalysisRecord> MessageAnalyses { get; set; }
    }

}
