using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    public IActionResult LanguageChangeNotAllowed()
    {
        return View();
    }

    //wlasne strony błędów:'

    [Route("Error/404")]
    public IActionResult PageNotFound()
    {
        return View("404");
    }

    [Route("Error/500")]
    public IActionResult ServerError()
    {
        return View("500");
    }

    [Route("Error")]
    public IActionResult General()
    {
        return View("General");
    }

}
