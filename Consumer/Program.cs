// See https://aka.ms/new-console-template for more information

using Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication2.Services;


var host = CreateDefaultBuilder().Build();

// Invoke Worker
using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
var workerInstance = provider.GetRequiredService<Worker>();
workerInstance.DoWork();

host.Run();
static IHostBuilder CreateDefaultBuilder()
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddSingleton<Worker>();
            services.AddScoped<IMQService,MQService>();
        });
}

