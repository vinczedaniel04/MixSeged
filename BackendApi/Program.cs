using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("EngeddKozel", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();
var app = builder.Build();
app.UseCors("EngeddKozel");

app.MapGet("/", () => "A MixSeged szervere elindult - API-Football v3!");

app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try 
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        // Az új környezeti változónk beolvasása a Render-ről
        var token = config["ApiFootballToken"];
        
        if (string.IsNullOrEmpty(token)) 
            return Results.Problem("Hiba: Az ApiFootballToken hianyzik a Renderrol!");

        // Az API-Football azonosítása a te kulcsoddal
        client.DefaultRequestHeaders.Add("x-apisports-key", token);
        
        // Lekérjük az Angol Premier League (league=39) következő 10 meccsét (next=10)
        // Mivel 2026-ban vagyunk, a futó szezon a 2025-ös (2025/2026)
        string url = "https://v3.football.api-sports.io/fixtures?league=39&season=2025&next=10";
        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            var hiba = await response.Content.ReadAsStringAsync();
            return Results.Problem($"API Hiba ({response.StatusCode}): {hiba}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Szerver hiba: {ex.Message}");
    }
});

app.Run();