using System;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using ProjektTI.Resources;

namespace WebAppAI.Models
{
    public class TextInputModel
    {
        [Key]
        public int Id { get; set; }

        [Display(ResourceType = typeof(FormLabels), Name = "MessageLabel")]
        [Required(ErrorMessageResourceType = typeof(FormLabels), ErrorMessageResourceName = "RequiredError")]
        [StringLength(1000, ErrorMessageResourceType = typeof(FormLabels), ErrorMessageResourceName = "TextLength")]
        public string Message { get; set; }

        [Display(Name = "Data wysłania")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Display(ResourceType = typeof(FormLabels),Name = "WordCount")]
        public int WordCount => !string.IsNullOrWhiteSpace(Message)
            ? Message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
            : 0;

        [Display(ResourceType =typeof(FormLabels),Name = "CharCount")]
        public int CharCount => Message?.Length ?? 0;

        public string ToShortPreview(int maxLength = 50)
        {
            if (string.IsNullOrEmpty(Message))
                return string.Empty;

            return Message.Length > maxLength
                ? Message.Substring(0, maxLength) + "..."
                : Message;
        }
    }
}
