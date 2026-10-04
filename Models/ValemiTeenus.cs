using System.Globalization;
using NCalc;

namespace funktsioonide_uurimise.Models;

public static class ValemiTeenus
{
    private static string S(double v) => v.ToString(CultureInfo.InvariantCulture);

    private static string A(double v) => S(v).Replace('-', '−');

    // x^3 -> Pow(x,3); töötab ka pesastatud sulgudega
    public static string Normaliseeri(string valem)
    {
        int pos;
        int piirang = 200;
        while ((pos = valem.LastIndexOf('^')) >= 0 && piirang-- > 0)
        {
            int vasakLopp = pos;
            while (vasakLopp > 0 && valem[vasakLopp - 1] == ' ') vasakLopp--;
            int vasakAlg = OperandiAlgus(valem, vasakLopp);

            int paremAlg = pos + 1;
            while (paremAlg < valem.Length && valem[paremAlg] == ' ') paremAlg++;
            int paremOperand = paremAlg;
            if (paremOperand < valem.Length && (valem[paremOperand] == '-' || valem[paremOperand] == '+'))
                paremOperand++;
            int paremLopp = OperandiLopp(valem, paremOperand);

            if (vasakAlg < 0 || paremLopp <= paremOperand) break;

            string vasak = valem[vasakAlg..vasakLopp];
            string parem = valem[paremAlg..paremLopp];
            valem = valem[..vasakAlg] + "Pow(" + vasak + "," + parem + ")" + valem[paremLopp..];
        }
        return valem;
    }

    private static int OperandiAlgus(string s, int lopp)
    {
        if (lopp == 0) return -1;
        if (s[lopp - 1] == ')')
        {
            int sygavus = 0;
            for (int i = lopp - 1; i >= 0; i--)
            {
                if (s[i] == ')') sygavus++;
                else if (s[i] == '(')
                {
                    sygavus--;
                    if (sygavus == 0)
                    {
                        int alg = i;
                        while (alg > 0 && char.IsLetter(s[alg - 1])) alg--;
                        return alg;
                    }
                }
            }
            return -1;
        }
        int k = lopp;
        while (k > 0 && (char.IsLetterOrDigit(s[k - 1]) || s[k - 1] == '.' || s[k - 1] == '_')) k--;
        return k == lopp ? -1 : k;
    }

    private static int OperandiLopp(string s, int i)
    {
        if (i >= s.Length) return i;
        if (s[i] == '(') return SulguLopp(s, i);
        if (char.IsLetter(s[i]))
        {
            int j = i;
            while (j < s.Length && (char.IsLetterOrDigit(s[j]) || s[j] == '_')) j++;
            return j < s.Length && s[j] == '(' ? SulguLopp(s, j) : j;
        }
        if (char.IsDigit(s[i]) || s[i] == '.')
        {
            int j = i;
            while (j < s.Length && (char.IsDigit(s[j]) || s[j] == '.')) j++;
            return j;
        }
        return i;
    }

    private static int SulguLopp(string s, int avatud)
    {
        int sygavus = 0;
        for (int i = avatud; i < s.Length; i++)
        {
            if (s[i] == '(') sygavus++;
            else if (s[i] == ')')
            {
                sygavus--;
                if (sygavus == 0) return i + 1;
            }
        }
        return s.Length;
    }

    // parsitud avaldiste vahemälu (kiirendab tuhandeid arvutusi)
    [ThreadStatic] private static Dictionary<string, Expression>? _vahemalu;

    public static double F(string valem, double x)
    {
        try
        {
            _vahemalu ??= new Dictionary<string, Expression>();
            if (!_vahemalu.TryGetValue(valem, out var avaldis))
            {
                if (_vahemalu.Count > 20) _vahemalu.Clear();
                avaldis = new Expression(Normaliseeri(valem));
                _vahemalu[valem] = avaldis;
            }
            avaldis.Parameters["x"] = x;
            avaldis.Parameters["Pi"] = Math.PI;
            avaldis.Parameters["e"] = Math.E;
            double tulemus = Convert.ToDouble(avaldis.Evaluate());
            return double.IsFinite(tulemus) ? tulemus : double.NaN;
        }
        catch
        {
            return double.NaN;
        }
    }

    public static double D(string valem, double x)
    {
        const double h = 1e-5;
        return (F(valem, x + h) - F(valem, x - h)) / (2 * h);
    }

    public static double Teine(string valem, double x)
    {
        const double h = 1e-3;
        double f = F(valem, x), a = F(valem, x + h), b = F(valem, x - h);
        if (double.IsNaN(f) || double.IsNaN(a) || double.IsNaN(b)) return double.NaN;
        return (a - 2 * f + b) / (h * h);
    }
    private static int Margi(double d) =>
        double.IsNaN(d) || Math.Abs(d) < 1e-7 ? 0 : Math.Sign(d);



    // ================= 6. EKSTREEMUMID =================

    public static List<(double X, double Y, bool Max)> LeiaEkstreemid(string valem, double alates = -10, double kuni = 10)
    {
        var tulemus = new List<(double X, double Y, bool Max)>();
        const int Samme = 2000;
        double h = (kuni - alates) / Samme;

        double viimaX = alates;
        int viimaMark = Margi(D(valem, alates));

        for (int i = 1; i <= Samme; i++)
        {
            double x = alates + i * h;
            int mark = Margi(D(valem, x));
            if (mark == 0) continue;

            if (viimaMark != 0 && mark != viimaMark)
            {
                double vasak = viimaX, parem = x;
                for (int j = 0; j < 60; j++)
                {
                    double keskel = (vasak + parem) / 2;
                    double dk = D(valem, keskel);
                    if (double.IsNaN(dk)) break;
                    if (Math.Sign(dk) == viimaMark) vasak = keskel; else parem = keskel;
                }
                double xe = Math.Round((vasak + parem) / 2, 4) + 0.0;
                double ye = Math.Round(F(valem, xe), 4) + 0.0;
                if (!double.IsNaN(ye) && Math.Abs(ye) < 1e6)
                    tulemus.Add((xe, ye, viimaMark > 0));
            }

            viimaMark = mark;
            viimaX = x;
        }
        return tulemus;
    }

    public static string Analuusi(string valem)
    {
        var loend = LeiaEkstreemid(valem);
        if (loend.Count == 0) return "puuduvad";

        return string.Join("\n",
            loend.Select(e => $"{(e.Max ? "max" : "min")}: ({A(e.X)}; {A(e.Y)})"));
    }

    // ================= 1. MÄÄRAMISPIIRKOND =================

    public static string Maaramispiirkond(string valem)
    {
        const int N = 5000; // x = -50 ... 50, samm 0.01
        var kehtib = new bool[2 * N + 1];
        for (int i = 0; i <= 2 * N; i++)
            kehtib[i] = !double.IsNaN(F(valem, (i - N) / 100.0));

        if (kehtib.All(b => b)) return "(−∞; ∞)";
        if (kehtib.All(b => !b)) return "∅";

        var osad = new List<string>();
        int algus = -1;
        for (int i = 0; i <= 2 * N + 1; i++)
        {
            bool sees = i <= 2 * N && kehtib[i];
            if (sees && algus < 0) algus = i;
            else if (!sees && algus >= 0)
            {
                int lopp = i - 1;
                string vasak, parem;

                if (algus == 0) vasak = "(−∞";
                else
                {
                    var (p, kuulub) = Piir(valem, (algus - N) / 100.0, (algus - 1 - N) / 100.0);
                    vasak = (kuulub ? "[" : "(") + A(p);
                }

                if (lopp == 2 * N) parem = "∞)";
                else
                {
                    var (p, kuulub) = Piir(valem, (lopp - N) / 100.0, (lopp + 1 - N) / 100.0);
                    parem = A(p) + (kuulub ? "]" : ")");
                }

                osad.Add(vasak + "; " + parem);
                algus = -1;
            }
        }
        return string.Join(" ∪ ", osad);
    }

    // ================= 2. MUUTUMISPIIRKOND =================

    // Tyyp 0 = teadmata, 1 = lõplik piirväärtus V, 2 = +∞, 3 = −∞
    private static (int Tyyp, double V) Piirvaartus(string valem, double[] xid)
    {
        var v = xid.Select(x => F(valem, x)).ToArray();
        if (v.Any(double.IsNaN)) return (0, 0);
        foreach (var a in v)
            if (Math.Abs(a) > 1e5) return (a > 0 ? 2 : 3, 0);
        if (Math.Abs(v[2]) > Math.Abs(v[1]) + 0.5 && Math.Abs(v[1]) > Math.Abs(v[0]) + 0.5)
            return (v[2] > 0 ? 2 : 3, 0);
        return (1, v[2]);
    }

    public static string Muutumispiirkond(string valem)
    {
        const int N = 5000;
        int n = 2 * N + 1;
        var xs = new double[n];
        var ys = new double[n];
        for (int i = 0; i < n; i++) { xs[i] = (i - N) / 100.0; ys[i] = F(valem, xs[i]); }
        if (ys.All(double.IsNaN)) return "∅";

        var ekstr = LeiaEkstreemid(valem, -50, 50);
        var osad = new List<(double Lo, bool LoK, double Hi, bool HiK)>();
        double Ymarda(double v) => double.IsInfinity(v) ? v : Math.Round(v, 4) + 0.0;

        int algus = -1;
        for (int i = 0; i <= n; i++)
        {
            bool sees = i < n && !double.IsNaN(ys[i]);
            if (sees && algus < 0) { algus = i; continue; }
            if (sees || algus < 0) continue;

            int a0 = algus, lopp = i - 1;
            algus = -1;

            var seg = ys[a0..(lopp + 1)];
            if (seg.Max() - seg.Min() < 1e-9)           // konstant
            {
                double c = Ymarda(seg[0]);
                osad.Add((c, true, c, true));
                continue;
            }

            var kand = new List<(double V, bool Saav)>();
            foreach (var e in ekstr)
                if (e.X >= xs[a0] && e.X <= xs[lopp]) kand.Add((e.Y, true));

            bool ylal = false, alal = false;
            foreach (var (idx, suund) in new[] { (a0, -1), (lopp, 1) })
            {
                bool serv = idx == 0 || idx == n - 1;
                double[] punktid;
                double ra = 0, rp = 0;
                bool nice = false, kuulub = false;

                if (serv)
                {
                    punktid = new[] { 100.0, 1e4, 1e6 }.Select(t => suund * t).ToArray();
                    kand.Add((ys[idx], false));
                }
                else
                {
                    ra = xs[idx];
                    double rb = xs[idx + suund];
                    for (int j = 0; j < 60; j++)
                    {
                        double m = (ra + rb) / 2;
                        if (double.IsNaN(F(valem, m))) rb = m; else ra = m;
                    }
                    rp = Math.Round(ra, 6);
                    nice = Math.Abs(ra - rp) < 1e-8;
                    kuulub = nice && !double.IsNaN(F(valem, rp));
                    punktid = new[] { 1e-3, 1e-6, 1e-9 }.Select(d => ra - suund * d).ToArray();
                }

                var (tyyp, v) = Piirvaartus(valem, punktid);
                if (tyyp == 2) ylal = true;
                else if (tyyp == 3) alal = true;
                else if (tyyp == 1)
                {
                    kand.Add((v, false));
                    if (!serv)
                    {
                        if (!nice) kuulub = Math.Abs(F(valem, ra) - v) < 1e-6;
                        if (kuulub) kand.Add((F(valem, nice ? rp : ra), true));
                    }
                }
            }

            double lo = alal ? double.NegativeInfinity : kand.Count > 0 ? kand.Min(c => c.V) : double.NaN;
            double hi = ylal ? double.PositiveInfinity : kand.Count > 0 ? kand.Max(c => c.V) : double.NaN;
            if (double.IsNaN(lo) || double.IsNaN(hi)) continue;

            bool loK = !alal && kand.Any(c => c.Saav && Math.Abs(c.V - lo) < 1e-6);
            bool hiK = !ylal && kand.Any(c => c.Saav && Math.Abs(c.V - hi) < 1e-6);
            osad.Add((Ymarda(lo), loK, Ymarda(hi), hiK));
        }

        if (osad.Count == 0) return "∅";

        // ühendame kattuvad vahemikud
        osad.Sort((p, q) => p.Lo.CompareTo(q.Lo));
        var liidetud = new List<(double Lo, bool LoK, double Hi, bool HiK)>();
        foreach (var o in osad)
        {
            if (liidetud.Count > 0)
            {
                var l = liidetud[^1];
                if (o.Lo < l.Hi || (o.Lo == l.Hi && (l.HiK || o.LoK)))
                {
                    bool hiK = o.Hi > l.Hi ? o.HiK : o.Hi < l.Hi ? l.HiK : (l.HiK || o.HiK);
                    bool loK = l.Lo == o.Lo ? (l.LoK || o.LoK) : l.LoK;
                    liidetud[^1] = (l.Lo, loK, Math.Max(l.Hi, o.Hi), hiK);
                    continue;
                }
            }
            liidetud.Add(o);
        }

        string Ots(double v) => double.IsNegativeInfinity(v) ? "−∞" : double.IsPositiveInfinity(v) ? "∞" : A(v);
        return string.Join(" ∪ ", liidetud.Select(o =>
            o.Lo == o.Hi && o.LoK && o.HiK
                ? "{" + Ots(o.Lo) + "}"
                : (o.LoK ? "[" : "(") + Ots(o.Lo) + "; " + Ots(o.Hi) + (o.HiK ? "]" : ")")));
    }

    // ================= 5. KASVAMINE / KAHANEMINE =================

    private static double Serv(string valem, double sees, double valjas)
    {
        for (int k = 0; k < 50; k++)
        {
            double m = (sees + valjas) / 2;
            if (double.IsNaN(F(valem, m))) valjas = m; else sees = m;
        }
        return Math.Round((sees + valjas) / 2, 4) + 0.0;
    }

    public static (string Kasvab, string Kahaneb) Monotoonsus(string valem)
    {
        const int N = 5000;
        int n = 2 * N + 1;
        var xs = new double[n];
        var ys = new double[n];
        for (int i = 0; i < n; i++) { xs[i] = (i - N) / 100.0; ys[i] = F(valem, xs[i]); }

        var ekstr = LeiaEkstreemid(valem, -50, 50);
        double Lahim(double x) => ekstr.Where(e => Math.Abs(e.X - x) < 0.02)
            .OrderBy(e => Math.Abs(e.X - x)).Select(e => e.X).DefaultIfEmpty(Math.Round(x, 4) + 0.0).First();

        var kehtib = new bool[n - 1];
        var m = new int[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            kehtib[i] = !double.IsNaN(ys[i]) && !double.IsNaN(ys[i + 1]);
            if (!kehtib[i]) continue;
            double vahe = ys[i + 1] - ys[i];
            m[i] = Math.Abs(vahe) > 1e-12 * (Math.Abs(ys[i]) + Math.Abs(ys[i + 1])) ? Math.Sign(vahe) : 0;
        }

        var kasv = new List<string>();
        var kah = new List<string>();
        int i0 = 0;
        while (i0 < n - 1)
        {
            if (m[i0] == 0) { i0++; continue; }
            int s = m[i0], j = i0;
            while (j + 1 < n - 1 && m[j + 1] == s) j++;

            string vasak;
            if (i0 == 0) vasak = "−∞";
            else if (kehtib[i0 - 1]) vasak = A(Lahim(xs[i0]));
            else vasak = A(Serv(valem, xs[i0], xs[i0 - 1]));

            string parem;
            if (j + 1 == n - 1) parem = "∞";
            else if (kehtib[j + 1]) parem = A(Lahim(xs[j + 1]));
            else parem = A(Serv(valem, xs[j + 1], xs[j + 2]));

            (s > 0 ? kasv : kah).Add($"({vasak}; {parem})");
            i0 = j + 1;
        }

        return (kasv.Count == 0 ? "puuduvad" : string.Join(" ∪ ", kasv),
                kah.Count == 0 ? "puuduvad" : string.Join(" ∪ ", kah));
    }

    // ================= 7. NÕGUSUS / KUMERUS / KÄÄNUPUNKTID =================
    // f''(x) > 0 -> nõgus, f''(x) < 0 -> kumer
    public static (string Nogus, string Kumer, string Kaanu) Kumerus(string valem)
    {
        const int N = 5000;
        const double h = 1e-3;
        int n = 2 * N + 1;
        var xs = new double[n];
        for (int i = 0; i < n; i++) xs[i] = (i - N) / 100.0;

        double Teine(double x)
        {
            double f = F(valem, x), a = F(valem, x + h), b = F(valem, x - h);
            if (double.IsNaN(f) || double.IsNaN(a) || double.IsNaN(b)) return double.NaN;
            return (a - 2 * f + b) / (h * h);
        }

        // 2 = ei ole määratud, 0 = f'' ≈ 0, ±1 = f'' märk
        int Mark(double x)
        {
            double d = Teine(x);
            if (double.IsNaN(d)) return 2;
            double eps = 1e-7 * (1 + Math.Abs(F(valem, x)));
            return Math.Abs(d) < eps ? 0 : Math.Sign(d);
        }

        int Toores(double x)
        {
            double d = Teine(x);
            return double.IsNaN(d) ? 2 : Math.Sign(d);
        }

        var m = new int[n];
        for (int i = 0; i < n; i++) m[i] = Mark(xs[i]);

        bool Katkestus(int p, int q)
        {
            for (int k = p + 1; k < q; k++) if (m[k] == 2) return true;
            return false;
        }

        // koht, kus märk vahetub p ja q vahel
        double Piir(int p, int q)
        {
            var nullid = new List<int>();
            for (int k = p + 1; k < q; k++) if (m[k] == 0) nullid.Add(k);
            if (nullid.Count > 0)
                return Math.Round((xs[nullid[0]] + xs[nullid[^1]]) / 2, 4) + 0.0;

            double a = xs[p], b = xs[q];
            int s = m[p];
            for (int k = 0; k < 50; k++)
            {
                double mid = (a + b) / 2;
                if (Toores(mid) == s) a = mid; else b = mid;
            }
            return Math.Round((a + b) / 2, 4) + 0.0;
        }

        // sama märgiga järjestikused lõigud (nullid ei katkesta)
        var rühmad = new List<(int S, int A, int B)>();
        for (int i = 0; i < n; i++)
        {
            if (m[i] != 1 && m[i] != -1) continue;
            if (rühmad.Count > 0 && rühmad[^1].S == m[i] && !Katkestus(rühmad[^1].B, i))
                rühmad[^1] = (m[i], rühmad[^1].A, i);
            else
                rühmad.Add((m[i], i, i));
        }

        string Intervall(int s)
        {
            var osad = new List<string>();
            for (int g = 0; g < rühmad.Count; g++)
            {
                if (rühmad[g].S != s) continue;
                var (_, a, b) = rühmad[g];

                string vasak;
                if (g > 0 && !Katkestus(rühmad[g - 1].B, a))
                    vasak = A(Piir(rühmad[g - 1].B, a));
                else
                {
                    int L = a;
                    while (L > 0 && m[L - 1] != 2) L--;
                    vasak = L == 0 ? "−∞" : A(Serv(valem, xs[L], xs[L - 1]));
                }

                string parem;
                if (g < rühmad.Count - 1 && !Katkestus(b, rühmad[g + 1].A))
                    parem = A(Piir(b, rühmad[g + 1].A));
                else
                {
                    int R = b;
                    while (R < n - 1 && m[R + 1] != 2) R++;
                    parem = R == n - 1 ? "∞" : A(Serv(valem, xs[R], xs[R + 1]));
                }

                osad.Add($"({vasak}; {parem})");
            }
            return osad.Count == 0 ? "puuduvad" : string.Join(" ∪ ", osad);
        }

        var kaanu = new List<string>();
        for (int g = 0; g < rühmad.Count - 1; g++)
        {
            if (Katkestus(rühmad[g].B, rühmad[g + 1].A)) continue;
            double x = Piir(rühmad[g].B, rühmad[g + 1].A);
            double y = F(valem, x);
            if (double.IsNaN(y) || Math.Abs(y) > 1e6) continue;
            kaanu.Add($"({A(x)}; {A(Math.Round(y, 4) + 0.0)})");
        }

        return (Intervall(1), Intervall(-1),
                kaanu.Count == 0 ? "puuduvad" : string.Join("\n", kaanu));
    }

    // ================= 8. PAARIS / PAARITU =================

    public static string Paarsus(string valem)
    {
        double[] punktid = { 0.3, 0.7, 1.1, 1.9, 2.6, 3.4, 4.5, 7.3 };
        bool paaris = true, paaritu = true;
        int kasutatud = 0;

        foreach (var x in punktid)
        {
            double a = F(valem, x), b = F(valem, -x);
            bool an = double.IsNaN(a), bn = double.IsNaN(b);
            if (an && bn) continue;
            kasutatud++;
            if (an || bn) { paaris = paaritu = false; break; }
            double tol = 1e-7 * (1 + Math.Abs(a));
            if (Math.Abs(b - a) > tol) paaris = false;
            if (Math.Abs(b + a) > tol) paaritu = false;
        }

        if (kasutatud == 0) return "määramata";
        if (paaris && paaritu) return "nii paaris- kui ka paaritu funktsioon (f(x) = 0)";
        if (paaris) return "paarisfunktsioon: f(−x) = f(x), graafik on sümmeetriline y-telje suhtes";
        if (paaritu) return "paaritu funktsioon: f(−x) = −f(x), graafik on sümmeetriline koordinaatide alguspunkti suhtes";
        return "ei ole paaris- ega paaritu funktsioon";
    }

    // ================= 9. PERIOOD =================

    private static int Syt(int a, int b) => b == 0 ? a : Syt(b, a % b);

    public static string Periood(string valem)
    {
        double[] proovid = { -6.37, -4.11, -2.53, -0.77, 0.29, 1.43, 2.97, 4.61, 5.83, 7.19, 9.41, 12.27 };
        var f0 = proovid.Select(x => F(valem, x)).ToArray();
        var kehtivad = f0.Where(v => !double.IsNaN(v)).ToArray();
        if (kehtivad.Length == 0) return "määramata";
        if (kehtivad.Length > 1 && kehtivad.Max() - kehtivad.Min() < 1e-9)
            return "konstantne funktsioon (periood puudub)";

        var kandidaadid = new List<(double T, string Silt)>();
        for (int a = 1; a <= 12; a++)
            for (int b = 1; b <= 6; b++)
            {
                if (Syt(a, b) != 1) continue;
                double r = (double)a / b;
                kandidaadid.Add((r, b == 1 ? $"{a}" : $"{a}/{b}"));
                kandidaadid.Add((r * Math.PI, (a == 1 ? "π" : a + "π") + (b == 1 ? "" : "/" + b)));
            }

        foreach (var (T, silt) in kandidaadid.OrderBy(k => k.T))
        {
            bool sobib = true;
            for (int i = 0; i < proovid.Length && sobib; i++)
            {
                double u = f0[i], v = F(valem, proovid[i] + T);
                bool un = double.IsNaN(u), vn = double.IsNaN(v);
                if (un && vn) continue;
                if (un || vn || Math.Abs(u - v) > 1e-7 * (1 + Math.Abs(u))) sobib = false;
            }
            if (sobib) return $"perioodiline, T = {silt} (≈ {S(Math.Round(T, 4))})";
        }
        return "ei ole perioodiline";
    }

    // ================= 3-4. NULLKOHAD, POSITIIVSUS, NEGATIIVSUS =================

    public static (string Nullid, string Positiivsed, string Negatiivsed) Margiuuring(string valem)
    {
        const int N = 5000;
        int n = 2 * N + 1;
        var xs = new double[n];
        var ys = new double[n];
        for (int i = 0; i < n; i++) { xs[i] = (i - N) / 100.0; ys[i] = F(valem, xs[i]); }

        const double eps = 1e-9;
        var nullid = new SortedSet<double>();
        for (int i = 0; i < n; i++)
        {
            if (double.IsNaN(ys[i])) continue;
            if (Math.Abs(ys[i]) < eps) { nullid.Add(Math.Round(xs[i], 4) + 0.0); continue; }
            if (i + 1 < n && !double.IsNaN(ys[i + 1]) && Math.Abs(ys[i + 1]) >= eps
                && Math.Sign(ys[i]) != Math.Sign(ys[i + 1]))
            {
                double a = xs[i], b = xs[i + 1];
                for (int k = 0; k < 60; k++)
                {
                    double m = (a + b) / 2, v = F(valem, m);
                    if (double.IsNaN(v)) break;
                    if (Math.Sign(v) == Math.Sign(ys[i])) a = m; else b = m;
                }
                double juur = (a + b) / 2;
                if (Math.Abs(F(valem, juur)) < 1e-6)      // ei ole katkevuskoht (nt tan x)
                    nullid.Add(Math.Round(juur, 4) + 0.0);
            }
        }

        string nullTekst = nullid.Count == 0 ? "puuduvad"
            : string.Join("\n", nullid.Select(x => $"x = {A(x)}"));

        return (nullTekst,
                Vahemikud(valem, xs, ys, v => v > eps),
                Vahemikud(valem, xs, ys, v => v < -eps));
    }

    private static string Vahemikud(string valem, double[] xs, double[] ys, Func<double, bool> tingimus)
    {
        var osad = new List<string>();
        int n = xs.Length, algus = -1;
        for (int i = 0; i <= n; i++)
        {
            bool sees = i < n && !double.IsNaN(ys[i]) && tingimus(ys[i]);
            if (sees && algus < 0) algus = i;
            else if (!sees && algus >= 0)
            {
                int lopp = i - 1;
                string vasak = algus == 0 ? "−∞" : A(Rajapunkt(valem, tingimus, xs[algus], xs[algus - 1]));
                string parem = lopp == n - 1 ? "∞" : A(Rajapunkt(valem, tingimus, xs[lopp], xs[lopp + 1]));
                osad.Add($"({vasak}; {parem})");
                algus = -1;
            }
        }
        return osad.Count == 0 ? "puuduvad" : string.Join(" ∪ ", osad);
    }

    private static double Rajapunkt(string valem, Func<double, bool> tingimus, double tosi, double vale)
    {
        for (int k = 0; k < 50; k++)
        {
            double m = (tosi + vale) / 2, v = F(valem, m);
            if (!double.IsNaN(v) && tingimus(v)) tosi = m; else vale = m;
        }
        return Math.Round((tosi + vale) / 2, 4) + 0.0;
    }

    private static (double Piir, bool Kuulub) Piir(string valem, double kehtiv, double kehtetu)
    {
        for (int i = 0; i < 40; i++)
        {
            double keskel = (kehtiv + kehtetu) / 2;
            if (double.IsNaN(F(valem, keskel))) kehtetu = keskel; else kehtiv = keskel;
        }
        double p = Math.Round((kehtiv + kehtetu) / 2, 3) + 0.0;
        return (p, !double.IsNaN(F(valem, p)));
    }
}