
using funktsioonide_uurimise.Models;
using Microsoft.EntityFrameworkCore;

namespace funktsioonide_uurimise.Models
{
    public class AndmeKontekst : DbContext
    {
        public AndmeKontekst(DbContextOptions<AndmeKontekst> valikud) : base(valikud) { }
        public DbSet<FunktsiooniUurimine> FunktsiooniUurimised => Set<FunktsiooniUurimine>();
    }
}
