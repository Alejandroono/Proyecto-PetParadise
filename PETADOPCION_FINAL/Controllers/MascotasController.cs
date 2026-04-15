using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PETADOPCION_FINAL.Models;

namespace PETADOPCION_FINAL.Controllers
{
    public class MascotasController : Controller
    {
        private readonly PetadopcionContext _context;
        private bool EsAdmin()
        {
            return HttpContext.Session.GetInt32("Rol") == 1;
        }

        private bool EstaLogueado()
        {
            return HttpContext.Session.GetString("UsuarioLogueado") != null;
        }
        private readonly IConfiguration _config;

        public MascotasController(PetadopcionContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<IActionResult> MisPublicaciones()
        {
            // Obtener el ID del usuario desde la sesión
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioLogueado == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Filtrar solo las mascotas que pertenecen a ese ID
            var misMascotas = await _context.Mascotas
                .Include(m => m.MascotaFotos)
                .Where(m => m.IdUsuarioPublicador == idUsuarioLogueado)
                .ToListAsync();

            return View(misMascotas);
        }
        // GET: Mascotas
        public async Task<IActionResult> Index()
        {
            if (!EstaLogueado())
                return RedirectToAction("Index", "Home");

            if (!EsAdmin())
                return RedirectToAction("Principal", "Home");
            // 1. VERIFICACIÓN DE SEGURIDAD
           

            // 2. Si hay sesión, procedemos con la carga normal
            var mascotas = _context.Mascotas
                .Include(m => m.MascotaFotos)
                .Include(m => m.IdUsuarioPublicadorNavigation);

            return View(await mascotas.ToListAsync());
        }

        // GET: Mascotas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var mascota = await _context.Mascotas
                .Include(m => m.IdUsuarioPublicadorNavigation)
                .Include(m => m.MascotaFotos)
                .FirstOrDefaultAsync(m => m.IdMascota == id);
            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // GET: Mascotas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mascotas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Tipo,Raza,Sexo,EstadoSalud,Descripcion,Ubicacion,EstadoAdopcion")] Mascota mascota, IFormFile imagenMascota, int edadAnos, int edadMeses, int edadDias)
        {
            // Tomamos el usuario de la sesión
            int? userId = HttpContext.Session.GetInt32("IdUsuario");
            if (userId == null) return RedirectToAction("Index", "Home");

            // Calculamos la edad total en días para el campo int Edad
            mascota.Edad = (edadAnos * 365) + (edadMeses * 30) + edadDias;
            mascota.IdUsuarioPublicador = userId.Value;
            mascota.FechaPublicacion = DateTime.Now;

            // Limpiamos validaciones de campos automáticos
            ModelState.Remove("IdUsuarioPublicador");
            ModelState.Remove("FechaPublicacion");
            ModelState.Remove("IdUsuarioPublicadorNavigation");
            ModelState.Remove("MascotaFotos");
            ModelState.Remove("Adopciones");
            ModelState.Remove("SolicitudesAdopcions");

            if (ModelState.IsValid)
            {
                _context.Add(mascota);
                await _context.SaveChangesAsync();

                if (imagenMascota != null && imagenMascota.Length > 0)
                {
                    string connectionString = _config.GetConnectionString("AzureStorage");
                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("archivos-petadopcion");

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagenMascota.FileName)}";
                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = imagenMascota.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders { ContentType = imagenMascota.ContentType }
                        });
                    }

                    var fotoMascota = new MascotaFoto
                    {
                        IdMascota = mascota.IdMascota,
                        UrlFoto = blobClient.Uri.ToString(),
                        EsPrincipal = true
                    };
                    _context.MascotaFotos.Add(fotoMascota);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mascota);
        }

        // GET: Mascotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Buscamos la mascota
            var mascota = await _context.Mascotas.FindAsync(id);

            if (mascota == null) return NotFound();

            // SEGURIDAD: Verificar que el usuario que intenta editar sea el dueño
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");
            if (mascota.IdUsuarioPublicador != idUsuarioLogueado)
            {
                return Forbid(); // O redirecciona a una página de error de acceso
            }

            return View(mascota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota) return NotFound();

            try
            {
                // 1. Buscamos la mascota REAL que está en la base de datos ahora mismo
                // Usamos AsNoTracking para que no choque con el objeto que vamos a actualizar
                var mascotaOriginal = await _context.Mascotas.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdMascota == id);

                if (mascotaOriginal == null) return NotFound();

                // 2. SEGURIDAD: Le devolvemos a "mascota" los valores originales que NO se deben tocar
                // Esto asegura que aunque el usuario mande basura en el form, el sistema use lo real
                mascota.IdUsuarioPublicador = mascotaOriginal.IdUsuarioPublicador;
                mascota.EstadoAdopcion = mascotaOriginal.EstadoAdopcion;
                mascota.FechaPublicacion = mascotaOriginal.FechaPublicacion;

                // 3. Actualizamos
                _context.Mascotas.Update(mascota);

                // 4. Doble candado: Le decimos a EF que ignore estos campos en el SQL final
                _context.Entry(mascota).Property(x => x.EstadoAdopcion).IsModified = false;
                _context.Entry(mascota).Property(x => x.IdUsuarioPublicador).IsModified = false;
                _context.Entry(mascota).Property(x => x.FechaPublicacion).IsModified = false;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MisPublicaciones));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR AL GUARDAR: " + ex.Message);
                // Si hay error, recargamos la vista
                return View(mascota);
            }
        }
        // GET: Mascotas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var mascota = await _context.Mascotas
                .Include(m => m.IdUsuarioPublicadorNavigation)
                .FirstOrDefaultAsync(m => m.IdMascota == id);
            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // POST: Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Buscamos la mascota incluyendo todas sus relaciones para poder borrarlas también
            var mascota = await _context.Mascotas
                .Include(m => m.Adopciones)
                .Include(m => m.MascotaFotos)
                .Include(m => m.SolicitudesAdopcions)
                .FirstOrDefaultAsync(m => m.IdMascota == id);

            if (mascota != null)
            {
                // 2. Limpieza de tablas relacionadas (obligatorio para que no salte el error de SQL)
                // Borramos los registros en Adopciones, Fotos y Solicitudes que usan ese IdMascota
                if (mascota.Adopciones.Any()) _context.Adopciones.RemoveRange(mascota.Adopciones);
                if (mascota.MascotaFotos.Any()) _context.MascotaFotos.RemoveRange(mascota.MascotaFotos);
                if (mascota.SolicitudesAdopcions.Any()) _context.SolicitudesAdopcions.RemoveRange(mascota.SolicitudesAdopcions);

                // 3. Ahora que el camino está limpio, borramos la mascota de la tabla principal
                _context.Mascotas.Remove(mascota);

                // 4. Ejecutamos el cambio final en la base de datos
                await _context.SaveChangesAsync();
            }

            // 5. Te manda de vuelta a la lista y la mascota ya NO existe en el SQL
            return Content(@"
        <script>
            // Obtenemos la URL de hace 2 pasos (la lista)
            var urlLista = document.referrer.split('/Delete')[0]; 
            
            // Si por alguna razón no la detecta, usa una ruta base
            if(!urlLista || urlLista.includes('Delete')) {
                urlLista = '/Mascotas/MisPublicaciones'; 
            }

            // Usamos replace para que el navegador pida la página al servidor otra vez
            window.location.replace(urlLista);
        </script>", "text/html");
        }
    }
}