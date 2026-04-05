using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("EngeddKozel", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();
var app = builder.Build();
app.UseCors("EngeddKozel");

app.MapGet("/", () => "MixSeged Pro Backend - Minden liga aktivalva!");

app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try 
    {
        var client = clientFactory.CreateClient();
        var token = config["OddsApiKey"];
        
        // A Te kívánságlistád:
        string[] ligak = {
            "soccer_epl", 
            "soccer_spain_la_liga", 
            "soccer_germany_bundesliga", 
            "soccer_italy_serie_a", 
            "soccer_france_ligue_1",
            "soccer_hungary_nb1",  // 🔥 EZ A MAGYAR NB1!
            "soccer_uefa_champs_league",
            "soccer_uefa_europa_league",
            "soccer_uefa_europa_conference_league",
            "soccer_netherlands_ere_divisie",
            "soccer_portugal_primeira_liga"
        };

        var mindenMeccs = new List<object>();

        foreach (var liga in ligak)
        {
            string url = $"https://api.the-odds-api.com/v4/sports/{liga}/odds/?apiKey={token}&regions=eu&markets=h2h";
            var response = await client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var meccsek = JsonSerializer.Deserialize<List<object>>(content);
                if (meccsek != null) mindenMeccs.AddRange(meccsek);
            }
        }

        return Results.Ok(mindenMeccs);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hiba: {ex.Message}");
    }
});

app.Run();