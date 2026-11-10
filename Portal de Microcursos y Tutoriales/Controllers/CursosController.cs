using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal_de_Microcursos_y_Tutoriales.Data;
using Portal_de_Microcursos_y_Tutoriales.Models;

namespace Portal_de_Microcursos_y_Tutoriales.Controllers
{
    // Sin [Authorize]: cualquier visitante puede ver el catálogo público
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cursos — solo muestra cursos publicados
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos
                .Where(c => c.EstaPublicado)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return View(cursos);
        }

        // GET: Cursos/Details/5 — solo permite ver cursos publicados
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Lecciones)
                .FirstOrDefaultAsync(c => c.Id == id && c.EstaPublicado);

            if (curso == null)
                return NotFound();

            return View(curso);
        }
    }
}
