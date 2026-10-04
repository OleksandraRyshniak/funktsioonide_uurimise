using System.Text.RegularExpressions;
using MathNet.Symbolics;

namespace funktsioonide_uurimise.Models;

public static class SumboolneTeenus
{
    private static string Puhasta(string valem) =>
        Regex.Replace(valem, @"\b(Sqrt|Sin|Cos|Tan|Ln|Abs|Pi)\b",
            m => m.Value.ToLowerInvariant(), RegexOptions.IgnoreCase);

    public static string Tuletis(string valem)
    {
        try
        {
            var x = SymbolicExpression.Variable("x");
            var f = SymbolicExpression.Parse(Puhasta(valem));
            return f.Differentiate(x).ToString();
        }
        catch
        {
            return "arvutatakse numbriliselt";
        }
    }

    public static string TeineTuletis(string valem)
    {
        try
        {
            var x = SymbolicExpression.Variable("x");
            var f = SymbolicExpression.Parse(Puhasta(valem));
            return f.Differentiate(x).Differentiate(x).ToString();
        }
        catch
        {
            return "arvutatakse numbriliselt";
        }
    }
}