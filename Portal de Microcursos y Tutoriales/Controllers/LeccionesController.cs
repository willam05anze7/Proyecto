using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portal_de_Microcursos_y_Tutoriales.Data;
using Portal_de_Microcursos_y_Tutoriales.Models;

namespace Portal_de_Microcursos_y_Tutoriales.Controllers
{
    public class LeccionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeccionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lecciones
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Lecciones.Include(l => l.Curso);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Lecciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leccion == null)
            {
                return NotFound();
            }

            return View(leccion);
        }

        // GET: Lecciones/Create
        public IActionResult Create()
        {
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel");
            return View();
        }

        // POST: Lecciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel", leccion.CursoId);
            return View(leccion);
        }

        // GET: Lecciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null)
            {
                return NotFound();
            }
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel", leccion.CursoId);
            return View(leccion);
        }

        // POST: Lecciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (id != leccion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeccionExists(leccion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel", leccion.CursoId);
            return View(leccion);
        }

        // GET: Lecciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leccion == null)
            {
                return NotFound();
            }

            return View(leccion);
        }

        // POST: Lecciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion != null)
            {
                _context.Lecciones.Remove(leccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeccionExists(int id)
        {
            return _context.Lecciones.Any(e => e.Id == id);
        }
    }
}
