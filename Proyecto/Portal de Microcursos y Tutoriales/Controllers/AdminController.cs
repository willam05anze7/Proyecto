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

        // ── Dashboard ──────────────────────────────────────────────────────────

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var totalCursos       = await _context.Cursos.CountAsync();
            var cursosPublicados  = await _context.Cursos.CountAsync(c => c.EstaPublicado);
            var totalLecciones    = await _context.Lecciones.CountAsync();
            var totalInscritos    = await _context.Inscripciones.CountAsync();

            ViewBag.TotalCursos      = totalCursos;
            ViewBag.CursosPublicados = cursosPublicados;
            ViewBag.TotalLecciones   = totalLecciones;
            ViewBag.TotalInscritos   = totalInscritos;

            var cursos = await _context.Cursos
                .Include(c => c.Lecciones)
                .Include(c => c.Inscripciones)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return View(cursos);
        }

        // ── CRUD Cursos ────────────────────────────────────────────────────────

        // GET: Admin/CrearCurso
        public IActionResult CrearCurso()
        {
            return View();
        }

        // POST: Admin/CrearCurso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(
            [Bind("Titulo,Descripcion,Nivel,DuracionMinutos,EstaPublicado")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                curso.FechaCreacion = DateTime.Now;
                _context.Add(curso);
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Curso \"{curso.Titulo}\" creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Admin/EditarCurso/5
        public async Task<IActionResult> EditarCurso(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: Admin/EditarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(
            int id,
            [Bind("Id,Titulo,Descripcion,Nivel,DuracionMinutos,EstaPublicado,FechaCreacion")] Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = $"Curso \"{curso.Titulo}\" actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cursos.Any(c => c.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Admin/EliminarCurso/5
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

        // POST: Admin/EliminarCurso/5
        [HttpPost, ActionName("EliminarCurso")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCursoConfirmado(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Curso \"{curso.Titulo}\" eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/TogglePublicacion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublicacion(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            curso.EstaPublicado = !curso.EstaPublicado;
            _context.Update(curso);
            await _context.SaveChangesAsync();

            TempData["Exito"] = curso.EstaPublicado
                ? $"Curso \"{curso.Titulo}\" publicado."
                : $"Curso \"{curso.Titulo}\" despublicado.";

            return RedirectToAction(nameof(Index));
        }

        // ── CRUD Lecciones ─────────────────────────────────────────────────────

        // GET: Admin/Lecciones/5  (lecciones de un curso)
        public async Task<IActionResult> Lecciones(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Lecciones.OrderBy(l => l.Orden))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            return View(curso);
        }

        // GET: Admin/CrearLeccion/5  (5 = CursoId)
        public async Task<IActionResult> CrearLeccion(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            // Sugerir el siguiente número de orden
            var maxOrden = await _context.Lecciones
                .Where(l => l.CursoId == id)
                .MaxAsync(l => (int?)l.Orden) ?? 0;

            ViewBag.Curso         = curso;
            ViewBag.OrdenSugerido = maxOrden + 1;

            return View();
        }

        // POST: Admin/CrearLeccion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearLeccion(
            [Bind("Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leccion);
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Lección \"{leccion.Titulo}\" creada.";
                return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
            }

            var curso = await _context.Cursos.FindAsync(leccion.CursoId);
            ViewBag.Curso = curso;
            return View(leccion);
        }

        // GET: Admin/EditarLeccion/5
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

        // POST: Admin/EditarLeccion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarLeccion(
            int id,
            [Bind("Id,Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (id != leccion.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leccion);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = $"Lección \"{leccion.Titulo}\" actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Lecciones.Any(l => l.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
            }

            var curso = await _context.Cursos.FindAsync(leccion.CursoId);
            ViewBag.Curso = curso;
            return View(leccion);
        }

        // GET: Admin/EliminarLeccion/5
        public async Task<IActionResult> EliminarLeccion(int? id)
        {
            if (id == null) return NotFound();

            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leccion == null) return NotFound();

            return View(leccion);
        }

        // POST: Admin/EliminarLeccion/5
        [HttpPost, ActionName("EliminarLeccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLeccionConfirmada(int id)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion != null)
            {
                int cursoId = leccion.CursoId;
                _context.Lecciones.Remove(leccion);
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Lección \"{leccion.Titulo}\" eliminada.";
                return RedirectToAction(nameof(Lecciones), new { id = cursoId });
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/MoverLeccion  (subir / bajar orden)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoverLeccion(int id, string direccion)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null) return NotFound();

            var hermanas = await _context.Lecciones
                .Where(l => l.CursoId == leccion.CursoId)
                .OrderBy(l => l.Orden)
                .ToListAsync();

            var idx = hermanas.FindIndex(l => l.Id == id);

            if (direccion == "subir" && idx > 0)
            {
                var anterior = hermanas[idx - 1];
                (leccion.Orden, anterior.Orden) = (anterior.Orden, leccion.Orden);
                _context.UpdateRange(leccion, anterior);
                await _context.SaveChangesAsync();
            }
            else if (direccion == "bajar" && idx < hermanas.Count - 1)
            {
                var siguiente = hermanas[idx + 1];
                (leccion.Orden, siguiente.Orden) = (siguiente.Orden, leccion.Orden);
                _context.UpdateRange(leccion, siguiente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
        }

        // ── Inscritos ──────────────────────────────────────────────────────────

        // GET: Admin/Inscritos/5
        public async Task<IActionResult> Inscritos(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Inscripciones)
                    .ThenInclude(i => i.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            return View(curso);
        }
    }
}
