var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => {
    options.AddPolicy("EngeddKozel", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();
app.UseCors("EngeddKozel");

// EZ A TESZT: Nem kell hozzá API, azonnal válaszolnia kell!
app.MapGet("/api/teszt", () => new { uzenet = "A szervered elesz, a hiba az API-nal van!", ido = DateTime.Now });

app.MapGet("/api/meccsek", () => new { hiba = "Ideiglenesen szunetel, hasznald a /api/teszt-et!" });

app.Run();