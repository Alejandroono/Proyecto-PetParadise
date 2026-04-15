using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PETADOPCION_FINAL.Models;

namespace PETADOPCION_FINAL.Controllers
{
    public class AdopcionesController : Controller
    {
        private readonly PetadopcionContext _context;

        public AdopcionesController(PetadopcionContext context)
        {
            _context = context;
        }

        // GET: Adopciones
        public async Task<IActionResult> Index()
        {
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");
            int? idRol = HttpContext.Session.GetInt32("IdRol");

            if (idUsuarioLogueado == null) return RedirectToAction("Index", "Home");

            // 1. Iniciamos la consulta con los Include para mostrar fotos, nombres de mascota y adoptante
            var query = _context.Adopciones
                .Include(a => a.IdMascotaNavigation)
                    .ThenInclude(m => m.MascotaFotos)
                .Include(a => a.IdAdoptanteNavigation)
                    .ThenInclude(u => u.DocumentosUsuarios)
                .AsQueryable();

            // 2. LÓGICA DE FILTRADO MEJORADA:
            if (idRol != 1) // Si NO es administrador
            {
                // El usuario ve la adopción SI:
                // Fue el que la publicó (entregó) O fue el que la adoptó
                query = query.Where(a =>
                    a.IdMascotaNavigation.IdUsuarioPublicador == idUsuarioLogueado ||
                    a.IdAdoptante == idUsuarioLogueado);
            }
            // Si es admin (Rol == 1), la query queda sin filtros y ve TODO.

            // 3. Ejecución
            var misAdopciones = await query
                .OrderByDescending(a => a.FechaAdopcion)
                .ToListAsync();

            return View(misAdopciones);
        }






        private bool AdopcioneExists(int id)
        {
            return _context.Adopciones.Any(e => e.IdAdopcion == id);
        }
    }
}
