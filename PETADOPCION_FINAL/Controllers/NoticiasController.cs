using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETADOPCION_FINAL.Models;

namespace PETADOPCION_FINAL.Controllers
{
    public class NoticiasController : Controller
    {
        private readonly PetadopcionContext _context;
        private readonly IConfiguration _configuration;

        public NoticiasController(PetadopcionContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private bool EsAdmin()
        {
            return HttpContext.Session.GetInt32("Rol") == 1;
        }

        private bool EstaLogueado()
        {
            return HttpContext.Session.GetString("UsuarioLogueado") != null;
        }

        // GET: Noticias
        public async Task<IActionResult> Index()
        {
            if (!EstaLogueado()) return RedirectToAction("Index", "Home");
            if (!EsAdmin()) return RedirectToAction("Principal", "Home");

            // Se eliminó la referencia a Orden, ahora solo por Fecha
            var noticias = await _context.Noticias
                .Include(n => n.Usuario)
                .OrderByDescending(n => n.FechaPublicacion)
                .ToListAsync();
            return View(noticias);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, bool activo)
        {
            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();

            noticia.Activo = activo;
            _context.Update(noticia);
            await _context.SaveChangesAsync();

            return Ok();
        }

       

        // GET: Noticias/Create
        public IActionResult Create()
        {
            if (!EstaLogueado() || !EsAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Noticias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Noticia noticia, IFormFile archivoImagen)
        {
            if (!EsAdmin()) return RedirectToAction("Principal", "Home");

            int? idSesion = HttpContext.Session.GetInt32("IdUsuario");
            if (idSesion == null) return RedirectToAction("Index", "Home");

            // 1. Proceso de Azure Storage
            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                string connectionString = _configuration.GetConnectionString("AzureStorage");
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("noticias");

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = archivoImagen.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                noticia.ImagenUrl = blobClient.Uri.ToString();
            }

            // 2. Asignaciones Automáticas (Se quitó Categoria y Orden)
            noticia.IdUsuarioPublicador = idSesion.Value;
            noticia.FechaPublicacion = DateTime.Now;
            noticia.Activo = true;

            // Limpiamos errores de validación de campos que no vienen del form o son automáticos
            ModelState.Remove("ImagenUrl");
            ModelState.Remove("Usuario");
            ModelState.Remove("archivoImagen");
            // Quitamos validaciones de los campos eliminados por si quedaron en el ModelState
            ModelState.Remove("TipoNoticia");
            ModelState.Remove("Orden");

            if (ModelState.IsValid)
            {
                _context.Add(noticia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(noticia);
        }

        // GET: Noticias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!EstaLogueado() || !EsAdmin()) return RedirectToAction("Index", "Home");
            if (id == null) return NotFound();

            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();

            return View(noticia);
        }

        // POST: Noticias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Noticia noticia, IFormFile? archivoImagen)
        {
            if (id != noticia.IdNoticia) return NotFound();

            // Limpiamos validaciones de campos que se manejan aparte
            ModelState.Remove("Usuario");
            ModelState.Remove("archivoImagen");
            ModelState.Remove("TipoNoticia");
            ModelState.Remove("Orden");

            if (ModelState.IsValid)
            {
                try
                {
                    if (archivoImagen != null && archivoImagen.Length > 0)
                    {
                        string connectionString = _configuration.GetConnectionString("AzureStorage");
                        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("noticias");

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                        BlobClient blobClient = containerClient.GetBlobClient(fileName);

                        using (var stream = archivoImagen.OpenReadStream())
                        {
                            await blobClient.UploadAsync(stream, true);
                        }
                        noticia.ImagenUrl = blobClient.Uri.ToString();
                    }
                    else
                    {
                        // Evita que Entity Framework marque ImagenUrl como null si no se sube una nueva
                        _context.Entry(noticia).Property(x => x.ImagenUrl).IsModified = false;
                    }

                    _context.Update(noticia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoticiaExists(noticia.IdNoticia)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(noticia);
        }

        // GET: Noticias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!EstaLogueado() || !EsAdmin()) return RedirectToAction("Index", "Home");
            if (id == null) return NotFound();

            var noticia = await _context.Noticias
                .Include(n => n.Usuario)
                .FirstOrDefaultAsync(m => m.IdNoticia == id);

            if (noticia == null) return NotFound();

            return View(noticia);
        }

        // POST: Noticias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia != null)
            {
                _context.Noticias.Remove(noticia);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NoticiaExists(int id)
        {
            return _context.Noticias.Any(e => e.IdNoticia == id);
        }
    }
}