using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portal_de_Microcursos_y_Tutoriales.Models;

namespace Portal_de_Microcursos_y_Tutoriales.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Leccion> Lecciones { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<LeccionCompletada> LeccionesCompletadas { get; set; }
    }
}
