using System.Diagnostics;
using Application.Deals;
using Application.Messages;
using Application.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDealCollection dealCollection;
    private readonly IRequirementCollection requirementCollection;
    private readonly IMessageCollection messageCollection;

    public HomeController(IDealCollection dealCollection, IRequirementCollection requirementCollection, IMessageCollection messageCollection)
    {
        this.dealCollection = dealCollection;
        this.requirementCollection = requirementCollection;
        this.messageCollection = messageCollection;
    }

    public IActionResult Index(Guid? dealId, Guid? requirementId)
    {
        DealResponseDto? selectedDeal = dealId is null ? null : dealCollection.GetById(dealId.Value);

        IReadOnlyList<RequirementResponseDto> requirementsForDeal = selectedDeal is null
            ? new List<RequirementResponseDto>()
            : requirementCollection.GetRequirementsForDeal(selectedDeal.Id);

        RequirementResponseDto? selectedRequirement = requirementId is null
            ? null
            : requirementCollection.GetRequirementById(requirementId.Value);

        IReadOnlyList<MessageResponseDto> messagesForRequirement = selectedRequirement is null
            ? new List<MessageResponseDto>()
            : messageCollection.GetMessagesForRequirement(selectedRequirement.Id);

        HomeViewModel viewModel = new HomeViewModel(
            dealCollection.GetAll(),
            selectedDeal,
            requirementsForDeal,
            selectedRequirement,
            messagesForRequirement
        );

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult SendMessage(Guid dealId, Guid requirementId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        string author = User.Identity?.Name ?? "Anonymous";
        messageCollection.SendMessage(requirementId, author, messageText);

        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

