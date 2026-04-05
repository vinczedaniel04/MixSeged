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

// MECCSEK LEKÉRÉSE
app.MapGet("/api/meccsek", async ([FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try 
    {
        var client = clientFactory.CreateClient();
        var token = config["FootballDataToken"];
        
        if (string.IsNullOrEmpty(token)) return Results.Text("HIBA: Nincs beallitva a Token a Renderen!", statusCode: 500);

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Auth-Token", token);
        
        // Először próbáljuk meg csak a főbb ligákat, az biztosabb és gyorsabb
       var response = await client.GetAsync("https://api.football-data.org/v4/matches?competitions=CL,PL,PD,BL1,SA,FL1");
        
        if (!response.IsSuccessStatusCode)
        {
            var hibaSzoveg = await response.Content.ReadAsStringAsync();
            return Results.Text($"API HIBA ({response.StatusCode}): {hibaSzoveg}", statusCode: 500);
        }

        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Text($"SZERVER HIBA: {ex.Message}", statusCode: 500);
    }
});

// STATISZTIKA
app.MapGet("/api/statisztika/{id}", async (int id, [FromServices] IHttpClientFactory clientFactory, [FromServices] IConfiguration config) =>
{
    try {
        var client = clientFactory.CreateClient();
        var token = config["FootballDataToken"];
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Auth-Token", token);
        
        var response = await client.GetAsync($"https://api.football-data.org/v4/teams/{id}/matches?status=FINISHED");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    } catch (Exception ex) {
        return Results.Text(ex.Message, statusCode: 500);
    }
});

app.Run();