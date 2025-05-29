using System;

namespace WebAppAI.Models
{
    public class SentimentResultModel
    {
        public string OriginalMessage { get; set; }

        public DateTime Timestamp { get; set; }

        public int WordCount { get; set; }

        public int CharCount { get; set; }

        public string Sentiment { get; set; } // "Pozytywny", "Negatywny", "Neutralny", etc.

        public double Confidence { get; set; } // np. 0.875 (czyli 87.5%)

        public string ToConfidencePercent()
        {
            return $"{Confidence * 100:0.##}%";
        }

        public string ToShortPreview(int maxLength = 50)
        {
            if (string.IsNullOrEmpty(OriginalMessage))
                return string.Empty;

            return OriginalMessage.Length > maxLength
                ? OriginalMessage.Substring(0, maxLength) + "..."
                : OriginalMessage;
        }
    }
}
