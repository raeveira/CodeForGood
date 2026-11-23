using Microsoft.AspNetCore.Mvc;
[Route("[controller]")]
public class UnauthorizedController : Controller
{
    [HttpGet("/unauthorized")]
    public IActionResult Index()
    {
        return View();
    }
}