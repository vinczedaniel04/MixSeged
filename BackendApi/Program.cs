using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS beállítása a Netlify-hoz
builder.Services.AddCors(options =>
{
    options.AddPolicy("EngeddKozel", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Regisztráljuk a HttpClient-et a rendszerbe
builder.Services.AddHttpClient();

var app = builder.Build();

// 2. Middleware-ek bekapcsolása
app.UseCors("EngeddKozel");

// 3. API Végpontok

// MECCSEK LEKÉRÉSE
app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    var client = clientFactory.CreateClient();
    var token = config["FootballDataToken"]; // A Render-en beállított Environment Variable
    
    client.DefaultRequestHeaders.Add("X-Auth-Token", token);
    
    // Példa: Premier League (2021) meccsek, módosítsd ha több ligát akarsz
    var response = await client.GetAsync("https://api.football-data.org/v4/matches");
    var content = await response.Content.ReadAsStringAsync();
    
    return Results.Content(content, "application/json");
});

// STATISZTIKA LEKÉRÉSE (Csapat ID alapján)
app.MapGet("/api/statisztika/{id}", async (int id, [FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    var client = clientFactory.CreateClient();
    var token = config["FootballDataToken"];
    
    client.DefaultRequestHeaders.Add("X-Auth-Token", token);
    
    var response = await client.GetAsync($"https://api.football-data.org/v4/teams/{id}/matches?status=FINISHED");
    var content = await response.Content.ReadAsStringAsync();
    
    return Results.Content(content, "application/json");
});

app.MapControllers();

app.Run();