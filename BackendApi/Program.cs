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

// ALAP TESZT - Ezt látod a főoldalon
app.MapGet("/", () => "A MixSeged szervere elindult!");

// MECCSEK LEKÉRÉSE - Szűrt változat a gyorsaságért
app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try 
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        // A Renderen beállított környezeti változót olvassa ki
        var token = config["FootballDataToken"];
        
        if (string.IsNullOrEmpty(token)) 
            return Results.Problem("Hiba: Az API kulcs (Token) hianyzik a Renderrol!");

        client.DefaultRequestHeaders.Add("X-Auth-Token", token);
        
        // Csak az Angol bajnokság (PL) - ez a legstabilabb
        var response = await client.GetAsync("https://api.football-data.org/v4/competitions/PL/matches?status=SCHEDULED");
        
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