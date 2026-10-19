using funktsioonide_uurimise.Models;

namespace funktsioonide_uurimise.Tests;

// Boonusülesanne (juhendi p 3.5): kas NCalc arvutab sisestatud valemi y-väärtused õigesti,
// mis juhtub vigase valemi ja nulliga jagamise korral.
public class ValemiArvutamiseTestid
{
    [Theory]
    [InlineData("x^2 - 4*x + 3", 1, 0)]
    [InlineData("x^2 - 4*x + 3", 2, -1)]
    [InlineData("x^3 - 3*x", -1, 2)]
    [InlineData("x^3 - 12*x", 2, -16)]
    [InlineData("-x^2", 3, -9)]          // miinus ei kuulu astendatava alla
    [InlineData("(x+1)^2", 2, 9)]        // sulud astendatavana
    [InlineData("2^x^2", 2, 16)]         // aste on paremassotsiatiivne: 2^(x^2)
    [InlineData("x^-1", 4, 0.25)]        // negatiivne astendaja
    [InlineData("1/2*x", 4, 2)]          // täisarvude jagamine ei tohi anda 0
    [InlineData("Sqrt(x)", 9, 3)]
    [InlineData("Sin(Pi/2)", 0, 1)]
    [InlineData("Ln(e)", 0, 1)]
    public void F_KehtivValem_ArvutabOigeVaartuse(string valem, double x, double oodatav)
    {
        Assert.Equal(oodatav, ValemiTeenus.F(valem, x), 6);
    }

    [Theory]
    [InlineData("x^3", "Pow(x,3)")]
    [InlineData("x^3 - 3*x", "Pow(x,3) - 3*x")]
    [InlineData("(x+1)^2", "Pow((x+1),2)")]
    [InlineData("Sin(x)^2", "Pow(Sin(x),2)")]
    public void Normaliseeri_Astendamine_TeisendatakseNCalcPow(string sisend, string oodatav)
    {
        Assert.Equal(oodatav, ValemiTeenus.Normaliseeri(sisend));
    }

    [Theory]
    [InlineData("x^^2")]
    [InlineData("sin(x")]
    [InlineData("3*")]
    [InlineData("")]
    [InlineData("<script>alert(1)</script>")]
    public void F_ViganeValem_TagastabNaNJaEiViskaErindit(string valem)
    {
        double tulemus = ValemiTeenus.F(valem, 1);

        Assert.True(double.IsNaN(tulemus));
    }

    [Fact]
    public void F_NulligaJagamine_TagastabNaN()
    {
        // 1/x punktis x = 0 ei ole määratud
        Assert.True(double.IsNaN(ValemiTeenus.F("1/x", 0)));
        Assert.Equal(0.5, ValemiTeenus.F("1/x", 2), 6);
    }

    [Fact]
    public void F_JuurNegatiivsest_TagastabNaN()
    {
        Assert.True(double.IsNaN(ValemiTeenus.F("Sqrt(x)", -1)));
    }

    [Fact]
    public void F_KumneAlusegaLogaritm_ArvutatakseOigesti()
    {
        // Kasutajaliidese klaviatuuri nupp "log" teisendatakse valemiks "Log(...)" (valemi-toimetaja.js).
        // log(100) = 2
        Assert.Equal(2, ValemiTeenus.F("Log(x)", 100), 6);
    }

    [Fact]
    public void Maaramispiirkond_PolunoomJaJuurJaMurd()
    {
        Assert.Equal("(−∞; ∞)", ValemiTeenus.Maaramispiirkond("x^3 - 3*x"));
        Assert.Equal("[0; ∞)", ValemiTeenus.Maaramispiirkond("Sqrt(x)"));
        Assert.Equal("(−∞; 0) ∪ (0; ∞)", ValemiTeenus.Maaramispiirkond("1/x"));
    }

    [Fact]
    public void Muutumispiirkond_RuutFunktsioon_AlatesMiinimumist()
    {
        // x² − 4x + 3 miinimum on −1, seega Y = [−1; ∞)
        Assert.Equal("[−1; ∞)", ValemiTeenus.Muutumispiirkond("x^2 - 4*x + 3"));
    }

    [Theory]
    [InlineData("x^2 - 4*x + 3", "ei ole paaris")]
    [InlineData("x^4 - 2*x^2", "paarisfunktsioon")]
    [InlineData("x^3 - 3*x", "paaritu funktsioon")]
    public void Paarsus_KontrollFunktsioonid(string valem, string algus)
    {
        Assert.StartsWith(algus, ValemiTeenus.Paarsus(valem));
    }

    [Fact]
    public void Periood_Siinus_On2Pi()
    {
        Assert.StartsWith("perioodiline, T = 2π", ValemiTeenus.Periood("Sin(x)"));
        Assert.Equal("ei ole perioodiline", ValemiTeenus.Periood("x^3 - 3*x"));
    }
}
