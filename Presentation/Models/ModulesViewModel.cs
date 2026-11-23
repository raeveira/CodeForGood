using System.ComponentModel.DataAnnotations;
using Application.Modules;

namespace Presentation.Models;

public sealed class ModuleInput
{
    [Required(ErrorMessage = "Enter a module name.")]
    [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Choose an integration complexity.")]
    [EnumDataType(typeof(EIntegrationComplexity), ErrorMessage = "Choose a valid integration complexity.")]
    public EIntegrationComplexity? Complexity { get; set; }
}

public sealed class ModulesViewModel
{
    public IReadOnlyList<Module> Modules { get; init; } = [];
    public ModuleInput Form { get; init; } = new();
    public Guid? EditingId { get; init; }
    public bool ShowModal { get; init; }
}
