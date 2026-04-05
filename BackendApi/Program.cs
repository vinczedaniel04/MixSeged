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

app.MapGet("/", () => "A MixSeged szervere elindult - The Odds API-val!");

app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try 
    {
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        // Az új Odds API kulcs beolvasása a Render-ről
        var token = config["OddsApiKey"];
        
        if (string.IsNullOrEmpty(token)) 
            return Results.Problem("Hiba: Az OddsApiKey hianyzik a Renderrol!");

        // Lekérjük a Premier League (soccer_epl) KÖZELGŐ meccseit és a H2H (1X2) oddsokat európai irodáktól
        string url = $"https://api.the-odds-api.com/v4/sports/soccer_epl/odds/?apiKey={token}&regions=eu&markets=h2h";
        
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