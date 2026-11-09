using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Portal_de_Microcursos_y_Tutoriales.Models
{
    /// <summary>
    /// Representa a un usuario del portal. Hereda de IdentityUser
    /// y agrega el nombre completo del usuario.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        // Propiedades de navegación
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public ICollection<LeccionCompletada> LeccionesCompletadas { get; set; } = new List<LeccionCompletada>();
    }
}
