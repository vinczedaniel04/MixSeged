var builder = WebApplication.CreateBuilder(args); // 1. Itt hozzuk létre

// 2. Itt adjuk hozzá a szolgáltatásokat (CORS, Controller stb.)
builder.Services.AddControllers();
builder.Services.AddCors(options => {
    options.AddPolicy("EngeddKozel", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build(); // 3. Itt ZÁRJUK LE a buildert
app.UseCors("EngeddKozel"); // <--- Ennek ott kell lennie!

// 1. ÖSSZES ELÉRHETŐ MECCS (Ma és a közeljövőben)
app.MapGet("/api/meccsek", async (HttpClient client, IConfiguration config) =>
{
    var apiKey = config["FootballDataToken"];
    var request = new HttpRequestMessage {
        Method = HttpMethod.Get,
        RequestUri = new Uri("https://api.football-data.org/v4/matches"),
        Headers = { { "X-Auth-Token", apiKey } }
    };
    var response = await client.SendAsync(request);
    var tartalom = await response.Content.ReadAsStringAsync();
    return Results.Content(tartalom, "application/json");
});

// 2. CSAPAT STATISZTIKA AZ ELEMZÉSHEZ
app.MapGet("/api/statisztika/{csapatId}", async (int csapatId, HttpClient client, IConfiguration config) =>
{
    var apiKey = config["FootballDataToken"];
    var request = new HttpRequestMessage {
        Method = HttpMethod.Get,
        RequestUri = new Uri($"https://api.football-data.org/v4/teams/{csapatId}/matches?status=FINISHED"),
        Headers = { { "X-Auth-Token", apiKey } }
    };
    var response = await client.SendAsync(request);
    var tartalom = await response.Content.ReadAsStringAsync();
    return Results.Content(tartalom, "application/json");
});

app.Run();