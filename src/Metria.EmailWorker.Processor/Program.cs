using Microsoft.EntityFrameworkCore;
using Metria.EmailWorker.Infrastructure.Persistence;
using Metria.EmailWorker.Processor.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddEmailWorker(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EmailWorkerDbContext>();
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();
