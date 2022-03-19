using Game.Common.MassTransit;
using Game.Common.MongoDB;
using Game.Inventory.Service.Clients;
using Game.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
const string AllowedOriginSetting = "AllowedOrigin";

builder.Services
    .AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryItems")
    .AddMongoRepository<CatalogItem>("catalogItems")
    .AddMassTransitWithRabbitMq();

AddCatalogClient(builder);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(policyBuilder => 
    {
        policyBuilder.WithOrigins(builder.Configuration[AllowedOriginSetting])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(WebApplicationBuilder builder)
{
    Random jitter = new Random();

    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7001");

    })
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            //ILog class
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");

        }
    ))
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
        },
        onReset: () =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Closing the circuit...");
        }
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1)); // 1 sec timeout policy
}
