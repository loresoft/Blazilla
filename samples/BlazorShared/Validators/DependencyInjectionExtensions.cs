using BlazorShared.Models;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorShared.Validators;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSharedValidators(this IServiceCollection services)
    {
        services.TryAddSingleton<IValidator<Address>, AddressValidator>();
        services.TryAddSingleton<IValidator<Company>, CompanyValidator>();
        services.TryAddSingleton<IValidator<CompanySettings>, CompanySettingsValidator>();
        services.TryAddSingleton<IValidator<Department>, DepartmentValidator>();
        services.TryAddSingleton<IValidator<Employee>, EmployeeValidator>();
        services.TryAddSingleton<IValidator<Person>, PersonValidator>();
        services.TryAddSingleton<IValidator<Project>, ProjectValidator>();
        services.TryAddSingleton<IValidator<ProjectMilestone>, ProjectMilestoneValidator>();

        return services;
    }
}
