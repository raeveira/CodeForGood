using System.Diagnostics;
using Application.Deals;
using Application.Messages;
using Application.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public IActionResult Index(Guid? dealId, Guid? requirementId, bool showCreateDealModal = false, bool showCreateRequirementModal = false, bool showUpdateRequirementModal = false, bool showUpdateDealModal = false)
    {
        ViewData["ShowUpdateDealModal"] = showUpdateDealModal && dealId is not null && User.IsInRole("Sales");
        ViewData["ShowCreateDealModal"] = showCreateDealModal && User.IsInRole("Sales");
        ViewData["ShowCreateRequirementModal"] = showCreateRequirementModal && User.IsInRole("Sales");
        ViewData["ShowUpdateRequirementModal"] = showUpdateRequirementModal && requirementId is not null && User.IsInRole("Sales");

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

        // Sort deals by creation date, newest first
        // then sort deals by their status, in progress -> open -> completed 
        IReadOnlyList<DealResponseDto> sortedDeals = [.. dealCollection.GetAll().OrderByDescending(deal => deal.CreatedAt).ToList().OrderBy(deal =>
            Convert.ToString(deal.Status)?.Replace(" ", string.Empty).ToLowerInvariant() switch
            {
                "inprogress" => 0,
                "open" => 1,
                "completed" => 2,
                _ => 3
            })];


        HomeViewModel viewModel = new HomeViewModel(
            sortedDeals,
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

    [HttpPost]
    [Authorize(Roles = "Developer")]
    public IActionResult AddModule(Guid dealId, Guid requirementId, string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);
        if (requirement is not null)
        {
            List<string> modules = new List<string>(requirement.Modules)
            {
                moduleName.Trim()
            };
            requirementCollection.UpdateRequirement(requirementId, requirement.Title, requirement.Description, Enum.Parse<ERequirementType>(requirement.Type), Enum.Parse<ERequirementPriority>(requirement.Priority), modules);
        }

        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Developer")]
    public IActionResult RemoveModule(Guid dealId, Guid requirementId, string module)
    {
        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);
        if (requirement is not null)
        {
            List<string> modules = requirement.Modules.Where(m => m != module).ToList();

            if (modules.Count > 0)
            {
                requirementCollection.UpdateRequirement(requirementId, requirement.Title, requirement.Description, Enum.Parse<ERequirementType>(requirement.Type), Enum.Parse<ERequirementPriority>(requirement.Priority), modules);
            }
        }

        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult OpenCreateDealModal(Guid? dealId, Guid? requirementId)
    {
        return RedirectToAction("Index", new { dealId, requirementId, showCreateDealModal = true });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult CloseCreateDealModal(Guid? dealId, Guid? requirementId)
    {
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateDeal(string title, string customer, string description, DateOnly endDate, Guid? dealId, Guid? requirementId)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(description) || endDate == default)
        {
            TempData["CreateDealError"] = "All fields are required and the end date must be valid.";
            return RedirectToAction("Index", new { dealId, requirementId, showCreateDealModal = true });
        }

        dealCollection.Create(title.Trim(), customer.Trim(), description.Trim(), endDate);
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult OpenUpdateDealModal(Guid dealId, Guid? requirementId)
    {
        return RedirectToAction("Index", new { dealId, requirementId, showUpdateDealModal = true });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult CloseUpdateDealModal(Guid dealId, Guid? requirementId)
    {
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateDeal(Guid dealId, Guid? requirementId, string title, string customer, string description, DateOnly endDate)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(description) || endDate == default)
        {
            TempData["UpdateDealError"] = "All fields are required and the end date must be valid.";
            TempData["DealTitle"] = title;
            TempData["DealCustomer"] = customer;
            TempData["DealDescription"] = description;
            TempData["DealEndDate"] = endDate.ToString("yyyy-MM-dd");
            return RedirectToAction("Index", new { dealId, requirementId, showUpdateDealModal = true });
        }

        dealCollection.UpdateDealDetails(dealId, title.Trim(), customer.Trim(), description.Trim(), endDate);
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteDeal(Guid dealId)
    {
        dealCollection.DeleteDeal(dealId);
        return RedirectToAction("Index", new { dealId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult OpenCreateRequirementModal(Guid dealId)
    {
        return RedirectToAction("Index", new { dealId, showCreateRequirementModal = true });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult CloseCreateRequirementModal(Guid dealId)
    {
        return RedirectToAction("Index", new { dealId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateRequirement(string title, string description, string type, string priority, Guid dealId)
    {
        if (dealCollection.GetById(dealId) is null)
        {
            return NotFound();
        }

        // Only developers can add modules, so for sales we initialize an empty list.
        List<string> moduleList = new List<string>();

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description)
            || !Enum.TryParse(type, out ERequirementType requirementType) || !Enum.IsDefined(requirementType)
            || !Enum.TryParse(priority, out ERequirementPriority requirementPriority) || !Enum.IsDefined(requirementPriority)
            )
        {
            TempData["CreateRequirementError"] = "Enter a title, description, valid type and priority.";
            TempData["RequirementTitle"] = title;
            TempData["RequirementDescription"] = description;
            TempData["RequirementType"] = type;
            TempData["RequirementPriority"] = priority;
            return RedirectToAction("Index", new { dealId, showCreateRequirementModal = true });
        }

        requirementCollection.CreateRequirement(dealId, title.Trim(), description.Trim(), requirementType, requirementPriority, moduleList);
        return RedirectToAction("Index", new { dealId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteRequirement(Guid dealId, Guid requirementId)
    {
        if (dealCollection.GetById(dealId) is null)
        {
            return NotFound();
        }

        if (requirementCollection.GetRequirementById(requirementId) is null)
        {
            return NotFound();
        }

        requirementCollection.DeleteRequirement(requirementId);
        return RedirectToAction("Index", new { dealId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult OpenUpdateRequirementModal(Guid dealId, Guid requirementId)
    {
        return RedirectToAction("Index", new { dealId, requirementId, showUpdateRequirementModal = true });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult CloseUpdateRequirementModal(Guid dealId, Guid requirementId)
    {
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateRequirement(Guid dealId, Guid requirementId, string title, string description, string type, string priority)
    {
        if (dealCollection.GetById(dealId) is null)
        {
            return NotFound();
        }

        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);

        if (requirement is null)
        {
            return NotFound();
        }

        // Sales is not allowed to modify modules directly, so we keep the existing list.
        List<string> modules = requirement.Modules.ToList();

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description)
            || !Enum.TryParse(type, out ERequirementType requirementType) || !Enum.IsDefined(requirementType)
            || !Enum.TryParse(priority, out ERequirementPriority requirementPriority) || !Enum.IsDefined(requirementPriority)
            )
        {
            TempData["UpdateRequirementError"] = "Enter a title, description, valid type and priority.";
            TempData["RequirementTitle"] = title;
            TempData["RequirementDescription"] = description;
            TempData["RequirementType"] = type;
            TempData["RequirementPriority"] = priority;
            return RedirectToAction("Index", new { dealId, requirementId, showUpdateRequirementModal = true });
        }

        requirementCollection.UpdateRequirement(requirementId, title.Trim(), description.Trim(), requirementType, requirementPriority, modules);
        return RedirectToAction("Index", new { dealId, requirementId });
    }
}
