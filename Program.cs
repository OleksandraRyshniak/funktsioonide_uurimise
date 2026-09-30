using funktsioonide_uurimise.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
    o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddDbContext<AndmeKontekst>(static o =>
    o.UseSqlite("Data Source=funktsioonid.db"));

var app = builder.Build();

using (var ulatus = app.Services.CreateScope())
{
    var kontekst = ulatus.ServiceProvider.GetRequiredService<AndmeKontekst>();
    kontekst.Database.Migrate();

    if (!kontekst.FunktsiooniUurimised.Any())
    {
        kontekst.FunktsiooniUurimised.AddRange(
            new FunktsiooniUurimine
            {
                Valem = "x^3 - 3*x",
                Maaramispiirkond = "X: (-∞, ∞)",
                Tuletis = "3*x^2 - 3",
               
                Ekstreemumid = "max = 2 (x = -1); min = -2 (x = 1)"
            },
            new FunktsiooniUurimine
            {
                Valem = "x^2 - 4*x + 3",
                Maaramispiirkond = "X: (-∞, ∞)",
                Tuletis = "2*x - 4",
                
                Ekstreemumid = "min = -1 (x = 2)"
            },
            new FunktsiooniUurimine
            {
                Valem = "x^3 - 12*x",
                Maaramispiirkond = "X: (-∞, ∞)",
                Tuletis = "3*x^2 - 12",
                
                Ekstreemumid = "max = 16 (x = -2); min = -16 (x = 2)"
            });
        kontekst.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Funktsioonid}/{action=Index}/{id?}");

app.Run();