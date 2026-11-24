using Application;
using Application.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;

[Authorize(Roles = "Developer")]
[Route("modules")]
public class ModulesController(ModuleService moduleService) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        IReadOnlyList<Module> modules = moduleService.GetAll();
        return View(new ModulesViewModel { Modules = modules });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind(Prefix = "Form")] ModuleInput input)
    {
        if (!ModelState.IsValid)
            return View("Index", Page(input));

        moduleService.Add(input.Name!.Trim(), input.Complexity!.Value);
        TempData["ModuleMessage"] = "Module created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid id, [Bind(Prefix = "Form")] ModuleInput input)
    {
        if (moduleService.GetById(id) is null)
            return NotFound();

        if (!ModelState.IsValid)
            return View("Index", Page(input, id));

        moduleService.Update(id, input.Name!.Trim(), input.Complexity!.Value);

        TempData["ModuleMessage"] = "Module updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        if (moduleService.GetById(id) is null)
            return NotFound();
        try
        {
            moduleService.Delete(id);
        }
        catch (Exception ex)
        {
            TempData["ModuleError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ModuleMessage"] = "Module deleted.";
        return RedirectToAction(nameof(Index));
    }

    private ModulesViewModel Page(ModuleInput input, Guid? id = null) => new()
    {
        Modules = moduleService.GetAll(),
        Form = input,
        EditingId = id,
        ShowModal = true
    };
}
