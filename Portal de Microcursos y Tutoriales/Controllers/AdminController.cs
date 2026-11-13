using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal_de_Microcursos_y_Tutoriales.Data;
using Portal_de_Microcursos_y_Tutoriales.Models;

namespace Portal_de_Microcursos_y_Tutoriales.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCursos       = await _context.Cursos.CountAsync();
            ViewBag.CursosPublicados  = await _context.Cursos.CountAsync(c => c.EstaPublicado);
            ViewBag.TotalLecciones    = await _context.Lecciones.CountAsync();
            ViewBag.TotalInscritos    = await _context.Inscripciones.CountAsync();
            return View();
        }

        // ══════════════════════════════════════════════
        // CRUD CURSOS
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Lecciones)
                .Include(c => c.Inscripciones)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
            return View(cursos);
        }

        public IActionResult CrearCurso() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(
            [Bind("Titulo,Descripcion,Nivel,DuracionMinutos,EstaPublicado")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                curso.FechaCreacion = DateTime.Now;
                _context.Add(curso);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Curso creado correctamente.";
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        public async Task<IActionResult> EditarCurso(int? id)
        {
            if (id == null) return NotFound();
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id,
            [Bind("Id,Titulo,Descripcion,Nivel,DuracionMinutos,EstaPublicado,FechaCreacion")] Curso curso)
        {
            if (id != curso.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Curso actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cursos.Any(c => c.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        public async Task<IActionResult> EliminarCurso(int? id)
        {
            if (id == null) return NotFound();
            var curso = await _context.Cursos
                .Include(c => c.Lecciones)
                .Include(c => c.Inscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        [HttpPost, ActionName("EliminarCurso"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCursoConfirmado(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Curso eliminado correctamente.";
            }
            return RedirectToAction(nameof(Cursos));
        }

        // Publicar / Despublicar con un solo click
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublicacion(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            curso.EstaPublicado = !curso.EstaPublicado;
            await _context.SaveChangesAsync();
            TempData["Exito"] = curso.EstaPublicado
                ? $"'{curso.Titulo}' publicado."
                : $"'{curso.Titulo}' despublicado.";
            return RedirectToAction(nameof(Cursos));
        }

        // ══════════════════════════════════════════════
        // CRUD LECCIONES
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Lecciones(int cursoId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Lecciones.OrderBy(l => l.Orden))
                .FirstOrDefaultAsync(c => c.Id == cursoId);
            if (curso == null) return NotFound();
            ViewBag.Curso = curso;
            return View(curso.Lecciones.OrderBy(l => l.Orden).ToList());
        }

        public async Task<IActionResult> CrearLeccion(int cursoId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null) return NotFound();
            ViewBag.Curso = curso;

            // Sugerir el siguiente número de orden
            var maxOrden = await _context.Lecciones
                .Where(l => l.CursoId == cursoId)
                .MaxAsync(l => (int?)l.Orden) ?? 0;

            var leccion = new Leccion { CursoId = cursoId, Orden = maxOrden + 1 };
            return View(leccion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearLeccion(
            [Bind("Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leccion);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Lección creada correctamente.";
                return RedirectToAction(nameof(Lecciones), new { cursoId = leccion.CursoId });
            }
            ViewBag.Curso = await _context.Cursos.FindAsync(leccion.CursoId);
            return View(leccion);
        }

        public async Task<IActionResult> EditarLeccion(int? id)
        {
            if (id == null) return NotFound();
            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (leccion == null) return NotFound();
            ViewBag.Curso = leccion.Curso;
            return View(leccion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarLeccion(int id,
            [Bind("Id,Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (id != leccion.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leccion);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Lección actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Lecciones.Any(l => l.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Lecciones), new { cursoId = leccion.CursoId });
            }
            ViewBag.Curso = await _context.Cursos.FindAsync(leccion.CursoId);
            return View(leccion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLeccion(int id)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion != null)
            {
                int cursoId = leccion.CursoId;
                _context.Lecciones.Remove(leccion);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Lección eliminada correctamente.";
                return RedirectToAction(nameof(Lecciones), new { cursoId });
            }
            return RedirectToAction(nameof(Cursos));
        }

        // Subir / Bajar orden de lección
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoverLeccion(int id, string direccion)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null) return NotFound();

            var hermanas = await _context.Lecciones
                .Where(l => l.CursoId == leccion.CursoId)
                .OrderBy(l => l.Orden)
                .ToListAsync();

            int idx = hermanas.FindIndex(l => l.Id == id);

            if (direccion == "subir" && idx > 0)
            {
                var anterior = hermanas[idx - 1];
                (anterior.Orden, leccion.Orden) = (leccion.Orden, anterior.Orden);
            }
            else if (direccion == "bajar" && idx < hermanas.Count - 1)
            {
                var siguiente = hermanas[idx + 1];
                (siguiente.Orden, leccion.Orden) = (leccion.Orden, siguiente.Orden);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lecciones), new { cursoId = leccion.CursoId });
        }

        // ══════════════════════════════════════════════
        // INSCRITOS POR CURSO
        // ══════════════════════════════════════════════

        public async Task<IActionResult> Inscritos(int cursoId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null) return NotFound();
            ViewBag.Curso = curso;

            var inscritos = await _context.Inscripciones
                .Include(i => i.Usuario)
                .Where(i => i.CursoId == cursoId)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();

            return View(inscritos);
        }
    }
}
