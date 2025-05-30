using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    public IActionResult LanguageChangeNotAllowed()
    {
        return View();
    }
}
