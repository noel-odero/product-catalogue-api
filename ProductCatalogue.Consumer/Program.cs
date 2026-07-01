using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalogue.Consumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaConsumerSettings>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<NotificationConsumerService>();

var host = builder.Build();
host.Run();