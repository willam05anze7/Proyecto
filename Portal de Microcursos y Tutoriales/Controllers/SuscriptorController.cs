using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal_de_Microcursos_y_Tutoriales.Data;
using Portal_de_Microcursos_y_Tutoriales.Models;

namespace Portal_de_Microcursos_y_Tutoriales.Controllers
{
    [Authorize(Roles = "Suscriptor,Administrador")]
    public class SuscriptorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuscriptorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context     = context;
            _userManager = userManager;
        }

        // ══════════════════════════════════════════════
        // MIS CURSOS
        // ══════════════════════════════════════════════

        public async Task<IActionResult> MisCursos()
        {
            var userId = _userManager.GetUserId(User)!;

            var inscripciones = await _context.Inscripciones
                .Include(i => i.Curso)
                    .ThenInclude(c => c!.Lecciones)
                .Where(i => i.UsuarioId == userId)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();

            // Calcular progreso por curso
            var leccionesCompletadas = await _context.LeccionesCompletadas
                .Where(lc => lc.UsuarioId == userId)
                .Select(lc => lc.LeccionId)
                .ToListAsync();

            ViewBag.LeccionesCompletadas = leccionesCompletadas;
            return View(inscripciones);
        }

        // ══════════════════════════════════════════════
        // INSCRIBIRSE EN UN CURSO
        // ══════════════════════════════════════════════

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var userId = _userManager.GetUserId(User)!;

            // Verificar que el curso existe y está publicado
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.EstaPublicado)
                return NotFound();

            // Verificar que no esté ya inscrito
            bool yaInscrito = await _context.Inscripciones
                .AnyAsync(i => i.UsuarioId == userId && i.CursoId == cursoId);

            if (!yaInscrito)
            {
                _context.Inscripciones.Add(new Inscripcion
                {
                    UsuarioId        = userId,
                    CursoId          = cursoId,
                    FechaInscripcion = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Te inscribiste en '{curso.Titulo}'.";
            }
            else
            {
                TempData["Info"] = "Ya estás inscrito en este curso.";
            }

            return RedirectToAction(nameof(VerCurso), new { cursoId });
        }

        // ══════════════════════════════════════════════
        // VER LECCIONES DEL CURSO
        // ══════════════════════════════════════════════

        public async Task<IActionResult> VerCurso(int cursoId)
        {
            var userId = _userManager.GetUserId(User)!;

            var curso = await _context.Cursos
                .Include(c => c.Lecciones.OrderBy(l => l.Orden))
                .FirstOrDefaultAsync(c => c.Id == cursoId && c.EstaPublicado);

            if (curso == null) return NotFound();

            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.UsuarioId == userId && i.CursoId == cursoId);

            var completadas = await _context.LeccionesCompletadas
                .Where(lc => lc.UsuarioId == userId &&
                             lc.Leccion!.CursoId == cursoId)
                .Select(lc => lc.LeccionId)
                .ToListAsync();

            int total    = curso.Lecciones.Count;
            int hechas   = completadas.Count;
            int porcentaje = total > 0 ? (int)Math.Round((double)hechas / total * 100) : 0;

            ViewBag.Inscripcion         = inscripcion;
            ViewBag.LeccionesCompletadas = completadas;
            ViewBag.Porcentaje          = porcentaje;
            ViewBag.Total               = total;
            ViewBag.Hechas              = hechas;

            return View(curso);
        }

        // ══════════════════════════════════════════════
        // MARCAR / DESMARCAR LECCIÓN COMO COMPLETADA
        // ══════════════════════════════════════════════

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLeccion(int leccionId, int cursoId)
        {
            var userId = _userManager.GetUserId(User)!;

            // Verificar que el usuario esté inscrito
            bool inscrito = await _context.Inscripciones
                .AnyAsync(i => i.UsuarioId == userId && i.CursoId == cursoId);
            if (!inscrito) return Forbid();

            var completada = await _context.LeccionesCompletadas
                .FirstOrDefaultAsync(lc => lc.UsuarioId == userId && lc.LeccionId == leccionId);

            if (completada == null)
            {
                _context.LeccionesCompletadas.Add(new LeccionCompletada
                {
                    UsuarioId       = userId,
                    LeccionId       = leccionId,
                    FechaCompletada = DateTime.Now
                });
            }
            else
            {
                _context.LeccionesCompletadas.Remove(completada);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(VerCurso), new { cursoId });
        }
    }
}
