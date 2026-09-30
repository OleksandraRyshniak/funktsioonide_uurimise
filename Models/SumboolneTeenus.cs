using System.Text.RegularExpressions;
using MathNet.Symbolics;

namespace funktsioonide_uurimise.Models;

public static class SumboolneTeenus
{
    public static string Tuletis(string valem)
    {
        try
        {
            string puhas = Regex.Replace(valem, @"\b(Sqrt|Sin|Cos|Tan|Ln|Abs|Pi)\b",
                m => m.Value.ToLowerInvariant(), RegexOptions.IgnoreCase);

            var x = SymbolicExpression.Variable("x");
            var f = SymbolicExpression.Parse(puhas);
            return f.Differentiate(x).ToString();
        }
        catch
        {
            return "arvutatakse numbriliselt";
        }
    }
}