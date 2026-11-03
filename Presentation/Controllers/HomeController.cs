using System.Diagnostics;
using Application.Deals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDealService _deals;

    public HomeController(IDealService deals)
    {
        _deals = deals;
    }

    public IActionResult Index(int? id)
    {
        DealDto? selected = id is null ? null : _deals.GetById(id.Value);
        return View(new HomeViewModel(_deals.GetAll(), selected));
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
