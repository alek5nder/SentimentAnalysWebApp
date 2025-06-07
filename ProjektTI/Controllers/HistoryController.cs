using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjektTI.Models;
using System.Net.Http;
using System.Text;
using WebAppAI.Data;
using WebAppAI.Models;
using SelectPdf;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace ProjektTI.Controllers
{
    public class HistoryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;


        //database (potrzebny do wyswietlenia historii):
        private readonly SentimentDbContext _db;

        public HistoryController(IHttpClientFactory httpClientFactory, SentimentDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
        }

        //historia analiz:
        [Authorize]
        public async Task<IActionResult> History(DateTime? date, string sentiment, int? minWords,
            double? minConfidence, string sortColumn, string sortDirection, string? messageContains)
        {
            // sprawdzamy, czy użytkownik jest użytkownikiem premium
            var isPremium = User.IsInRole("Premium");

            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            var ip = ipAddress?.IsIPv4MappedToIPv6 == true
                ? ipAddress.MapToIPv4().ToString()
                : ipAddress?.ToString();

            var query = _db.MessageAnalyses.Where(r => r.UserIp == ip);

            // jeśli użytkownik to użytkownik premium to pokazujemy wszystkie daty, jeśli nie, to tylko dzisiejszą
            if (!isPremium)
            {
                var today = DateTime.Today;
                query = query.Where(r => r.Timestamp.Date == today);
            }

            // filtr - data
            if (date.HasValue)
            {
                query = query.Where(r => r.Timestamp.Date == date.Value.Date);
            }

            // filtr - sentyment
            if (!string.IsNullOrEmpty(sentiment))
            {
                query = query.Where(r => r.Sentiment == sentiment);
            }


            // filtr - min. liczba słów
            if (minWords.HasValue)
            {
                query = query.Where(r => r.WordCount >= minWords.Value);
            }

            // filtr - poziom ufności
            if (minConfidence.HasValue)
            {
                query = query.Where(r => r.Confidence >= minConfidence.Value);
            }

            // filtr - wiadomość 
            if (!string.IsNullOrEmpty(messageContains))
            {
                query = query.Where(r => r.Message.Contains(messageContains));
            }

            // Sortowanie
            query = (sortColumn, sortDirection) switch
            {
                ("Message", "asc") => query.OrderBy(r => r.Message),
                ("Message", "desc") => query.OrderByDescending(r => r.Message),
                ("Timestamp", "asc") => query.OrderBy(r => r.Timestamp),
                ("Timestamp", "desc") => query.OrderByDescending(r => r.Timestamp),
                ("Sentiment", "asc") => query.OrderBy(r => r.Sentiment),
                ("Sentiment", "desc") => query.OrderByDescending(r => r.Sentiment),
                ("Confidence", "asc") => query.OrderBy(r => r.Confidence),
                ("Confidence", "desc") => query.OrderByDescending(r => r.Confidence),
                ("WordCount", "asc") => query.OrderBy(r => r.WordCount),
                ("WordCount", "desc") => query.OrderByDescending(r => r.WordCount),
                ("CharCount", "asc") => query.OrderBy(r => r.CharCount),
                ("CharCount", "desc") => query.OrderByDescending(r => r.CharCount),
                _ => query.OrderByDescending(r => r.Timestamp) // domyślnie
            };

            var records = await query.ToListAsync();
            return View(records);

        }
    }
}
