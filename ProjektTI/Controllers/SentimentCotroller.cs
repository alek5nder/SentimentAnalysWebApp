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
using System.Text.RegularExpressions; //do pdf


namespace WebAppAI.Controllers
{
    public class SentimentController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;

        private string GetClientIp()
        {
            var forwardedHeader = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // Może zawierać wiele IP oddzielonych przecinkami — bierzemy pierwszy
                return forwardedHeader.Split(',')[0];
            }

            var ip = HttpContext.Connection.RemoteIpAddress;
            return ip?.IsIPv4MappedToIPv6 == true
                ? ip.MapToIPv4().ToString()
                : ip?.ToString() ?? "unknown";
        }


        //historia analiz:
        [HttpGet]
        public async Task<IActionResult> History(string userType, DateTime? date, 
            string sentiment, int? minWords, double? minConfidence, 
            string sortColumn, string sortDirection, string? messageContains)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            var ip = ipAddress?.IsIPv4MappedToIPv6 == true
                ? ipAddress.MapToIPv4().ToString()
                : ipAddress?.ToString();

            var query = _db.MessageAnalyses.Where(r => r.UserIp == ip);

            // jeśli użytkownik to użytkownik premium to pokazujemy wszystkie daty, jeśli nie, to tylko dzisiejszą
            if (userType != "premium")
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
                query = query.Where(r => r.Message.StartsWith(messageContains));
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

        //database:
        private readonly SentimentDbContext _db;

        public SentimentController(IHttpClientFactory httpClientFactory, SentimentDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
        }

        //zapis wyników do db

        [HttpGet]
        public IActionResult Index()
        {
            var initialList = new List<TextInputModel>
            {
                new TextInputModel()
            };
            return View(initialList);
        }

        // usuwanie rekordu z db
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var record = _db.MessageAnalyses.FirstOrDefault(x =>  x.Id == id);
            if (record != null)
            {
                _db.MessageAnalyses.Remove(record);
                _db.SaveChanges();
            }
            return RedirectToAction("History");
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(List<TextInputModel> messages)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", messages);
            }

            // sprawdzenie, czy użytkownik wpisał wiadomość - ma wyświetlać error ale nie działa :(
            if (messages == null || !messages.Any(m => !string.IsNullOrWhiteSpace(m.Message)))
            {
                ModelState.AddModelError(string.Empty, "Wprowadź przynajmniej jedną wiadomość.");
                return View("Index", messages);
            }

            // 🔐 Pobieramy unikalny identyfikator użytkownika z ciasteczka
            var clientId = Request.Cookies["ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = Guid.NewGuid().ToString();
                Response.Cookies.Append("ClientId", clientId);
            }

            // 🔍 Znajdź lub utwórz użytkownika
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UniqueClientId == clientId);
            if (user == null)
            {
                user = new User { UniqueClientId = clientId };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            // 🔍 Znajdź lub utwórz model AI (hardcoded dla uproszczenia)
            var modelName = "SentimentModel v1";
            var sentimentModel = await _db.SentimentModels.FirstOrDefaultAsync(m => m.Name == modelName);
            if (sentimentModel == null)
            {
                sentimentModel = new SentimentModel
                {
                    Name = modelName,
                    Version = "1.0"
                };
                _db.SentimentModels.Add(sentimentModel);
                await _db.SaveChangesAsync();
            }

            var sentimentResults = new List<SentimentResultModel>();

            foreach (var message in messages)
            {
                if (string.IsNullOrWhiteSpace(message.Message))
                    continue;

                var result = await CallPythonApiAsync(message.Message);

                var record = new MessageAnalysisRecord
                {
                    Message = message.Message,
                    Timestamp = DateTime.Now,
                    Sentiment = result.Sentiment,
                    Confidence = result.Confidence,
                    WordCount = message.WordCount,
                    CharCount = message.CharCount,
                    UserId = user.Id,
                    SentimentModelId = sentimentModel.Id,
                    UserIp = GetClientIp()
                };


                _db.MessageAnalyses.Add(record);

                sentimentResults.Add(new SentimentResultModel
                {
                    OriginalMessage = message.Message,
                    Timestamp = record.Timestamp,
                    WordCount = record.WordCount,
                    CharCount = record.CharCount,
                    Sentiment = result.Sentiment,
                    Confidence = result.Confidence
                });
            }

            await _db.SaveChangesAsync();

            TempData["ResultsJson"] = JsonConvert.SerializeObject(sentimentResults);

            return View("Results", sentimentResults);
        }

        [HttpGet]
        public IActionResult ChooseUserType()
        {
            return View("UserTypeChoice");
        }



        private async Task<SentimentResultModel> CallPythonApiAsync(string message)
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new { message };
            var json = JsonConvert.SerializeObject(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync("http://localhost:5000/predict", content);

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<SentimentResultModel>(responseJson);

                return result ?? new SentimentResultModel
                {
                    Sentiment = "Nieznany",
                    Confidence = 0.0
                };
            }
            catch (Exception ex)
            {
                // Tryb demo: udawany wynik przy braku serwera Python
                return new SentimentResultModel
                {
                    Sentiment = "ZEPSULO SIE (model z pythona nie dziala)",
                    Confidence =  0 //new Random().NextDouble() * 0.5 + 0.5 // 50–100%
                };
            }
                        

        }
    }
}
