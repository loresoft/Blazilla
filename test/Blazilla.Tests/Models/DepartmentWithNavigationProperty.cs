using BlazorShared.Models;

namespace Blazilla.Tests.Models;

public record DepartmentWithNavigationProperty :
    Department
{
    public Employee? Manager { get; set; }
}
