using funktsioonide_uurimise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace funktsioonide_uurimise.Controllers;

public class FunktsioonidController : Controller
{
    private readonly AndmeKontekst _kontekst;

    public FunktsioonidController(AndmeKontekst kontekst)
    {
        _kontekst = kontekst;
    }

    public async Task<IActionResult> Index()
    {
        var nimekiri = await _kontekst.FunktsiooniUurimised
            .OrderByDescending(f => f.LuodudAeg)
            .ToListAsync();

        return View(nimekiri);
    }

    public async Task<IActionResult> History()
    {
        var nimekiri = await _kontekst.FunktsiooniUurimised
            .OrderByDescending(f => f.LuodudAeg)
            .ToListAsync();

        return View(nimekiri);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);

        if (olem == null)
        {
            return NotFound();
        }

        if (OnTulemusi(olem))
        {
            TaastaValikud(olem);

            ViewBag.Lahendus = LahendusTeenus.Koosta(olem);
        }

        return View(olem);
    }

    public IActionResult Create()
    {
        return View(new FunktsiooniUurimine());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Valem,Valikud")] FunktsiooniUurimine olem)
    {
        Kontrolli(olem);

        if (!ModelState.IsValid)
        {
            return View(olem);
        }

        Arvuta(olem);

        await _kontekst.FunktsiooniUurimised.AddAsync(olem);
        await _kontekst.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = olem.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);

        if (olem == null)
        {
            return NotFound();
        }

        // Näitame olemasolevad tulemused valitud analüüsidena
        TaastaValikud(olem);

        return View(olem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Valem,Valikud")] FunktsiooniUurimine vorm)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);

        if (olem == null)
        {
            return NotFound();
        }

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

        return RedirectToAction(nameof(Details), new { id = id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);

        if (olem == null)
        {
            return NotFound();
        }

        return View(olem);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KinnitaKustutamine(int id)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);

        if (olem != null)
        {
            _kontekst.FunktsiooniUurimised.Remove(olem);
            await _kontekst.SaveChangesAsync();
        }

        return RedirectToAction(nameof(History));
    }

    // JSON graafiku jaoks
    [HttpGet]
    public IActionResult Punktid(
        string? valem,
        double alates = -5,
        double kuni = 5,
        double samm = 0.1)
    {
        if (string.IsNullOrWhiteSpace(valem) ||
            samm < 0.001 ||
            kuni <= alates ||
            (kuni - alates) / samm > 5000)
        {
            return BadRequest("Vigane valem või vahemik");
        }

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

            if (double.IsNaN(fv))
            {
                f.Add(null);
            }
            else
            {
                f.Add(fv);
            }

            if (double.IsNaN(dv))
            {
                d.Add(null);
            }
            else
            {
                d.Add(dv);
            }
        }

        var ekstreemid = ValemiTeenus.LeiaEkstreemid(valem, alates, kuni)
            .Select(e => new
            {
                x = e.X,
                y = e.Y,
                max = e.Max
            })
            .ToList();

        return Json(new
        {
            x,
            f,
            d,
            ekstreemid
        });
    }

    private void Kontrolli(FunktsiooniUurimine olem)
    {
        if (string.IsNullOrWhiteSpace(olem.Valem))
        {
            return;
        }

        bool valemKehtib = new[] { 0.5, 1.5, 2.5, -0.5 }
            .All(x => !double.IsNaN(ValemiTeenus.F(olem.Valem, x)));

        if (!valemKehtib)
        {
            ModelState.AddModelError(
                nameof(olem.Valem),
                "Valemit ei saa arvutada. Näide: x^3 - 3*x");
        }

        if (olem.Valikud == null || olem.Valikud.Count == 0)
        {
            ModelState.AddModelError(
                nameof(olem.Valikud),
                "Vali vähemalt üks uuring.");
        }
    }

    private static void Arvuta(FunktsiooniUurimine olem)
    {
        olem.Valem = olem.Valem.Trim();

        PuhastaTulemused(olem);

        // Määramispiirkond
        if (olem.Valikud.Contains("Maaramispiirkond"))
        {
            olem.Maaramispiirkond =
                ValemiTeenus.Maaramispiirkond(olem.Valem);
        }

        // Muutumispiirkond
        if (olem.Valikud.Contains("Muutumispiirkond"))
        {
            olem.Muutumispiirkond =
                ValemiTeenus.Muutumispiirkond(olem.Valem);
        }

        // Tuletis
        // Tuletist on vaja ka mõnede teiste analüüside jaoks.
        if (olem.Valikud.Contains("Tuletis") ||
            olem.Valikud.Contains("Monotoonsus") ||
            olem.Valikud.Contains("Ekstreemumid") ||
            olem.Valikud.Contains("Kumerus"))
        {
            olem.Tuletis =
                SumboolneTeenus.Tuletis(olem.Valem);
        }

        // Nullkohad ja positiivsus/negatiivsus
        if (olem.Valikud.Contains("Nullkohad") ||
            olem.Valikud.Contains("Margiuuring"))
        {
            var margid = ValemiTeenus.Margiuuring(olem.Valem);

            if (olem.Valikud.Contains("Nullkohad"))
            {
                olem.Nullkohad = margid.Nullid;
            }

            if (olem.Valikud.Contains("Margiuuring"))
            {
                olem.Positiivsus = margid.Positiivsed;
                olem.Negatiivsus = margid.Negatiivsed;
            }
        }

        // Kasvamine ja kahanemine
        if (olem.Valikud.Contains("Monotoonsus"))
        {
            var monotoonsus =
                ValemiTeenus.Monotoonsus(olem.Valem);

            olem.Kasvamine = monotoonsus.Kasvab;
            olem.Kahanemine = monotoonsus.Kahaneb;
        }

        // Ekstreemumid
        if (olem.Valikud.Contains("Ekstreemumid"))
        {
            olem.Ekstreemumid =
                ValemiTeenus.Analuusi(olem.Valem);
        }

        // Nõgusus, kumerus ja käänupunktid
        if (olem.Valikud.Contains("Kumerus"))
        {
            var kumerus =
                ValemiTeenus.Kumerus(olem.Valem);

            olem.Nogusus = kumerus.Nogus;
            olem.Kumerus = kumerus.Kumer;
            olem.Kaanupunktid = kumerus.Kaanu;
        }

        // Paaris / paaritu
        if (olem.Valikud.Contains("Paarsus"))
        {
            olem.Paarsus =
                ValemiTeenus.Paarsus(olem.Valem);
        }

        // Periood
        if (olem.Valikud.Contains("Periood"))
        {
            olem.Periood =
                ValemiTeenus.Periood(olem.Valem);
        }
    }

    private static void PuhastaTulemused(FunktsiooniUurimine olem)
    {
        olem.Maaramispiirkond = string.Empty;
        olem.Muutumispiirkond = string.Empty;
        olem.Tuletis = string.Empty;
        olem.Nullkohad = string.Empty;
        olem.Positiivsus = string.Empty;
        olem.Negatiivsus = string.Empty;
        olem.Kasvamine = string.Empty;
        olem.Kahanemine = string.Empty;
        olem.Ekstreemumid = string.Empty;
        olem.Nogusus = string.Empty;
        olem.Kumerus = string.Empty;
        olem.Kaanupunktid = string.Empty;
        olem.Paarsus = string.Empty;
        olem.Periood = string.Empty;
    }

    private static bool OnTulemusi(FunktsiooniUurimine olem)
    {
        if (!string.IsNullOrWhiteSpace(olem.Maaramispiirkond))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Muutumispiirkond))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Tuletis))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Nullkohad))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Positiivsus))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Negatiivsus))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Kasvamine))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Kahanemine))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Ekstreemumid))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Nogusus))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Kumerus))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Kaanupunktid))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Paarsus))
            return true;

        if (!string.IsNullOrWhiteSpace(olem.Periood))
            return true;

        return false;
    }

    private static void TaastaValikud(FunktsiooniUurimine olem)
    {
        olem.Valikud = new List<string>();

        if (!string.IsNullOrWhiteSpace(olem.Maaramispiirkond))
            olem.Valikud.Add("Maaramispiirkond");

        if (!string.IsNullOrWhiteSpace(olem.Muutumispiirkond))
            olem.Valikud.Add("Muutumispiirkond");

        if (!string.IsNullOrWhiteSpace(olem.Nullkohad))
            olem.Valikud.Add("Nullkohad");

        if (!string.IsNullOrWhiteSpace(olem.Positiivsus) ||
            !string.IsNullOrWhiteSpace(olem.Negatiivsus))
        {
            olem.Valikud.Add("Margiuuring");
        }

        if (!string.IsNullOrWhiteSpace(olem.Kasvamine) ||
            !string.IsNullOrWhiteSpace(olem.Kahanemine))
        {
            olem.Valikud.Add("Monotoonsus");
        }

        if (!string.IsNullOrWhiteSpace(olem.Ekstreemumid))
            olem.Valikud.Add("Ekstreemumid");

        if (!string.IsNullOrWhiteSpace(olem.Nogusus) ||
            !string.IsNullOrWhiteSpace(olem.Kumerus) ||
            !string.IsNullOrWhiteSpace(olem.Kaanupunktid))
        {
            olem.Valikud.Add("Kumerus");
        }

        if (!string.IsNullOrWhiteSpace(olem.Paarsus))
            olem.Valikud.Add("Paarsus");

        if (!string.IsNullOrWhiteSpace(olem.Periood))
            olem.Valikud.Add("Periood");
    }
}