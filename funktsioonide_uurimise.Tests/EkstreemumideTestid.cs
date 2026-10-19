using funktsioonide_uurimise.Models;

namespace funktsioonide_uurimise.Tests;

// Kriitiliste punktide ja ekstreemumite leidmine (ValemiTeenus.LeiaEkstreemid ja Analuusi).
// Oodatavad väärtused on juhendi tabelist 3.
public class EkstreemumideTestid
{
    [Fact]
    public void Ekstreemumid_RuutFunktsioon_UksMiinimum()
    {
        // f(x) = x² − 4x + 3: min (2; −1)
        var tulemus = ValemiTeenus.LeiaEkstreemid("x^2 - 4*x + 3");

        var e = Assert.Single(tulemus);
        Assert.False(e.Max);
        Assert.Equal(2, e.X, 3);
        Assert.Equal(-1, e.Y, 3);
    }

    [Fact]
    public void Ekstreemumid_KuupFunktsioon_MaksimumJaMiinimum()
    {
        // f(x) = x³ − 3x: max (−1; 2); min (1; −2)
        var tulemus = ValemiTeenus.LeiaEkstreemid("x^3 - 3*x");

        Assert.Equal(2, tulemus.Count);
        Assert.True(tulemus[0].Max);
        Assert.Equal(-1, tulemus[0].X, 3);
        Assert.Equal(2, tulemus[0].Y, 3);
        Assert.False(tulemus[1].Max);
        Assert.Equal(1, tulemus[1].X, 3);
        Assert.Equal(-2, tulemus[1].Y, 3);
    }

    [Fact]
    public void Ekstreemumid_KuupFunktsioon12x_MaksimumJaMiinimum()
    {
        // f(x) = x³ − 12x: max (−2; 16); min (2; −16)
        var tulemus = ValemiTeenus.LeiaEkstreemid("x^3 - 12*x");

        Assert.Equal(2, tulemus.Count);
        Assert.True(tulemus[0].Max);
        Assert.Equal(-2, tulemus[0].X, 3);
        Assert.Equal(16, tulemus[0].Y, 3);
        Assert.False(tulemus[1].Max);
        Assert.Equal(2, tulemus[1].X, 3);
        Assert.Equal(-16, tulemus[1].Y, 3);
    }

    [Fact]
    public void Ekstreemumid_TestijaFunktsioon_KaksMiinimumiJaMaksimum()
    {
        // f(x) = x⁴ − 2x²: f'(x) = 4x(x − 1)(x + 1) = 0 -> x = −1; 0; 1
        // min (−1; −1), max (0; 0), min (1; −1)
        var tulemus = ValemiTeenus.LeiaEkstreemid("x^4 - 2*x^2");

        Assert.Equal(3, tulemus.Count);
        Assert.Equal((false, -1.0, -1.0), (tulemus[0].Max, tulemus[0].X, tulemus[0].Y));
        Assert.Equal((true, 0.0, 0.0), (tulemus[1].Max, tulemus[1].X, tulemus[1].Y));
        Assert.Equal((false, 1.0, -1.0), (tulemus[2].Max, tulemus[2].X, tulemus[2].Y));
    }

    [Fact]
    public void Ekstreemumid_MonotoonneFunktsioon_Puuduvad()
    {
        // f(x) = x³ + x on rangelt kasvav (f'(x) = 3x² + 1 > 0)
        Assert.Empty(ValemiTeenus.LeiaEkstreemid("x^3 + x"));
        Assert.Equal("puuduvad", ValemiTeenus.Analuusi("x^3 + x"));
    }

    [Fact]
    public void Analuusi_KuupFunktsioon_TekstVastabTabelile3()
    {
        // Andmebaasi salvestatav tekst peab sisaldama mõlemat ekstreemumit
        var tulemus = Abi.Ekstreemumid(ValemiTeenus.Analuusi("x^3 - 3*x"));

        Assert.Equal(new List<(bool, double, double)> { (true, -1, 2), (false, 1, -2) }, tulemus);
    }

    [Fact]
    public void Monotoonsus_RuutFunktsioon_KahanebEnne2JaKasvabParast()
    {
        // f(x) = x² − 4x + 3: kahaneb (−∞; 2), kasvab (2; ∞)
        var (kasvab, kahaneb) = ValemiTeenus.Monotoonsus("x^2 - 4*x + 3");

        Assert.Equal("(2; ∞)", kasvab);
        Assert.Equal("(−∞; 2)", kahaneb);
    }

    [Fact]
    public void Kumerus_KuupFunktsioon_KaanupunktNullis()
    {
        // f(x) = x³ − 3x, f''(x) = 6x: kumer (−∞; 0), nõgus (0; ∞), käänupunkt (0; 0)
        var (nogus, kumer, kaanu) = ValemiTeenus.Kumerus("x^3 - 3*x");

        Assert.Equal("(0; ∞)", nogus);
        Assert.Equal("(−∞; 0)", kumer);
        Assert.Equal("(0; 0)", kaanu);
    }
}
