using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// CORS engedélyezése a frontend felé
builder.Services.AddCors(options => {
    options.AddPolicy("EngeddKozel", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();
var app = builder.Build();
app.UseCors("EngeddKozel");

// Alapértelmezett útvonal ellenőrzéshez
app.MapGet("/", () => "MixSeged Pro Backend - Top 5 Liga & Kredit Figyelo Aktiv!");

app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try 
    {
        var client = clientFactory.CreateClient();
        var token = config["OddsApiKey"];
        
        if (string.IsNullOrEmpty(token)) {
            return Results.Problem("Hiba: Az OddsApiKey nincs beallitva a Render környezeti valtozoi kozott!");
        }

        // Top 5 Liga - Takarékos üzemmód (5 kredit / frissítés)
        string[] ligak = {
            "soccer_epl",                // Angol Premier League
            "soccer_spain_la_liga",      // Spanyol La Liga
            "soccer_germany_bundesliga", // Német Bundesliga
            "soccer_italy_serie_a",      // Olasz Serie A
            "soccer_france_ligue_1"      // Francia Ligue 1
        };

        var mindenMeccs = new List<object>();

        foreach (var liga in ligak)
        {
            // Lekérés decimal formátumban, EU irodákkal
            string url = $"https://api.the-odds-api.com/v4/sports/{liga}/odds/?apiKey={token}&regions=eu&markets=h2h&oddsFormat=decimal";
            
            var response = await client.GetAsync(url);
            
            // --- KREDIT FIGYELŐ ---
            // Kiolvassuk a maradék lekérések számát a válasz fejlécéből
            if (response.Headers.TryGetValues("x-requests-remaining", out var values))
            {
                var maradek = values.FirstOrDefault();
                Console.WriteLine($"[{liga}] Lekeres sikeres. Hatralevo kreditek: {maradek}");
            }

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var meccsek = JsonSerializer.Deserialize<List<object>>(content);
                if (meccsek != null) mindenMeccs.AddRange(meccsek);
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine("!!! HIBA: Elfogyott a havi limited az Odds API-nal (429) !!!");
                return Results.Json(new { error = "Limit elerve", message = "Elfogyott a havi lekérési kereted." }, statusCode: 429);
            }
        }

        return Results.Ok(mindenMeccs);
    }
    catch (Exception ex) 
    {
        Console.WriteLine($"Sulyos hiba: {ex.Message}");
        return Results.Problem(ex.Message); 
    }
});

app.Run();