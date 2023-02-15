using Domain;
using CrossCutting;
using Microsoft.Extensions.Hosting;
using Serverless_Api.Middlewares;
using Application;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddEventStore();
        services.AddDomainDependencies();

        services.AddPersonService();
        services.AddBbqService();
    })
    .ConfigureFunctionsWorkerDefaults(builder => builder.UseMiddleware<AuthMiddleware>())
    .Build();

host.Run();
