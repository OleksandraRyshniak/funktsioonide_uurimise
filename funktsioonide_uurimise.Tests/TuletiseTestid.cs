using funktsioonide_uurimise.Models;

namespace funktsioonide_uurimise.Tests;

// Tuletise kontroll (juhendi tabel 3). Rakendus arvutab tuletise kahel viisil:
// numbriliselt (ValemiTeenus.D, keskdiferents) ja sümboolselt (SumboolneTeenus.Tuletis, MathNet.Symbolics).
public class TuletiseTestid
{
    [Fact]
    public void Tuletis_KuupFunktsioon_TagastabOigeVaartuse()
    {
        // f(x) = x^3 - 3x, seega f'(2) = 3 * 2^2 - 3 = 9
        double tulemus = ValemiTeenus.D("x^3 - 3*x", 2);

        Assert.Equal(9, tulemus, 3); // täpsus 3 komakohta
    }

    [Theory]
    // f(x) = x² − 4x + 3, f'(x) = 2x − 4
    [InlineData("x^2 - 4*x + 3", 0, -4)]
    [InlineData("x^2 - 4*x + 3", 2, 0)]
    [InlineData("x^2 - 4*x + 3", 5, 6)]
    // f(x) = x³ − 3x, f'(x) = 3x² − 3
    [InlineData("x^3 - 3*x", -1, 0)]
    [InlineData("x^3 - 3*x", 0, -3)]
    [InlineData("x^3 - 3*x", 1, 0)]
    // f(x) = x³ − 12x, f'(x) = 3x² − 12
    [InlineData("x^3 - 12*x", -2, 0)]
    [InlineData("x^3 - 12*x", 0, -12)]
    [InlineData("x^3 - 12*x", 3, 15)]
    // Testija valitud funktsioon: f(x) = x⁴ − 2x², f'(x) = 4x³ − 4x
    [InlineData("x^4 - 2*x^2", 2, 24)]
    [InlineData("x^4 - 2*x^2", -1, 0)]
    public void Tuletis_KontrollFunktsioonid_VastabKasitsiArvutusele(string valem, double x, double oodatav)
    {
        double tulemus = ValemiTeenus.D(valem, x);

        Assert.Equal(oodatav, tulemus, 3);
    }

    [Theory]
    [InlineData("x^2 - 4*x + 3", -4, -2)]   // f'(x) = 2x − 4:   f'(0) = −4,  f'(1) = −2
    [InlineData("x^3 - 3*x", -3, 0)]        // f'(x) = 3x² − 3:  f'(0) = −3,  f'(1) = 0
    [InlineData("x^3 - 12*x", -12, -9)]     // f'(x) = 3x² − 12: f'(0) = −12, f'(1) = −9
    public void SumboolneTuletis_KontrollFunktsioonid_AnnabOigeAvaldise(string valem, double fd0, double fd1)
    {
        string tuletis = SumboolneTeenus.Tuletis(valem);

        // MathNet.Symbolics võib liikmete järjekorda muuta (nt "-4 + 2*x"), seega
        // kontrollitakse tagastatud avaldist selle väärtuste kaudu punktides 0 ja 1.
        Assert.NotEqual("arvutatakse numbriliselt", tuletis);
        Assert.Equal(fd0, ValemiTeenus.F(tuletis, 0), 6);
        Assert.Equal(fd1, ValemiTeenus.F(tuletis, 1), 6);
    }

    [Fact]
    public void TeineTuletis_KuupFunktsioon_On6x()
    {
        // f(x) = x³ − 3x, f''(x) = 6x, f''(2) = 12
        Assert.Equal(12, ValemiTeenus.Teine("x^3 - 3*x", 2), 2);
        Assert.Equal(-6, ValemiTeenus.Teine("x^3 - 3*x", -1), 2);
    }
}
