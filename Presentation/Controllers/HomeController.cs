using System.Diagnostics;
using System.Globalization;
using Application;
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
    private readonly IModuleRepository moduleRepository;

    public HomeController(IDealCollection dealCollection, IRequirementCollection requirementCollection, IMessageCollection messageCollection, IModuleRepository moduleRepository)
    {
        this.dealCollection = dealCollection;
        this.requirementCollection = requirementCollection;
        this.messageCollection = messageCollection;
        this.moduleRepository = moduleRepository;
    }

    public IActionResult Index(Guid? dealId, Guid? requirementId, bool showCreateDealModal = false, bool showCreateRequirementModal = false, bool showUpdateRequirementModal = false,     bool showUpdateDealModal = false, string? sort = null)
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

        if (selectedRequirement is not null && selectedRequirement.DealId != selectedDeal?.Id)
            return NotFound();

        IReadOnlyList<MessageResponseDto> messagesForRequirement = selectedRequirement is null
            ? new List<MessageResponseDto>()
            : messageCollection.GetMessagesForRequirement(selectedRequirement.Id);

        IReadOnlyList<DealSortOption> sortOptions = DealSortOption.GetAll();
        string? requestedSort = sort ?? Request.Cookies["dealSort"];
        DealSortOption activeSort = sortOptions.FirstOrDefault(option => option.Key == requestedSort) ?? sortOptions[0];
        IReadOnlyList<DealResponseDto> deals = dealCollection.GetAll();
        Dictionary<Guid, AttentionInfo> dealAttention = new();
        Dictionary<Guid, AttentionInfo> requirementAttention = new();
        foreach (DealResponseDto deal in deals)
        {
            List<string> dealEvents = [$"deal:{deal.Id}"];
            bool dealPending = false;
            foreach (RequirementResponseDto requirement in requirementCollection.GetRequirementsForDeal(deal.Id))
            {
                IReadOnlyList<MessageResponseDto> messages = messageCollection.GetMessagesForRequirement(requirement.Id);
                List<string> events = [$"requirement:{requirement.Id}"];
                events.AddRange(messages
                    .Where(message => !string.Equals(message.Author, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(message => $"message:{message.Id}"));
                bool pending = User.IsInRole("Sales") && messages.Any(message => message.RequestedDeadline is not null && !message.DeadlineAdapted);
                requirementAttention[requirement.Id] = new AttentionInfo(events, pending);
                dealEvents.AddRange(events);
                dealPending |= pending;
            }
            dealAttention[deal.Id] = new AttentionInfo(dealEvents, dealPending);
        }

        HomeViewModel viewModel = new HomeViewModel(
            activeSort.Apply(deals),
            selectedDeal,
            requirementsForDeal,
            selectedRequirement,
            messagesForRequirement,
            sortOptions,
            activeSort.Key,
            dealAttention,
            requirementAttention,
            moduleRepository.GetAll()
        );

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SendMessage(Guid dealId, Guid requirementId, string messageText)
    {
        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);
        if (dealCollection.GetById(dealId) is null || requirement is null || requirement.DealId != dealId)
            return NotFound();

        if (string.IsNullOrWhiteSpace(messageText))
        {
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        string author = User.Identity?.Name ?? "Anonymous";
        string text = messageText.Trim();
        if (text.StartsWith('/'))
        {
            string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!User.IsInRole("Developer") || parts.Length != 2 || parts[0] != "/deadline"
                || !DateOnly.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly deadline)
                || deadline < DateOnly.FromDateTime(DateTime.Today))
            {
                TempData["ChatError"] = "Developers can request a future or current date using /deadline YYYY-MM-DD.";
                return RedirectToAction("Index", new { dealId, requirementId });
            }
            messageCollection.SendDeadlineRequest(requirementId, author, deadline);
        }
        else
        {
            messageCollection.SendMessage(requirementId, author, text);
        }

        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult AdaptDeadline(Guid dealId, Guid requirementId, Guid messageId)
    {
        DealResponseDto? deal = dealCollection.GetById(dealId);
        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);
        MessageResponseDto? message = messageCollection.GetMessageById(messageId);
        if (deal is null || requirement is null || requirement.DealId != dealId || message is null || message.RequirementId != requirementId)
            return NotFound();
        if (message.RequestedDeadline is not DateOnly deadline || message.DeadlineAdapted
            || deal.Status == "Completed")
        {
            TempData["ChatError"] = "This deadline request can no longer be adapted.";
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        dealCollection.UpdateDealDetails(dealId, deal.Title, deal.Customer, deal.Description, deadline);
        requirementCollection.UpdateDeadline(requirementId, deadline);
        messageCollection.AdaptDeadlineRequest(messageId);
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Developer")]
    [ValidateAntiForgeryToken]
    public IActionResult AddModule(Guid dealId, Guid requirementId, List<Guid> moduleIds)
    {
        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);
        if (dealCollection.GetById(dealId) is null || requirement is null || requirement.DealId != dealId)
            return NotFound();

        if (!ModelState.IsValid || moduleIds.Count == 0 || moduleIds.Any(id => moduleRepository.GetById(id) is null))
        {
            TempData["RequirementModuleError"] = "Select one or more valid modules from the module library.";
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        requirementCollection.UpdateModules(requirementId, requirement.Modules.Select(module => module.Id).Concat(moduleIds));
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Developer")]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveModule(Guid dealId, Guid requirementId, Guid moduleId)
    {
        RequirementResponseDto? requirement = requirementCollection.GetRequirementById(requirementId);
        if (dealCollection.GetById(dealId) is null || requirement is null || requirement.DealId != dealId)
            return NotFound();
        if (!requirement.Modules.Any(module => module.Id == moduleId))
            return NotFound();

        requirementCollection.UpdateModules(requirementId, requirement.Modules.Where(module => module.Id != moduleId).Select(module => module.Id));
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
    public IActionResult CreateDeal(string title, string customer, string description, DateOnly EstimatedDeadline, Guid? dealId, Guid? requirementId)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(description) || EstimatedDeadline == default)
        {
            TempData["CreateDealError"] = "All fields are required and the end date must be valid.";
            return RedirectToAction("Index", new { dealId, requirementId, showCreateDealModal = true });
        }

        dealCollection.Create(title.Trim(), customer.Trim(), description.Trim(), EstimatedDeadline);
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
    public IActionResult UpdateDeal(Guid dealId, Guid? requirementId, string title, string customer, string description, DateOnly EstimatedDeadline)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(description) || EstimatedDeadline == default)
        {
            TempData["UpdateDealError"] = "All fields are required and the end date must be valid.";
            TempData["DealTitle"] = title;
            TempData["DealCustomer"] = customer;
            TempData["DealDescription"] = description;
            TempData["DealEstimatedDeadline"] = EstimatedDeadline.ToString("yyyy-MM-dd");
            return RedirectToAction("Index", new { dealId, requirementId, showUpdateDealModal = true });
        }

        dealCollection.UpdateDealDetails(dealId, title.Trim(), customer.Trim(), description.Trim(), EstimatedDeadline);
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

        requirementCollection.CreateRequirement(dealId, title.Trim(), description.Trim(), requirementType, requirementPriority);
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

        requirementCollection.UpdateRequirement(requirementId, title.Trim(), description.Trim(), requirementType, requirementPriority);
        return RedirectToAction("Index", new { dealId, requirementId });
    }
}
