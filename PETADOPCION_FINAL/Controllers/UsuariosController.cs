using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PETADOPCION_FINAL.Models;

namespace PETADOPCION_FINAL.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly PetadopcionContext _context;

        public UsuariosController(PetadopcionContext context)
        {
            _context = context;
        }
        // 🔒 VALIDACIONES
        private bool EsAdmin()
        {
            return HttpContext.Session.GetInt32("Rol") == 1;
        }

        private bool EstaLogueado()
        {
            return HttpContext.Session.GetString("UsuarioLogueado") != null;
        }
        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (!EstaLogueado())
                return RedirectToAction("Index", "Home");

            if (!EsAdmin())
                return RedirectToAction("Principal", "Home");

            var petadopcionContext = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Include(u => u.Mascota)
                    .ThenInclude(m => m.SolicitudesAdopcions) // ESTO es para las RECIBIDAS
                .Include(u => u.Adopciones)                   // Mascotas que él adoptó
                .Include(u => u.SolicitudesAdopcions);        // Solicitudes que él ENVIÓ

            return View(await petadopcionContext.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!EstaLogueado())
                return RedirectToAction("Index", "Home");

            if (!EsAdmin())
                return RedirectToAction("Principal", "Home");
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
      
        public IActionResult Create()
        {
          
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol");
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create([Bind("IdUsuario,IdRol,Nombre,Apellido,Email,PasswordHash,Telefono,Direccion,Ciudad")] Usuario usuario)
        {
            
            // 1. Limpiamos las validaciones de navegación que no vienen del form
            ModelState.Remove("IdRolNavigation");
            ModelState.Remove("Adopciones");
            ModelState.Remove("DocumentosUsuarios");
            ModelState.Remove("Mascota");
            ModelState.Remove("SolicitudesAdopcions");

            // 2. VALIDACIÓN DEFINITIVA: ¿Existe ya el correo?
            bool existeCorreo = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);

            if (existeCorreo)
            {
                // Agregamos un error manual al campo Email para que la vista lo muestre
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.FechaRegistro = DateTime.Now;
                    usuario.Estado = true;
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
                    _context.Add(usuario);
                    await _context.SaveChangesAsync();

                    TempData["Exito"] = "Usuario creado con éxito";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error inesperado al guardar: " + ex.Message);
                }
            }

            // Si llegamos aquí es porque hubo error (de duplicado o de otro tipo)
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "IdRol", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,IdRol,Nombre,Apellido,Email,PasswordHash,Telefono,Direccion,Ciudad,FechaRegistro,Estado")] Usuario usuario)
        {
           
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            // 1. ELIMINAR validaciones de navegación para que ModelState sea True
            ModelState.Remove("IdRolNavigation");
            ModelState.Remove("Adopciones");
            ModelState.Remove("DocumentosUsuarios");
            ModelState.Remove("Mascota");
            ModelState.Remove("SolicitudesAdopcions");

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. ACTUALIZACIÓN: EF se encarga de mapear los cambios
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
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

            // Si hay error, recargamos el select de roles
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }
        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!EstaLogueado())
                return RedirectToAction("Index", "Home");

            if (!EsAdmin())
                return RedirectToAction("Principal", "Home");
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int IdUsuario)
        {
            if (!EstaLogueado())
                return RedirectToAction("Index", "Home");

            if (!EsAdmin())
                return RedirectToAction("Principal", "Home");
            // 1. Cargamos al usuario con TODA la descendencia (Fotos, Solicitudes, Adopciones de sus Mascotas)
            var usuario = await _context.Usuarios
                .Include(u => u.Adopciones)
                .Include(u => u.DocumentosUsuarios)
                .Include(u => u.SolicitudesAdopcions)
                .Include(u => u.Mascota)
                    .ThenInclude(m => m.MascotaFotos)
                .FirstOrDefaultAsync(u => u.IdUsuario == IdUsuario);

            if (usuario == null) return RedirectToAction(nameof(Index));

            try
            {
                // 2. NIVEL 1: Limpiar todo lo que dependa de las MASCOTAS del usuario
                foreach (var m in usuario.Mascota)
                {
                    // Borrar fotos de la mascota
                    if (m.MascotaFotos != null) _context.RemoveRange(m.MascotaFotos);

                    // Borrar solicitudes de adopción de esa mascota (Error anterior)
                    var solicitudesMascota = _context.SolicitudesAdopcions.Where(s => s.IdMascota == m.IdMascota);
                    _context.SolicitudesAdopcions.RemoveRange(solicitudesMascota);

                    // Borrar registros de Adopciones donde esté esta mascota (EL ERROR ACTUAL)
                    var adopcionesMascota = _context.Adopciones.Where(a => a.IdMascota == m.IdMascota);
                    _context.Adopciones.RemoveRange(adopcionesMascota);
                }

                // 3. NIVEL 2: Limpiar lo que dependa directamente del USUARIO
                if (usuario.Adopciones.Any()) _context.Adopciones.RemoveRange(usuario.Adopciones);
                if (usuario.DocumentosUsuarios.Any()) _context.DocumentosUsuarios.RemoveRange(usuario.DocumentosUsuarios);
                if (usuario.SolicitudesAdopcions.Any()) _context.SolicitudesAdopcions.RemoveRange(usuario.SolicitudesAdopcions);

                // 4. NIVEL 3: Borrar Mascotas y luego al Usuario
                if (usuario.Mascota.Any()) _context.Mascotas.RemoveRange(usuario.Mascota);
                _context.Usuarios.Remove(usuario);

                // 5. GUARDADO DEFINITIVO
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Si sale otro error, dime el nombre de la tabla que menciona
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("ERROR DE TABLA RESTANTE: " + msg);
            }
        }
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
