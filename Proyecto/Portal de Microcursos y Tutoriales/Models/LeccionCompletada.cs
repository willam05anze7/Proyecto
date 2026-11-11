using System.ComponentModel.DataAnnotations;

namespace Portal_de_Microcursos_y_Tutoriales.Models
{
    /// <summary>
    /// Registra cuándo un usuario completa una lección.
    /// </summary>
    public class LeccionCompletada
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Fecha completada")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCompletada { get; set; } = DateTime.Now;

        // Clave foránea hacia ApplicationUser
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; } = string.Empty;

        // Propiedad de navegación
        public ApplicationUser? Usuario { get; set; }

        // Clave foránea hacia Leccion
        [Display(Name = "Lección")]
        public int LeccionId { get; set; }

        // Propiedad de navegación
        public Leccion? Leccion { get; set; }
    }
}
