using Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class ServicesContainer
{
    public static IServiceCollection AddApplicationService(this IServiceCollection service)
    {
        service.AddScoped<IAccountService, AccountService>();
        return service;
    }
}