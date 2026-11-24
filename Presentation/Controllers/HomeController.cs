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
    private readonly IDealService dealService;
    private readonly IRequirementService requirementService;
    private readonly IMessageService messageService;
    private readonly IModuleRepository moduleRepository;

    public HomeController(IDealService dealService, IRequirementService requirementService, IMessageService messageService, IModuleRepository moduleRepository)
    {
        this.dealService = dealService;
        this.requirementService = requirementService;
        this.messageService = messageService;
        this.moduleRepository = moduleRepository;
    }

    public IActionResult Index(Guid? dealId, Guid? requirementId, bool showCreateDealModal = false, bool showCreateRequirementModal = false, bool showUpdateRequirementModal = false,     bool showUpdateDealModal = false, string? sort = null)
    {
        ViewData["ShowUpdateDealModal"] = showUpdateDealModal && dealId is not null && User.IsInRole("Sales");
        ViewData["ShowCreateDealModal"] = showCreateDealModal && User.IsInRole("Sales");
        ViewData["ShowCreateRequirementModal"] = showCreateRequirementModal && User.IsInRole("Sales");
        ViewData["ShowUpdateRequirementModal"] = showUpdateRequirementModal && requirementId is not null && User.IsInRole("Sales");

        DealResponseDto? selectedDeal = dealId is null ? null : dealService.GetById(dealId.Value);

        IReadOnlyList<RequirementResponseDto> requirementsForDeal = selectedDeal is null
            ? new List<RequirementResponseDto>()
            : requirementService.GetRequirementsForDeal(selectedDeal.Id);

        RequirementResponseDto? selectedRequirement = requirementId is null
            ? null
            : requirementService.GetRequirementById(requirementId.Value);

        if (selectedRequirement is not null && selectedRequirement.DealId != selectedDeal?.Id)
            return NotFound();

        IReadOnlyList<MessageResponseDto> messagesForRequirement = selectedRequirement is null
            ? new List<MessageResponseDto>()
            : messageService.GetMessagesForRequirement(selectedRequirement.Id);

        IReadOnlyList<DealSortOption> sortOptions = DealSortOption.GetAll();
        string? requestedSort = sort ?? Request.Cookies["dealSort"];
        DealSortOption activeSort = sortOptions.FirstOrDefault(option => option.Key == requestedSort) ?? sortOptions[0];
        IReadOnlyList<DealResponseDto> deals = dealService.GetAll();
        Dictionary<Guid, AttentionInfo> dealAttention = new();
        Dictionary<Guid, AttentionInfo> requirementAttention = new();
        foreach (DealResponseDto deal in deals)
        {
            List<string> dealEvents = [$"deal:{deal.Id}"];
            bool dealPending = false;
            foreach (RequirementResponseDto requirement in requirementService.GetRequirementsForDeal(deal.Id))
            {
                IReadOnlyList<MessageResponseDto> messages = messageService.GetMessagesForRequirement(requirement.Id);
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
        RequirementResponseDto? requirement = requirementService.GetRequirementById(requirementId);
        if (dealService.GetById(dealId) is null || requirement is null || requirement.DealId != dealId)
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
            messageService.SendDeadlineRequest(requirementId, author, deadline);
        }
        else
        {
            messageService.SendMessage(requirementId, author, text);
        }

        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult AdaptDeadline(Guid dealId, Guid requirementId, Guid messageId)
    {
        DealResponseDto? deal = dealService.GetById(dealId);
        RequirementResponseDto? requirement = requirementService.GetRequirementById(requirementId);
        MessageResponseDto? message = messageService.GetMessageById(messageId);
        if (deal is null || requirement is null || requirement.DealId != dealId || message is null || message.RequirementId != requirementId)
            return NotFound();
        if (message.RequestedDeadline is not DateOnly deadline || message.DeadlineAdapted
            || deal.Status == "Completed")
        {
            TempData["ChatError"] = "This deadline request can no longer be adapted.";
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        dealService.UpdateDealDetails(dealId, deal.Title, deal.Customer, deal.Description, deadline);
        requirementService.UpdateDeadline(requirementId, deadline);
        messageService.AdaptDeadlineRequest(messageId);
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Developer")]
    [ValidateAntiForgeryToken]
    public IActionResult AddModule(Guid dealId, Guid requirementId, List<Guid> moduleIds)
    {
        RequirementResponseDto? requirement = requirementService.GetRequirementById(requirementId);
        if (dealService.GetById(dealId) is null || requirement is null || requirement.DealId != dealId)
            return NotFound();

        if (!ModelState.IsValid || moduleIds.Count == 0 || moduleIds.Any(id => moduleRepository.GetById(id) is null))
        {
            TempData["RequirementModuleError"] = "Select one or more valid modules from the module library.";
            return RedirectToAction("Index", new { dealId, requirementId });
        }

        requirementService.UpdateModules(requirementId, requirement.Modules.Select(module => module.Id).Concat(moduleIds));
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Developer")]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveModule(Guid dealId, Guid requirementId, Guid moduleId)
    {
        RequirementResponseDto? requirement = requirementService.GetRequirementById(requirementId);
        if (dealService.GetById(dealId) is null || requirement is null || requirement.DealId != dealId)
            return NotFound();
        if (!requirement.Modules.Any(module => module.Id == moduleId))
            return NotFound();

        requirementService.UpdateModules(requirementId, requirement.Modules.Where(module => module.Id != moduleId).Select(module => module.Id));
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

        dealService.Create(title.Trim(), customer.Trim(), description.Trim(), EstimatedDeadline);
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

        dealService.UpdateDealDetails(dealId, title.Trim(), customer.Trim(), description.Trim(), EstimatedDeadline);
        return RedirectToAction("Index", new { dealId, requirementId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteDeal(Guid dealId)
    {
        dealService.DeleteDeal(dealId);
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
        if (dealService.GetById(dealId) is null)
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

        requirementService.CreateRequirement(dealId, title.Trim(), description.Trim(), requirementType, requirementPriority);
        return RedirectToAction("Index", new { dealId });
    }

    [HttpPost]
    [Authorize(Roles = "Sales")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteRequirement(Guid dealId, Guid requirementId)
    {
        if (dealService.GetById(dealId) is null)
        {
            return NotFound();
        }

        if (requirementService.GetRequirementById(requirementId) is null)
        {
            return NotFound();
        }

        requirementService.DeleteRequirement(requirementId);
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
        if (dealService.GetById(dealId) is null)
        {
            return NotFound();
        }

        RequirementResponseDto? requirement = requirementService.GetRequirementById(requirementId);

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

        requirementService.UpdateRequirement(requirementId, title.Trim(), description.Trim(), requirementType, requirementPriority);
        return RedirectToAction("Index", new { dealId, requirementId });
    }
}
