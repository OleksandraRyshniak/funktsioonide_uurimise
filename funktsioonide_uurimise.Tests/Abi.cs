using System.Globalization;
using System.Text.RegularExpressions;

namespace funktsioonide_uurimise.Tests;

// Abimeetodid rakenduse tekstiliste tulemuste (nt "x = −1.7321") lugemiseks arvudeks.
internal static class Abi
{
    // Rakendus kasutab miinusmärgina sümbolit "−" (U+2212), mitte tavalist "-".
    public static double Arv(string tekst) =>
        double.Parse(tekst.Trim().Replace('−', '-'), NumberStyles.Float, CultureInfo.InvariantCulture);

    // "x = −1.7321\nx = 0\nx = 1.7321" -> [-1.7321, 0, 1.7321]
    public static List<double> Nullkohad(string tekst) =>
        Regex.Matches(tekst, @"x = (\S+)")
            .Select(m => Arv(m.Groups[1].Value))
            .ToList();

    // "max: (−1; 2)\nmin: (1; −2)" -> [(true, -1, 2), (false, 1, -2)]
    public static List<(bool Max, double X, double Y)> Ekstreemumid(string tekst) =>
        Regex.Matches(tekst, @"(max|min): \(([^;]+); ([^)]+)\)")
            .Select(m => (m.Groups[1].Value == "max", Arv(m.Groups[2].Value), Arv(m.Groups[3].Value)))
            .ToList();
}
