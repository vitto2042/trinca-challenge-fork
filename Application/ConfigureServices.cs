using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddPersonService(this IServiceCollection services)
        {
            services.AddTransient<IPersonService, PersonService>();

            return services;
        }

        public static IServiceCollection AddBbqService(this IServiceCollection services)
        {
            services.AddTransient<IBbqService, BbqService>();

            return services;
        }
    }
}
