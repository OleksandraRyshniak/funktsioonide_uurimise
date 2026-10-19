# Funktsioonide uurimise rakenduse ühiktestid

Vastastikuse testimise töö raames kirjutatud xUnit ühiktestid Oleksandra Ryshniaku funktsioonide uurimise veebirakendusele.

- **Testija:** Anastasiia Radasheva, TARpv24
- **Testitav töö:** https://github.com/OleksandraRyshniak/funktsioonide_uurimise
- **Testitud commit:** `71f62f1`

## Tehnoloogiad

- .NET 10
- xUnit 2.9.3
- Microsoft.NET.Test.Sdk 17.14.1

## Testide käivitamine

Repositooriumi juurkaustast:

```bash
dotnet test funktsioonide_uurimise.Tests
```

Visual Studios: ava `funktsioonide_uurimise.slnx` → **Test** → **Test Explorer** → **Run All**.

## Testklassid

| Fail | Mida kontrollib | Teste |
|---|---|---|
| `TuletiseTestid.cs` | Numbriline tuletis (`ValemiTeenus.D`), sümboolne tuletis (`SumboolneTeenus.Tuletis`), teine tuletis | 16 |
| `EkstreemumideTestid.cs` | Kriitilised punktid ja ekstreemumid (`LeiaEkstreemid`, `Analuusi`), monotoonsus, kumerus | 8 |
| `NullkohtadeTestid.cs` | Nullkohad, positiivsus- ja negatiivsuspiirkonnad (`Margiuuring`) | 6 |
| `ValemiArvutamiseTestid.cs` | NCalc arvutus, `^` → `Pow` teisendus, vigane valem, nulliga jagamine, logaritm, määramis- ja muutumispiirkond, paarsus, periood | 30 |
| `Abi.cs` | Abimeetodid rakenduse tekstiliste tulemuste lugemiseks | – |

## Kontrollväärtused

Oodatavad väärtused on võetud testimisjuhendi tabelist 3, lisaks testija valitud funktsioon x⁴ − 2x².

| Funktsioon | Tuletis | Nullkohad | Ekstreemumid |
|---|---|---|---|
| x² − 4x + 3 | 2x − 4 | x = 1; x = 3 | min (2; −1) |
| x³ − 3x | 3x² − 3 | x = 0; x = ±√3 | max (−1; 2); min (1; −2) |
| x³ − 12x | 3x² − 12 | x = 0; x = ±2√3 | max (−2; 16); min (2; −16) |
| x⁴ − 2x² | 4x³ − 4x | x = 0; x = ±√2 | min (±1; −1); max (0; 0) |

## Tulemus

**60 testist 59 läbivad, 1 kukub läbi.**

Läbi kukub `F_KumneAlusegaLogaritm_ArvutatakseOigesti`: kasutajaliidese nupp „log“ teisendatakse valemiks `Log(x)`, kuid NCalc funktsioon `Log` nõuab kahte argumenti (arv ja alus), mistõttu tagastatakse `NaN`. See on rakenduse viga (vearaport V-04), mitte testi viga.

## Märkus

Testprojekt asub rakenduse kausta sees. Selleks, et veebirakendus ei kompileeriks testide faile, on failis `funktsioonide_uurimise.csproj` testikaust kompileerimisest välja jäetud (`<Compile Remove="funktsioonide_uurimise.Tests\**" />`). Rakenduse koodi ei muudetud.
