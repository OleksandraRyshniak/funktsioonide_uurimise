using System.ComponentModel.DataAnnotations;

namespace funktsioonide_uurimise.Models;

public class FunktsiooniUurimine
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Valem on kohustuslik")]
    [Display(Name = "Valem")]
    public string Valem { get; set; } = string.Empty;

    [Display(Name = "Määramispiirkond X")]
    public string Maaramispiirkond { get; set; } = string.Empty;

    [Display(Name = "Muutumispiirkond Y")]
    public string Muutumispiirkond { get; set; } = string.Empty;

    [Display(Name = "Tuletis f'(x)")]
    public string Tuletis { get; set; } = string.Empty;

    [Display(Name = "Nullkohad f(x) = 0")]
    public string Nullkohad { get; set; } = string.Empty;

    [Display(Name = "Positiivsuspiirkond f(x) > 0")]
    public string Positiivsus { get; set; } = string.Empty;

    [Display(Name = "Negatiivsuspiirkond f(x) < 0")]
    public string Negatiivsus { get; set; } = string.Empty;

    [Display(Name = "Kasvamispiirkond X↑")]
    public string Kasvamine { get; set; } = string.Empty;

    [Display(Name = "Kahanemispiirkond X↓")]
    public string Kahanemine { get; set; } = string.Empty;

    [Display(Name = "Ekstreemumid")]
    public string Ekstreemumid { get; set; } = string.Empty;

    [Display(Name = "Nõgususvahemikud f''(x) > 0")]
    public string Nogusus { get; set; } = string.Empty;

    [Display(Name = "Kumerusvahemikud f''(x) < 0")]
    public string Kumerus { get; set; } = string.Empty;

    [Display(Name = "Käänupunktid")]
    public string Kaanupunktid { get; set; } = string.Empty;

    [Display(Name = "Paarsus")]
    public string Paarsus { get; set; } = string.Empty;

    [Display(Name = "Periood T")]
    public string Periood { get; set; } = string.Empty;

    [Display(Name = "Loodud")]
    public DateTime LuodudAeg { get; set; } = DateTime.UtcNow;
}