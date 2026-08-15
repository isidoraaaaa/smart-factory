using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Infrastructure.Hubs;
using SmartFactoryBackend.Infrastructure.Persistence;
using SmartFactoryBackend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    }); ;

// Background service koji generiše telemetriju
builder.Services.AddHostedService<TelemetrySimulatorService>();

// CORS — da React (Vite) klijent na drugom portu može da se konektuje
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // obavezno za SignalR
    });
});
builder.Services.AddDbContext<SmartFactoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowClient"); // mora pre UseAuthorization i pre mapiranja huba

app.UseAuthorization();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry"); // ruta na koju se React konektuje

app.Run();
