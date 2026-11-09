using System.ComponentModel.DataAnnotations;

namespace Portal_de_Microcursos_y_Tutoriales.Models
{
    /// <summary>
    /// Representa una lección que pertenece a un curso.
    /// </summary>
    public class Leccion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Contenido")]
        public string Contenido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El orden es obligatorio")]
        [Range(1, 1000, ErrorMessage = "El {0} debe estar entre {1} y {2}")]
        [Display(Name = "Orden")]
        public int Orden { get; set; }

        // Clave foránea
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        // Propiedad de navegación
        public Curso? Curso { get; set; }

        // Propiedades de navegación
        public ICollection<LeccionCompletada> LeccionesCompletadas { get; set; } = new List<LeccionCompletada>();
    }
}
