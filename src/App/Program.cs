using FoodDatabase.App.Components;
using FoodDatabase.App.Data;
using FoodDatabase.App.Data.Repositories;
using FoodDatabase.App.Services.Classes;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddLogging();

// Entity Framework Core DbContext
var connectionString = builder.Configuration.GetConnectionString("FoodDatabaseConnection");
builder.Services.AddDbContext<FoodDatabaseContext>(options =>
    options.UseSqlite(connectionString));

// Razor Components + Interactive Server
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Generic Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// TimeProvider für UC7 (VerfallsdatumWarnungen)
builder.Services.AddSingleton(TimeProvider.System);

// Service Registrierungen (13 Services)
builder.Services.AddScoped<ILebensmittelService, LebensmittelService>();
builder.Services.AddScoped<ILagerbestandService, LagerbestandService>();
builder.Services.AddScoped<INährwertService, NährwertService>();
builder.Services.AddScoped<IRezeptService, RezeptService>();
builder.Services.AddScoped<IRezeptZutatService, RezeptZutatService>();
builder.Services.AddScoped<IEinheitConverter, EinheitConverter>();
builder.Services.AddScoped<INährwertCalculator, NährwertCalculator>();
builder.Services.AddScoped<IRezeptNährwertService, RezeptNährwertService>();
builder.Services.AddScoped<IVerbrauchAusbuchangService, VerbrauchAusbuchangService>();
builder.Services.AddScoped<IVerfallsdatumWarnungService, VerfallsdatumWarnungService>();
builder.Services.AddScoped<IEinkaufslistenService, EinkaufslistenService>();
builder.Services.AddScoped<ILagerortService, LagerortService>();
builder.Services.AddScoped<IProduktInstanzService, ProduktInstanzService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Razor Components routing
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Health check endpoint
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
