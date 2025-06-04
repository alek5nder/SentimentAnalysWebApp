using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjektTI.Resources;
using SelectPdf;
using System.Text;
using WebAppAI.Models;


namespace WebAppAI.Controllers
{
    public class ExpToPdfController : Controller
    {
        [HttpGet]
        public IActionResult ExportToPdf()
        {
            var resultsJson = TempData["ResultsJson"] as string;

            if (string.IsNullOrEmpty(resultsJson))
            {
                return RedirectToAction("Index", "Sentiment");
            }

            var results = JsonConvert.DeserializeObject<List<SentimentResultModel>>(resultsJson);

            var htmlContent = new StringBuilder();
            htmlContent.Append($"<h2>{ResultLabels.ResultTitle}</h2>");
            htmlContent.Append("<table border='1' cellpadding='5' cellspacing='0'>");
            htmlContent.Append("<thead><tr>");
            htmlContent.Append($"<th>{ResultLabels.MessageLabel}</th>");
            htmlContent.Append($"<th>{ResultLabels.Date}</th>");
            htmlContent.Append($"<th>{ResultLabels.WordCount}</th>");
            htmlContent.Append($"<th>{ResultLabels.CharCount}</th>");
            htmlContent.Append($"<th>{ResultLabels.Sentiment}</th>");
            htmlContent.Append($"<th>{ResultLabels.Confidence}</th>");
            htmlContent.Append("</tr></thead><tbody>");

            foreach (var result in results)
            {
                htmlContent.Append("<tr>");
                htmlContent.Append($"<td>{System.Net.WebUtility.HtmlEncode(result.OriginalMessage)}</td>");
                htmlContent.Append($"<td>{result.Timestamp:yyyy-MM-dd HH:mm:ss}</td>");
                htmlContent.Append($"<td>{result.WordCount}</td>");
                htmlContent.Append($"<td>{result.CharCount}</td>");
                htmlContent.Append($"<td>{result.Sentiment}</td>");
                htmlContent.Append($"<td>{Math.Round(result.Confidence * 100, 2)}%</td>");
                htmlContent.Append("</tr>");
            }

            htmlContent.Append("</tbody></table>");

            var pdf = new HtmlToPdf();
            var doc = pdf.ConvertHtmlString(htmlContent.ToString());

            var pdfBytes = doc.Save();
            doc.Close();

            return File(pdfBytes, "application/pdf", ResultLabels.ResultTitle+"_"+ DateTime.Now.ToShortDateString() +".pdf");
        }
    }
}
