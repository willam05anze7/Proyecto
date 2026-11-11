using System.ComponentModel.DataAnnotations;

namespace Portal_de_Microcursos_y_Tutoriales.Models
{
    /// <summary>
    /// Representa un microcurso o tutorial del portal.
    /// </summary>
    public class Curso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La {0} no puede exceder los {1} caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel es obligatorio")]
        [StringLength(50, ErrorMessage = "El {0} no puede exceder los {1} caracteres")]
        [Display(Name = "Nivel")]
        public string Nivel { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La duración debe estar entre {1} y {2} minutos")]
        [Display(Name = "Duración (minutos)")]
        public int DuracionMinutos { get; set; }

        [Display(Name = "¿Está publicado?")]
        public bool EstaPublicado { get; set; }

        [Display(Name = "Fecha de creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        public ICollection<Leccion> Lecciones { get; set; } = [];
        public ICollection<Inscripcion> Inscripciones { get; set; } = [];
    }
}
