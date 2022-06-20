using Hangfire;
using HangfireService;
using HangfireService.Services;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostcontext,services) =>
    {
        services.AddHangfire(conf =>
        {
            conf.UseSqlServerStorage(hostcontext.Configuration.GetConnectionString("Default"));
        });
        services.AddHangfireServer();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(hostcontext.Configuration.GetConnectionString("Default"));
        },ServiceLifetime.Singleton);
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
