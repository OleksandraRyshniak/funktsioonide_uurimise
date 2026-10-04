using System.Globalization;
using System.Text.RegularExpressions;

namespace funktsioonide_uurimise.Models;

public record LahendusSamm(string Pealkiri, List<string> Read);

public static class LahendusTeenus
{
    private static string N(double v) =>
        (Math.Round(v, 4) + 0.0).ToString(CultureInfo.InvariantCulture).Replace('-', '−');

    private static string G(double v) =>
        double.IsNaN(v) ? "ei ole määratud"
        : (v + 0.0).ToString("G4", CultureInfo.InvariantCulture).Replace('-', '−');

    private static string Ots(double v) =>
        double.IsNegativeInfinity(v) ? "−∞" : double.IsPositiveInfinity(v) ? "∞" : N(v);

    private static string Piir(double v) =>
        Math.Abs(v) > 1e6 ? (v > 0 ? "+∞" : "−∞") : N(v);

    private static bool Parse(string t, out double v)
    {
        t = t.Trim().Replace('−', '-');
        if (t == "∞" || t == "+∞") { v = double.PositiveInfinity; return true; }
        if (t == "-∞") { v = double.NegativeInfinity; return true; }
        return double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    // "(a; b) ∪ (c; d)" -> [(a,b), (c,d)]
    private static List<(double Lo, double Hi)> Vahemikud(string? s)
    {
        var tulemus = new List<(double Lo, double Hi)>();
        if (string.IsNullOrWhiteSpace(s)) return tulemus;
        foreach (Match m in Regex.Matches(s, @"\(([^;()]+);\s*([^;()]+)\)"))
            if (Parse(m.Groups[1].Value, out var a) && Parse(m.Groups[2].Value, out var b))
                tulemus.Add((a, b));
        return tulemus;
    }

    private static List<double> Nullid(string? s)
    {
        var l = new List<double>();
        if (string.IsNullOrWhiteSpace(s)) return l;
        foreach (Match m in Regex.Matches(s, @"x = (\S+)"))
            if (Parse(m.Groups[1].Value, out var x)) l.Add(x);
        return l;
    }

    private static List<(bool Max, double X, double Y)> Ekstreemid(string? s)
    {
        var l = new List<(bool Max, double X, double Y)>();
        if (string.IsNullOrWhiteSpace(s)) return l;
        foreach (Match m in Regex.Matches(s, @"(max|min): \(([^;]+); ([^)]+)\)"))
            if (Parse(m.Groups[2].Value, out var x) && Parse(m.Groups[3].Value, out var y))
                l.Add((m.Groups[1].Value == "max", x, y));
        return l;
    }

    private static double Kontroll(double lo, double hi)
    {
        if (double.IsInfinity(lo) && double.IsInfinity(hi)) return 0;
        if (double.IsNegativeInfinity(lo)) return hi - 1;
        if (double.IsPositiveInfinity(hi)) return lo + 1;
        return (lo + hi) / 2;
    }

    private static void Tabel(List<string> read, string? hulk, Func<double, double> fn,
                              string nimi, string mark, string jareldus)
    {
        foreach (var (lo, hi) in Vahemikud(hulk))
        {
            double t = Kontroll(lo, hi);
            double v = fn(t);
            read.Add($"Vahemik ({Ots(lo)}; {Ots(hi)}): kontrollpunkt x = {N(t)}, " +
                     $"{nimi}({N(t)}) = {G(v)} {mark} 0 ⇒ {jareldus}");
        }
    }

    private static double TLabel(string s)
    {
        var m = Regex.Match(s, @"^(\d*)(π?)(?:/(\d+))?$");
        if (!m.Success) return double.NaN;
        double a = m.Groups[1].Value == "" ? 1 : double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        double t = a * (m.Groups[2].Value == "π" ? Math.PI : 1);
        if (m.Groups[3].Success) t /= double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
        return t;
    }

    public static List<LahendusSamm> Koosta(FunktsiooniUurimine o)
    {
        string v = o.Valem;
        var tulemus = new List<LahendusSamm>();
        var ek = Ekstreemid(o.Ekstreemumid);

        // ===== 1 =====
        var r1 = new List<string>();
        string vl = v.ToLowerInvariant().Replace(" ", "");
        bool piirang = false;
        if (vl.Contains('/')) { r1.Add("Murru nimetaja ei tohi olla 0: lahendame nimetaja = 0 ja jätame need x välja."); piirang = true; }
        if (vl.Contains("sqrt(")) { r1.Add("Ruutjuure all peab olema avaldis ≥ 0: lahendame võrratuse."); piirang = true; }
        if (vl.Contains("ln(")) { r1.Add("Logaritmitav avaldis peab olema > 0: lahendame võrratuse."); piirang = true; }
        if (vl.Contains("tan(")) { r1.Add("tan x = sin x / cos x, seega cos x ≠ 0, st x ≠ π/2 + πk."); piirang = true; }
        if (!piirang) r1.Add("Valemis ei ole murde, juuri, logaritme ega tangensit, seega piiranguid ei ole: x võib olla mis tahes reaalarv.");
        r1.Add("Vastus: X = " + o.Maaramispiirkond);
        tulemus.Add(new("1. Määramispiirkond X", r1));

        // ===== 2 =====
        var r2 = new List<string> { "Y on funktsiooni kõigi väärtuste hulk f(x), kui x ∈ X." };
        foreach (var e in ek)
            r2.Add($"Lokaalne {(e.Max ? "maksimum" : "miinimum")}: f({N(e.X)}) = {N(e.Y)}");
        if ((o.Periood ?? "").StartsWith("perioodiline"))
        {
            r2.Add("Funktsioon on perioodiline, väärtused kordduvad: Y leitakse ühe perioodi põhjal.");
        }
        else
        {
            double a = ValemiTeenus.F(v, -1000), b = ValemiTeenus.F(v, 1000);
            if (!double.IsNaN(a)) r2.Add($"Kui x → −∞: f(−1000) ≈ {Piir(a)}");
            if (!double.IsNaN(b)) r2.Add($"Kui x → +∞: f(1000) ≈ {Piir(b)}");
        }
        r2.Add("Vastus: Y = " + o.Muutumispiirkond);
        tulemus.Add(new("2. Muutumispiirkond Y", r2));

        // ===== 3 =====
        var r3 = new List<string> { "Lahendame võrrandi f(x) = 0, kus f(x) = " + v };
        var nullid = Nullid(o.Nullkohad);
        if (nullid.Count == 0)
            r3.Add("Võrrandil lahendid puuduvad (graafik ei lõika x-telge).");
        foreach (var x in nullid)
            r3.Add($"x = {N(x)}: kontroll f({N(x)}) ≈ {G(ValemiTeenus.F(v, x))}");
        r3.Add("Vastus: X₀ = " + (nullid.Count == 0 ? "∅" : "{" + string.Join("; ", nullid.Select(N)) + "}"));
        tulemus.Add(new("3. Nullkohad X₀", r3));

        // ===== 4 =====
        var r4 = new List<string> {
            "Jagame teljestiku nullkohtade (ja katkevuskohtade) abil vahemikeks ning arvutame f(x) igas vahemikus kontrollpunktis." };
        Tabel(r4, o.Positiivsus, x => ValemiTeenus.F(v, x), "f", ">", "f(x) > 0");
        Tabel(r4, o.Negatiivsus, x => ValemiTeenus.F(v, x), "f", "<", "f(x) < 0");
        r4.Add("Vastus: X⁺ = " + o.Positiivsus);
        r4.Add("Vastus: X⁻ = " + o.Negatiivsus);
        tulemus.Add(new("4. Positiivsus- ja negatiivsuspiirkond", r4));

        // ===== 5 =====
        var r5 = new List<string> {
            "Tuletis: f'(x) = " + o.Tuletis,
            ek.Count == 0
                ? "Kriitilised punktid: f'(x) märk ei vaheta, ekstreemumkohti ei ole."
                : "Kriitilised punktid (f'(x) = 0): " + string.Join("; ", ek.Select(e => "x = " + N(e.X))),
            "Kui f'(x) > 0, siis funktsioon kasvab; kui f'(x) < 0, siis kahaneb. Kontrollime vahemikes:" };
        Tabel(r5, o.Kasvamine, x => ValemiTeenus.D(v, x), "f'", ">", "funktsioon kasvab");
        Tabel(r5, o.Kahanemine, x => ValemiTeenus.D(v, x), "f'", "<", "funktsioon kahaneb");
        r5.Add("Vastus: X↑ = " + o.Kasvamine);
        r5.Add("Vastus: X↓ = " + o.Kahanemine);
        tulemus.Add(new("5. Kasvamine ja kahanemine", r5));

        // ===== 6 =====
        var r6 = new List<string>();
        if (ek.Count == 0)
        {
            r6.Add("f'(x) märk ei muutu, seega ekstreemumeid ei ole.");
            r6.Add("Vastus: ekstreemumkohti ei ole");
        }
        else
        {
            r6.Add("Ekstreemum on koht, kus f'(x) märk muutub: + → − on maksimum, − → + on miinimum.");
            foreach (var e in ek)
            {
                double l = ValemiTeenus.D(v, e.X - 0.01), r = ValemiTeenus.D(v, e.X + 0.01);
                r6.Add($"x = {N(e.X)}: f' vasakul ≈ {G(l)} ({(l > 0 ? "+" : "−")}), paremal ≈ {G(r)} ({(r > 0 ? "+" : "−")}) ⇒ " +
                       $"{(e.Max ? "maksimum" : "miinimum")}, f({N(e.X)}) = {N(e.Y)}");
            }
            foreach (var e in ek)
                r6.Add($"Vastus: {(e.Max ? "maksimumkoht" : "miinimumkoht")} x = {N(e.X)}, f(x) = {N(e.Y)}");
        }
        tulemus.Add(new("6. Ekstreemumkohad Xₑ", r6));

        // ===== 7 =====
        var r7 = new List<string> {
            "Teine tuletis: f''(x) = " + SumboolneTeenus.TeineTuletis(v),
            "Kui f''(x) > 0, siis funktsioon on nõgus; kui f''(x) < 0, siis kumer. Kontrollime vahemikes:" };
        Tabel(r7, o.Nogusus, x => ValemiTeenus.Teine(v, x), "f''", ">", "nõgus");
        Tabel(r7, o.Kumerus, x => ValemiTeenus.Teine(v, x), "f''", "<", "kumer");
        var kaanu = Vahemikud(o.Kaanupunktid);
        r7.Add(kaanu.Count == 0
            ? "Käänupunkt on koht, kus f''(x) märk muutub. Selliseid kohti ei ole."
            : "Käänupunkt on koht, kus f''(x) märk muutub: " +
              string.Join("; ", kaanu.Select(k => $"x = {N(k.Lo)}, f(x) = {N(k.Hi)}")));
        r7.Add("Vastus: nõgus " + o.Nogusus);
        r7.Add("Vastus: kumer " + o.Kumerus);
        r7.Add("Vastus: käänupunktid " + (kaanu.Count == 0 ? "puuduvad"
            : string.Join("; ", kaanu.Select(k => $"({N(k.Lo)}; {N(k.Hi)})"))));
        tulemus.Add(new("7. Nõgusus, kumerus, käänupunktid", r7));

        // ===== 8 =====
        var r8 = new List<string> { "Kontrollime, kas f(−x) = f(x) (paaris) või f(−x) = −f(x) (paaritu)." };
        int kasut = 0;
        foreach (var x in new[] { 1.0, 2.0, 0.5, 3.0, 1.5 })
        {
            if (kasut >= 3) break;
            double a = ValemiTeenus.F(v, x), b = ValemiTeenus.F(v, -x);
            if (double.IsNaN(a) || double.IsNaN(b)) continue;
            kasut++;
            r8.Add($"x = {N(x)}: f({N(x)}) = {N(a)}, f({N(-x)}) = {N(b)}, −f({N(x)}) = {N(-a)}");
        }
        if (kasut == 0) r8.Add("Kontrollpunktides ei ole funktsioon määratud mõlemal pool nulli.");
        string p = (o.Paarsus ?? "").Trim();
        r8.Add("Vastus: " +
            (p.StartsWith("nii paaris") ? "nii paaris- kui ka paaritu funktsioon (f(x) = 0)" :
             p.StartsWith("paarisfunktsioon") ? "paarisfunktsioon, sest f(−x) = f(x); graafik on sümmeetriline y-telje suhtes" :
             p.StartsWith("paaritu") ? "paaritu funktsioon, sest f(−x) = −f(x); graafik on sümmeetriline koordinaatide alguspunkti suhtes" :
             p.StartsWith("ei ole") ? "ei ole paaris- ega paaritu funktsioon, sest ei kehti f(−x) = f(x) ega f(−x) = −f(x)" :
             "määramata"));
        tulemus.Add(new("8. Paaris- või paaritu funktsioon", r8));

        // ===== 9 =====
        var r9 = new List<string> { "Otsime vähimat T > 0, mille korral f(x + T) = f(x) kõigil x." };
        string per = (o.Periood ?? "").Trim();
        var pm = Regex.Match(per, @"T = (\S+) \(≈");
        if (per.StartsWith("perioodiline") && pm.Success)
        {
            double T = TLabel(pm.Groups[1].Value);
            if (!double.IsNaN(T))
                foreach (var x in new[] { 0.5, 1.3, 2.1 })
                {
                    double a = ValemiTeenus.F(v, x), b = ValemiTeenus.F(v, x + T);
                    if (double.IsNaN(a) || double.IsNaN(b)) continue;
                    r9.Add($"x = {N(x)}: f({N(x)}) = {N(a)}, f({N(x)} + T) = {N(b)}");
                }
            r9.Add("Vastus: funktsioon on perioodiline, T = " + pm.Groups[1].Value + " (≈ " + (double.IsNaN(T) ? "" : N(T)) + ")");
        }
        else if (per.StartsWith("konstantne"))
        {
            r9.Add("f(x) on konstant, igasugune T sobib, vähimat positiivset perioodi ei ole.");
            r9.Add("Vastus: konstantne funktsioon, periood puudub");
        }
        else
        {
            r9.Add("Proovisime perioodi T väärtusi (täisarvud, murrud, π kordsed): ühegi korral ei kehti f(x + T) = f(x) kõigis kontrollpunktides.");
            r9.Add("Vastus: funktsioon ei ole perioodiline");
        }
        tulemus.Add(new("9. Perioodilisus", r9));

        return tulemus;
    }
}