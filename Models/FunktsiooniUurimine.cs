using System.ComponentModel.DataAnnotations;

namespace funktsioonide_uurimise.Models;

public class FunktsiooniUurimine
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Valem on kohustuslik")]
    [Display(Name = "Valem")]
    public string Valem { get; set; } = string.Empty;

    [Display(Name = "Määramispiirkond")]
    public string Maaramispiirkond { get; set; } = string.Empty;

    [Display(Name = "Tuletis f'(x)")]
    public string Tuletis { get; set; } = string.Empty;

    [Display(Name = "Nullkohad f(x) = 0")]
    public string Nullkohad { get; set; } = string.Empty;

    [Display(Name = "Positiivsuspiirkond f(x) > 0")]
    public string Positiivsus { get; set; } = string.Empty;

    [Display(Name = "Negatiivsuspiirkond f(x) < 0")]
    public string Negatiivsus { get; set; } = string.Empty;

    [Display(Name = "Ekstreemumid")]
    public string Ekstreemumid { get; set; } = string.Empty;

    [Display(Name = "Loodud")]
    public DateTime LuodudAeg { get; set; } = DateTime.UtcNow;
}