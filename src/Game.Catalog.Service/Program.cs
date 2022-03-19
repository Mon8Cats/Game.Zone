using Game.Catalog.Service.Entities;
using Game.Catalog.Service.Settings;
using Game.Common.MassTransit;
using Game.Common.MongoDB;
using Game.Common.Settings;
using MassTransit;
using MassTransit.Definition;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
const string AllowedOriginSetting = "AllowedOrigin";

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items")
    .AddMassTransitWithRabbitMq();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
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
