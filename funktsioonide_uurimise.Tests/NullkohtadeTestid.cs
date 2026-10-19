using funktsioonide_uurimise.Models;

namespace funktsioonide_uurimise.Tests;

// Nullkohtade ning positiivsus- ja negatiivsuspiirkondade leidmine (ValemiTeenus.Margiuuring).
public class NullkohtadeTestid
{
    [Fact]
    public void Nullkohad_RuutFunktsioon_X1JaX3()
    {
        // x² − 4x + 3 = (x − 1)(x − 3) = 0 -> x = 1; x = 3
        var nullid = Abi.Nullkohad(ValemiTeenus.Margiuuring("x^2 - 4*x + 3").Nullid);

        Assert.Equal(new List<double> { 1, 3 }, nullid);
    }

    [Fact]
    public void Nullkohad_KuupFunktsioon_NullJaPlussMiinusJuur3()
    {
        // x³ − 3x = x(x² − 3) = 0 -> x = 0; x = ±√3 ≈ ±1.7321
        var nullid = Abi.Nullkohad(ValemiTeenus.Margiuuring("x^3 - 3*x").Nullid);

        Assert.Equal(3, nullid.Count);
        Assert.Equal(-Math.Sqrt(3), nullid[0], 3);
        Assert.Equal(0, nullid[1], 3);
        Assert.Equal(Math.Sqrt(3), nullid[2], 3);
    }

    [Fact]
    public void Nullkohad_KuupFunktsioon12x_NullJaPlussMiinus2Juur3()
    {
        // x³ − 12x = x(x² − 12) = 0 -> x = 0; x = ±2√3 ≈ ±3.4641
        var nullid = Abi.Nullkohad(ValemiTeenus.Margiuuring("x^3 - 12*x").Nullid);

        Assert.Equal(3, nullid.Count);
        Assert.Equal(-2 * Math.Sqrt(3), nullid[0], 3);
        Assert.Equal(0, nullid[1], 3);
        Assert.Equal(2 * Math.Sqrt(3), nullid[2], 3);
    }

    [Fact]
    public void Nullkohad_TestijaFunktsioon_KahekordneNullkoht()
    {
        // x⁴ − 2x² = x²(x² − 2) = 0 -> x = 0 (kahekordne, märk ei muutu); x = ±√2
        var nullid = Abi.Nullkohad(ValemiTeenus.Margiuuring("x^4 - 2*x^2").Nullid);

        Assert.Equal(3, nullid.Count);
        Assert.Equal(-Math.Sqrt(2), nullid[0], 3);
        Assert.Equal(0, nullid[1], 3);
        Assert.Equal(Math.Sqrt(2), nullid[2], 3);
    }

    [Fact]
    public void Nullkohad_ParaboolÜlevalpool_Puuduvad()
    {
        // x² + 1 > 0 kõigil x
        var (nullid, positiivsed, negatiivsed) = ValemiTeenus.Margiuuring("x^2 + 1");

        Assert.Equal("puuduvad", nullid);
        Assert.Equal("(−∞; ∞)", positiivsed);
        Assert.Equal("puuduvad", negatiivsed);
    }

    [Fact]
    public void Margiuuring_RuutFunktsioon_OigedVahemikud()
    {
        // x² − 4x + 3 > 0, kui x ∈ (−∞; 1) ∪ (3; ∞); < 0, kui x ∈ (1; 3)
        var (_, positiivsed, negatiivsed) = ValemiTeenus.Margiuuring("x^2 - 4*x + 3");

        Assert.Equal("(−∞; 1) ∪ (3; ∞)", positiivsed);
        Assert.Equal("(1; 3)", negatiivsed);
    }
}
