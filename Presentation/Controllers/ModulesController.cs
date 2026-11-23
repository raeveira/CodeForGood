using Application;
using Application.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;

[Authorize(Roles = "Developer")]
[Route("modules")]
public class ModulesController(IModuleRepository repository, IRequirementRepository requirementRepository) : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View(new ModulesViewModel { Modules = repository.GetAll() });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind(Prefix = "Form")] ModuleInput input)
    {
        if (!ModelState.IsValid)
            return View("Index", Page(input));

        repository.Add(new Module(Guid.NewGuid(), input.Name!.Trim(), input.Complexity!.Value));
        TempData["ModuleMessage"] = "Module created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid id, [Bind(Prefix = "Form")] ModuleInput input)
    {
        if (repository.GetById(id) is null)
            return NotFound();

        if (!ModelState.IsValid)
            return View("Index", Page(input, id));

        if (!repository.Update(new Module(id, input.Name!.Trim(), input.Complexity!.Value)))
            return NotFound();

        TempData["ModuleMessage"] = "Module updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        if (repository.GetById(id) is null)
            return NotFound();
        if (requirementRepository.GetAll().Any(requirement => requirement.ModuleIds.Contains(id)))
        {
            TempData["ModuleError"] = "This module is assigned to a requirement. Remove it from all requirements before deleting it.";
            return RedirectToAction(nameof(Index));
        }

        if (!repository.Remove(id))
            return NotFound();

        TempData["ModuleMessage"] = "Module deleted.";
        return RedirectToAction(nameof(Index));
    }

    private ModulesViewModel Page(ModuleInput input, Guid? id = null) => new()
    {
        Modules = repository.GetAll(),
        Form = input,
        EditingId = id,
        ShowModal = true
    };
}
