using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETADOPCION_FINAL.Models;
using Microsoft.AspNetCore.Http; // Necesario para el manejo de sesiones
using System.Net;
using System.Net.Mail;

namespace PETADOPCION_FINAL.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PetadopcionContext _context;

        public HomeController(ILogger<HomeController> logger, PetadopcionContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Acción para el error 404
        [Route("Home/Error404/{code?}")]
        public IActionResult Error404(int? code)
        {
            // Buscamos directamente en Shared para que no haya errores de ubicación
            return View("~/Views/Shared/Error404.cshtml");
        }

        public IActionResult Error500()
        {
            return View("~/Views/Shared/Error500.cshtml");
        }
        // 1. MÉTODO POST: Envía el correo con el tiempo incrustado en el enlace
        [HttpPost]
    public async Task<IActionResult> contrasena(string email)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario != null)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential("jsalgadobolano@correo.unicordoba.edu.co", "hfye rdlj mccp mfds");

                    // --- TRUCO DEL TIEMPO ---
                    // Guardamos la fecha/hora actual en un formato simple
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");

                    // El enlace ahora lleva el correo Y la hora de creación
                    var enlace = Url.Action("Restablecer", "Home", new { correo = usuario.Email, t = timestamp }, Request.Scheme);

                    var mail = new MailMessage();
                    mail.From = new MailAddress("jsalgadobolano@correo.unicordoba.edu.co", "PetParadise 🐾");
                    mail.To.Add(email);
                    mail.Subject = "Recupera tu acceso a PetParadise";
                    mail.IsBodyHtml = true;
                    mail.Body = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; border-radius: 20px; padding: 20px;'>
                    <div style='background-color: #2F5975; padding: 20px; text-align: center; border-radius: 10px;'>
                        <h1 style='color: white; margin: 0;'>PETPARADISE</h1>
                    </div>
                    <div style='padding: 30px; color: #444;'>
                        <h2>Hola, {usuario.Nombre}</h2>
                        <p>Has solicitado restablecer tu contraseña. Este enlace solo será válido por <b>15 minutos</b>.</p>
                        <div style='text-align: center; margin: 30px;'>
                            <a href='{enlace}' style='background-color: #2F5975; color: white; padding: 15px 25px; border-radius: 5px; text-decoration: none; font-weight: bold;'>RESTABLECER CONTRASEÑA</a>
                        </div>
                        <p style='font-size: 10px; color: #888;'>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                    </div>
                </div>";

                    await smtp.SendMailAsync(mail);
                }
                TempData["Mensaje"] = "HEMOS ENVIADO LAS INSTRUCCIONES A TU CORREO. SÍGUELAS PARA RESTABLECER TU CONTRASEÑA.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "ERROR AL ENVIAR: " + ex.Message;
            }
            return RedirectToAction("contrasena");
        }
        TempData["Error"] = "ESE CORREO NO ESTÁ REGISTRADO.";
        return View();
    }

    // 2. MÉTODO GET: Valida si el enlace expiró
    public IActionResult Restablecer(string correo, string t)
    {
        if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(t)) return RedirectToAction("Index");

        try
        {
            // Convertimos el texto del token 't' de nuevo a fecha
            DateTime fechaCreacion = DateTime.ParseExact(t, "yyyyMMddHHmm", null);
            var minutosPasados = (DateTime.Now - fechaCreacion).TotalMinutes;

            // Si pasaron más de 15 minutos, lo mandamos a volar
            if (minutosPasados > 15)
            {
                TempData["Error"] = "EL ENLACE HA EXPIRADO. SOLICITA UNO NUEVO.";
                return RedirectToAction("contrasena");
            }
        }
        catch
        {
            return RedirectToAction("Index");
        }

        ViewBag.Correo = correo;
        return View();
    }

    // 3. MÉTODO POST: Actualiza con ENCRIPTACIÓN
    [HttpPost]
    public async Task<IActionResult> ActualizarPassword(string correo, string nuevaPassword)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == correo);
        if (usuario != null)
        {
            // IMPORTANTE: Encriptamos la contraseña con BCrypt antes de guardar
            // Esto evita que la BD reviente y protege al usuario
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            // No usamos TempData aquí porque la vista Restablecer maneja el SweetAlert
            return Ok();
        }
        return BadRequest();
    }

    // --- VISTA DE LOGIN (INDEX) ---
    public IActionResult Index()
        {
            // Si el usuario ya está logueado y trata de ir al Index, lo mandamos a Principal
            if (HttpContext.Session.GetString("UsuarioLogueado") != null)
            {
                return RedirectToAction("Principal");
            }
            return View();
        }

        // --- PROCESO DE LOGIN (POST) ---
        [HttpPost]
        public async Task<IActionResult> Index(string Correo, string Contrasena)
        {
            // 1. Buscar al usuario por Email
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == Correo);

            // 2. Validar credenciales
            if (usuario != null && BCrypt.Net.BCrypt.Verify(Contrasena, usuario.PasswordHash))
            {
                // 3. CREAR SESIÓN: Guardamos datos clave para identificar al usuario
                HttpContext.Session.SetString("UsuarioLogueado", usuario.Email);
                HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
                HttpContext.Session.SetString("NombreUsuario", usuario.Nombre + " " + usuario.Apellido);

                HttpContext.Session.SetInt32("Rol", usuario.IdRol); // 🔥 AGREGA ESTO

                return RedirectToAction("Principal");
            }

            // 4. Si falla, enviamos error y volvemos a la vista (el modal se abrirá por JS)
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View();
        }

        // --- PÁGINA PRINCIPAL PROTEGIDA ---
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Principal()
        {
            // 1. TU VALIDACIÓN ORIGINAL (Por nombre/string)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioLogueado")))
            {
                return RedirectToAction("Index");
            }

            // 2. OBTENER EL ID (Para el filtro de "Mis Mascotas")
            // Asumo que al loguear también guardaste el ID en la sesión
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");
            ViewBag.Noticias = await _context.Noticias
        .Where(n => n.Activo)
        .OrderByDescending(n => n.FechaPublicacion)
        .ToListAsync();
            // 3. CONSULTA FILTRADA
            var mascotas = await _context.Mascotas
                .Include(m => m.MascotaFotos)
                .Where(m => m.EstadoAdopcion == "Disponible"
                       && m.IdUsuarioPublicador != idUsuarioLogueado) // No mostrar las mías
                .ToListAsync();

            return View(mascotas);
        }

        // --- CERRAR SESIÓN ---
        public IActionResult Salir()
        {
            // Destruye todas las variables de sesión
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult A_Cerca_DE()
        {
            return View();
        }
        public IActionResult contrasena()
        {
            return View();
        }
        public IActionResult solicitudAdopcion()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}