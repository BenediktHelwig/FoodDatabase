var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddLogging();

// Services werden später hinzugefügt:
// builder.Services.AddScoped<ILebensmittelService, LebensmittelService>();
// builder.Services.AddScoped<IRecipeService, RecipeService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Minimal API endpoint
app.MapGet("/", () => Results.Json(new { status = "FoodDatabase API ready" }));

app.Run();
