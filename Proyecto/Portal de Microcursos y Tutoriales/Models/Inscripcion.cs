using System.ComponentModel.DataAnnotations;

namespace Portal_de_Microcursos_y_Tutoriales.Models
{
    /// <summary>
    /// Representa la inscripción de un usuario en un curso.
    /// </summary>
    public class Inscripcion
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Fecha de inscripción")]
        [DataType(DataType.DateTime)]
        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        // Clave foránea hacia ApplicationUser
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; } = string.Empty;

        // Propiedad de navegación
        public ApplicationUser? Usuario { get; set; }

        // Clave foránea hacia Curso
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        // Propiedad de navegación
        public Curso? Curso { get; set; }
    }
}
