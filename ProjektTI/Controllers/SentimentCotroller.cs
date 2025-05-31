using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjektTI.Models;
using System.Net.Http;
using System.Text;
using WebAppAI.Data;
using WebAppAI.Models;
using SelectPdf; //do pdf


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
        public async Task<IActionResult> History()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            var ip = ipAddress?.IsIPv4MappedToIPv6 == true
                ? ipAddress.MapToIPv4().ToString()
                : ipAddress?.ToString();

            var allRecords = await _db.MessageAnalyses
                .Where(r => r.UserIp == ip)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();


            return View(allRecords);
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

        [HttpPost]
        public async Task<IActionResult> Analyze(List<TextInputModel> messages)
        {
            if (!ModelState.IsValid)
            {
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
                    Confidence = new Random().NextDouble() * 0.5 + 0.5 // 50–100%
                };
            }
                        

        }
    }
}
