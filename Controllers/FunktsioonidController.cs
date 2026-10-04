using funktsioonide_uurimise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace funktsioonide_uurimise.Controllers;

public class FunktsioonidController : Controller
{
    private readonly AndmeKontekst _kontekst;

    public FunktsioonidController(AndmeKontekst kontekst) => _kontekst = kontekst;

    public async Task<IActionResult> Index()
    {
        var nimekiri = await _kontekst.FunktsiooniUurimised
            .OrderByDescending(f => f.LuodudAeg)
            .ToListAsync();
        return View(nimekiri);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(olem.Maaramispiirkond))
            ViewBag.Lahendus = LahendusTeenus.Koosta(olem);

        return View(olem);
    }
    public IActionResult Create() => View(new FunktsiooniUurimine());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Valem")] FunktsiooniUurimine olem)
    {
        Kontrolli(olem);
        if (!ModelState.IsValid) return View(olem);

        Arvuta(olem);
        await _kontekst.FunktsiooniUurimised.AddAsync(olem);
        await _kontekst.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = olem.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        return olem == null ? NotFound() : View(olem);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Valem")] FunktsiooniUurimine vorm)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem == null) return NotFound();

        Kontrolli(vorm);
        if (!ModelState.IsValid)
        {
            vorm.Id = id;
            return View(vorm);
        }

        Arvuta(vorm);
        olem.Valem = vorm.Valem;
        olem.Maaramispiirkond = vorm.Maaramispiirkond;
        olem.Muutumispiirkond = vorm.Muutumispiirkond;
        olem.Tuletis = vorm.Tuletis;
        olem.Nullkohad = vorm.Nullkohad;
        olem.Positiivsus = vorm.Positiivsus;
        olem.Negatiivsus = vorm.Negatiivsus;
        olem.Kasvamine = vorm.Kasvamine;
        olem.Kahanemine = vorm.Kahanemine;
        olem.Ekstreemumid = vorm.Ekstreemumid;
        olem.Nogusus = vorm.Nogusus;
        olem.Kumerus = vorm.Kumerus;
        olem.Kaanupunktid = vorm.Kaanupunktid;
        olem.Paarsus = vorm.Paarsus;
        olem.Periood = vorm.Periood;
        await _kontekst.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        return olem == null ? NotFound() : View(olem);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> KinnitaKustutamine(int id)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem != null)
        {
            _kontekst.FunktsiooniUurimised.Remove(olem);
            await _kontekst.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // JSON graafiku jaoks
    [HttpGet]
    public IActionResult Punktid(string? valem, double alates = -5, double kuni = 5, double samm = 0.1)
    {
        if (string.IsNullOrWhiteSpace(valem) || samm < 0.001 || kuni <= alates || (kuni - alates) / samm > 5000)
            return BadRequest("Vigane valem või vahemik");

        var x = new List<double>();
        var f = new List<double?>();
        var d = new List<double?>();
        int samme = (int)Math.Round((kuni - alates) / samm);

        for (int i = 0; i <= samme; i++)
        {
            double xv = Math.Round(alates + i * samm, 6);
            double fv = ValemiTeenus.F(valem, xv);
            double dv = ValemiTeenus.D(valem, xv);
            x.Add(xv);
            f.Add(double.IsNaN(fv) ? null : fv);
            d.Add(double.IsNaN(dv) ? null : dv);
        }

        var ekstreemid = ValemiTeenus.LeiaEkstreemid(valem, alates, kuni)
            .Select(e => new { x = e.X, y = e.Y, max = e.Max })
            .ToList();

        return Json(new { x, f, d, ekstreemid });
    }

    private void Kontrolli(FunktsiooniUurimine olem)
    {
        if (!string.IsNullOrWhiteSpace(olem.Valem) &&
            new[] { 0.5, 1.5, 2.5, -0.5 }.All(x => double.IsNaN(ValemiTeenus.F(olem.Valem, x))))
        {
            ModelState.AddModelError(nameof(olem.Valem), "Valemit ei saa arvutada. Näide: x^3 - 3*x");
        }
    }

    // Kõik väljad arvutatakse valemi põhjal automaatselt
    private static void Arvuta(FunktsiooniUurimine olem)
    {
        olem.Valem = olem.Valem.Trim();

        // 1-2
        olem.Maaramispiirkond = ValemiTeenus.Maaramispiirkond(olem.Valem);
        olem.Muutumispiirkond = ValemiTeenus.Muutumispiirkond(olem.Valem);
        olem.Tuletis = SumboolneTeenus.Tuletis(olem.Valem);

        // 3-4
        var (nullid, pos, neg) = ValemiTeenus.Margiuuring(olem.Valem);
        olem.Nullkohad = nullid;
        olem.Positiivsus = pos;
        olem.Negatiivsus = neg;

        // 5
        var (kasv, kah) = ValemiTeenus.Monotoonsus(olem.Valem);
        olem.Kasvamine = kasv;
        olem.Kahanemine = kah;

        // 6
        olem.Ekstreemumid = ValemiTeenus.Analuusi(olem.Valem);

        // 7
        var (nogus, kumer, kaanu) = ValemiTeenus.Kumerus(olem.Valem);
        olem.Nogusus = nogus;
        olem.Kumerus = kumer;
        olem.Kaanupunktid = kaanu;

        // 8-9
        olem.Paarsus = ValemiTeenus.Paarsus(olem.Valem);
        olem.Periood = ValemiTeenus.Periood(olem.Valem);
    }
}