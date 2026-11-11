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
    public class InscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscripcionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inscripciones
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Inscripciones.Include(i => i.Curso).Include(i => i.Usuario);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Inscripciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await _context.Inscripciones
                .Include(i => i.Curso)
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inscripcion == null)
            {
                return NotFound();
            }

            return View(inscripcion);
        }

        // GET: Inscripciones/Create
        public IActionResult Create()
        {
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel");
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Inscripciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FechaInscripcion,UsuarioId,CursoId")] Inscripcion inscripcion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inscripcion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel", inscripcion.CursoId);
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Email", inscripcion.UsuarioId);
            return View(inscripcion);
        }

        // GET: Inscripciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await _context.Inscripciones.FindAsync(id);
            if (inscripcion == null)
            {
                return NotFound();
            }
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel", inscripcion.CursoId);
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Email", inscripcion.UsuarioId);
            return View(inscripcion);
        }

        // POST: Inscripciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaInscripcion,UsuarioId,CursoId")] Inscripcion inscripcion)
        {
            if (id != inscripcion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inscripcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InscripcionExists(inscripcion.Id))
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
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nivel", inscripcion.CursoId);
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Email", inscripcion.UsuarioId);
            return View(inscripcion);
        }

        // GET: Inscripciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await _context.Inscripciones
                .Include(i => i.Curso)
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inscripcion == null)
            {
                return NotFound();
            }

            return View(inscripcion);
        }

        // POST: Inscripciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);
            if (inscripcion != null)
            {
                _context.Inscripciones.Remove(inscripcion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InscripcionExists(int id)
        {
            return _context.Inscripciones.Any(e => e.Id == id);
        }
    }
}
