using System.Globalization;
using NCalc;

namespace funktsioonide_uurimise.Models;

public static class ValemiTeenus
{
    private static string S(double v) => v.ToString(CultureInfo.InvariantCulture);

    // x^3 -> Pow(x,3); töötab ka pesastatud sulgudega

    private static string A(double v) => S(v).Replace('-', '−');
    private static string Indeks(int n) =>
        string.Concat(n.ToString().Select(c => "₀₁₂₃₄₅₆₇₈₉"[c - '0']));
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

    public static double F(string valem, double x)
    {
        try
        {
            var avaldis = new Expression(Normaliseeri(valem));
            avaldis.Parameters["x"] = x;
            avaldis.Parameters["Pi"] = Math.PI;
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

    private static int Margi(double d) =>
        double.IsNaN(d) || Math.Abs(d) < 1e-7 ? 0 : Math.Sign(d);

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

    // nullkohad + positiivsus- ja negatiivsuspiirkonnad (x = -50 ... 50)
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