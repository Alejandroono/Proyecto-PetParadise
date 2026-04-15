using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PETADOPCION_FINAL.Models;
using PETADOPCION_FINAL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PETADOPCION_FINAL.Controllers
{
    public class SolicitudesAdopcionsController : Controller
    {
        private readonly PetadopcionContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService; // ✅ Reemplaza "object" por IEmailService

        public SolicitudesAdopcionsController(PetadopcionContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        // ✅ AGREGADO: Index
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesAdopcions
                .Include(s => s.IdMascotaNavigation)
                .Include(s => s.IdUsuarioSolicitanteNavigation)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        // ✅ AGREGADO: Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.SolicitudesAdopcions
                .Include(s => s.IdMascotaNavigation)
                .Include(s => s.IdUsuarioSolicitanteNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null) return NotFound();

            return View(solicitud);
        }

        public async Task<IActionResult> MisSolicitudesEnviadas()
        {
            int? idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario");
            if (idUsuarioLogueado == null) return RedirectToAction("Index", "Home");

            var misSolicitudes = await _context.SolicitudesAdopcions
                .Include(s => s.IdMascotaNavigation)
                    .ThenInclude(m => m.MascotaFotos)
                .Include(s => s.IdMascotaNavigation.IdUsuarioPublicadorNavigation)
                .Where(s => s.IdUsuarioSolicitante == idUsuarioLogueado)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(misSolicitudes);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarAdopcionFinal(int idSolicitud)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var solicitudGanadora = await _context.SolicitudesAdopcions
                            .Include(s => s.IdMascotaNavigation)
                            .Include(s => s.IdUsuarioSolicitanteNavigation)
                            .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

                        if (solicitudGanadora == null)
                            return Json(new { success = false, message = "No se encontró la solicitud." });

                        var mascota = solicitudGanadora.IdMascotaNavigation;

                        var otrosSolicitantes = await _context.SolicitudesAdopcions
                            .Include(s => s.IdUsuarioSolicitanteNavigation)
                            .Where(s => s.IdMascota == solicitudGanadora.IdMascota &&
                                        s.IdUsuarioSolicitante != solicitudGanadora.IdUsuarioSolicitante)
                            .Select(s => new {
                                Email = s.IdUsuarioSolicitanteNavigation.Email,
                                Nombre = s.IdUsuarioSolicitanteNavigation.Nombre
                            })
                            .ToListAsync();

                        var nuevaAdopcion = new Adopcione
                        {
                            IdMascota = solicitudGanadora.IdMascota,
                            IdAdoptante = solicitudGanadora.IdUsuarioSolicitante,
                            FechaAdopcion = DateTime.Now
                        };
                        _context.Adopciones.Add(nuevaAdopcion);

                        mascota.EstadoAdopcion = "Adoptado";
                        _context.Update(mascota);

                        var solicitudesAEliminar = _context.SolicitudesAdopcions
                            .Where(s => s.IdMascota == solicitudGanadora.IdMascota);
                        _context.SolicitudesAdopcions.RemoveRange(solicitudesAEliminar);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        foreach (var usuario in otrosSolicitantes)
                        {
                            string cuerpoRechazo = $@"
                        <h2 style='color: #1e293b;'>Hola, {usuario.Nombre}</h2>
                        <p>Te informamos que el proceso de adopción para <b>{mascota.Nombre}</b> ha finalizado, ya que ha encontrado un nuevo hogar.</p>
                        <p>¡Gracias por tu interés en PetParadise!</p>";

                            _ = _emailService.Enviar(usuario.Email, $"Actualización: {mascota.Nombre}", cuerpoRechazo, "#94a3b8");
                        }

                        return Json(new { success = true, message = "Adopción registrada con éxito y notificaciones enviadas." });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Error: " + ex.Message });
                    }
                }
            });
        }

        public async Task<IActionResult> MisSolicitudes()
        {
            int? idDueño = HttpContext.Session.GetInt32("IdUsuario");
            if (idDueño == null) return RedirectToAction("Index", "Home");

            var solicitudes = await _context.SolicitudesAdopcions
                .Include(s => s.IdMascotaNavigation)
                .Include(s => s.IdUsuarioSolicitanteNavigation)
                .Where(s => s.IdMascotaNavigation.IdUsuarioPublicador == idDueño)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        public async Task<IActionResult> VerDocumento(int idUsuario, string queArchivo)
        {
            var doc = await _context.DocumentosUsuarios
                .FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);

            if (doc == null)
            {
                return Content("<script>alert('El usuario no ha cargado documentos aún.'); window.close();</script>", "text/html");
            }

            string urlFinal = "";

            if (queArchivo.ToLower() == "cedula")
            {
                urlFinal = doc.UrlArchivo;
            }
            else if (queArchivo.ToLower() == "recibo")
            {
                urlFinal = doc.UrlRecibo;
            }
            else if (queArchivo.ToLower() == "formulario")
            {
                urlFinal = doc.UrlFormulario;
            }

            if (string.IsNullOrEmpty(urlFinal))
            {
                return Content("<script>alert('El archivo solicitado no está disponible.'); window.close();</script>", "text/html");
            }

            return Redirect(urlFinal);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarSolicitud(int idSolicitud, string accion)
        {
            var solicitud = await _context.SolicitudesAdopcions
                .Include(s => s.IdUsuarioSolicitanteNavigation)
                .Include(s => s.IdMascotaNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null) return Json(new { success = false, message = "Solicitud no encontrada." });

            var mascota = solicitud.IdMascotaNavigation;
            if (mascota == null) return Json(new { success = false, message = "Mascota no encontrada." });

            string asunto = "";
            string mensajeCuerpo = "";
            string colorHeader = "#5CA9DD";
            bool enviarEmail = true;

            if (accion == "Aceptar")
            {
                solicitud.Estado = "Aprobada";
                mascota.EstadoAdopcion = "En Proceso";

                asunto = $"¡Felicidades! Tu solicitud para {mascota.Nombre} fue aprobada 🐾";
                mensajeCuerpo = $@"
            <h2 style='color: #1e293b;'>¡Hola, {solicitud.IdUsuarioSolicitanteNavigation.Nombre}!</h2>
            <p>Tenemos excelentes noticias: Tu solicitud para adoptar a <b>{mascota.Nombre}</b> ha sido aprobada.</p>
            <p>El protector de la mascota se pondrá en contacto contigo muy pronto para los siguientes pasos.</p>
            <p style='color: #64748b; font-size: 0.9em;'>¡Gracias por abrir tu corazón a un peludo!</p>";
            }
            else if (accion == "Rechazar")
            {
                solicitud.Estado = "Rechazada";
                mascota.EstadoAdopcion = "Disponible";
                colorHeader = "#e11d48";
                asunto = $"Actualización sobre la solicitud de {mascota.Nombre}";
                mensajeCuerpo = $@"
            <h2 style='color: #1e293b;'>Hola, {solicitud.IdUsuarioSolicitanteNavigation.Nombre}</h2>
            <p>Agradecemos mucho tu interés en adoptar a <b>{mascota.Nombre}</b>.</p>
            <p>Lamentamos informarte que en esta ocasión el protector ha decidido continuar con otro perfil. 
            Te invitamos a seguir buscando a tu compañero ideal en nuestra plataforma.</p>";
            }
            else if (accion == "Restablecer")
            {
                solicitud.Estado = "En revisión";
                mascota.EstadoAdopcion = "Disponible";
                enviarEmail = false;
            }

            try
            {
                await _context.SaveChangesAsync();

                if (enviarEmail)
                {
                    // ✅ Usa IEmailService → mockeable en tests
                    await _emailService.Enviar(
                        solicitud.IdUsuarioSolicitanteNavigation.Email,
                        asunto,
                        mensajeCuerpo,
                        colorHeader
                    );
                }

                return Json(new { success = true, message = $"Solicitud procesada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ✅ AGREGADO: Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var solicitud = await _context.SolicitudesAdopcions
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null)
                return Json(new { success = false, message = "Solicitud no encontrada." });

            _context.SolicitudesAdopcions.Remove(solicitud);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Solicitud eliminada correctamente." });
        }

        public async Task<IActionResult> solicitarMascota(int id)
        {
            if (id <= 0) return NotFound();

            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.IdMascota == id);
            if (mascota == null) return NotFound();

            int totalDias = mascota.Edad.GetValueOrDefault(0);
            int anios = totalDias / 365;
            int meses = (totalDias % 365) / 30;
            int dias = (totalDias % 365) % 30;

            ViewBag.EdadFormateada = $"{anios} {(anios == 1 ? "Año" : "Años")}, {meses} {(meses == 1 ? "Mes" : "Meses")}, {dias} {(dias == 1 ? "Día" : "Días")}";

            var idLogueado = HttpContext.Session.GetInt32("IdUsuario");
            if (idLogueado == null) return RedirectToAction("Index", "Home");

            var usuario = await _context.Usuarios.FindAsync(idLogueado);

            ViewBag.Mascota = mascota;
            ViewBag.Usuario = usuario;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(int idMascota, int idUsuario, string mensaje, IFormFile pdfFormulario, IFormFile pdfCedula, IFormFile pdfRecibo)
        {
            try
            {
                var existe = await _context.SolicitudesAdopcions.AnyAsync(s => s.IdMascota == idMascota && s.IdUsuarioSolicitante == idUsuario);
                if (existe) return Json(new { success = false, message = "Ya enviaste una solicitud para esta mascota." });

                string connAzure = _config.GetConnectionString("AzureStorage");
                var blobServiceClient = new BlobServiceClient(connAzure);
                var container = blobServiceClient.GetBlobContainerClient("documentos-adopcion");
                await container.CreateIfNotExistsAsync();

                string urlFormulario = await SubirArchivo(pdfFormulario, container, idUsuario, "formulario");
                string urlCedula = await SubirArchivo(pdfCedula, container, idUsuario, "cedula");
                string urlRecibo = await SubirArchivo(pdfRecibo, container, idUsuario, "recibo");

                var solici = new DocumentosUsuario
                {
                    IdUsuario = idUsuario,
                    UrlArchivo = urlCedula,
                    UrlRecibo = urlRecibo,
                    UrlFormulario = urlFormulario,
                    EstadoVerificacion = "Pendiente",
                    FechaSubida = DateTime.Now
                };

                _context.DocumentosUsuarios.Add(solici);

                var solicitud = new SolicitudesAdopcion
                {
                    IdMascota = idMascota,
                    IdUsuarioSolicitante = idUsuario,
                    Mensaje = mensaje,
                    Estado = "En revisión",
                    FechaSolicitud = DateTime.Now
                };

                _context.SolicitudesAdopcions.Add(solicitud);
                await _context.SaveChangesAsync();

                var solicitante = await _context.Usuarios.FindAsync(idUsuario);
                var mascota = await _context.Mascotas.FindAsync(idMascota);
                var dueno = await _context.Usuarios.FindAsync(mascota.IdUsuarioPublicador);

                await EnviarEmailGmail(solicitante, mascota, dueno, mensaje);

                return Json(new { success = true, message = "Solicitud y documentos registrados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private async Task<string> SubirArchivo(IFormFile file, BlobContainerClient container, int userId, string tipo)
        {
            string nombreUnico = $"{tipo}_{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blob = container.GetBlobClient(nombreUnico);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, true);
            }
            return blob.Uri.ToString();
        }

        private async Task EnviarEmailGmail(Usuario u, Mascota m, Usuario dueno, string msg)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("jsalgadobolano@correo.unicordoba.edu.co", "hfye rdlj mccp mfds");

                    int totalDias = m.Edad.GetValueOrDefault(0);
                    int anios = totalDias / 365;
                    int meses = (totalDias % 365) / 30;
                    int dias = (totalDias % 365) % 30;
                    string edadTexto = $"{anios} {(anios == 1 ? "Año" : "Años")}, {meses} {(meses == 1 ? "Mes" : "Meses")}, {dias} {(dias == 1 ? "Día" : "Días")}";

                    var mailSolicitante = new MailMessage();
                    mailSolicitante.From = new MailAddress("jsalgadobolano@correo.unicordoba.edu.co", "PetParadise");
                    mailSolicitante.To.Add(u.Email);
                    mailSolicitante.Subject = $"Solicitud de Adopción Enviada: {m.Nombre}";
                    mailSolicitante.IsBodyHtml = true;
                    mailSolicitante.Body = $@"
            <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e2e8f0; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);'>
                <div style='background-color: #5CA9DD; padding: 30px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 28px; letter-spacing: 1px;'>¡Solicitud Recibida! 🐾</h1>
                </div>
                <div style='padding: 30px; background-color: white;'>
                    <h2 style='color: #1e293b; margin-top: 0;'>¡Hola, {u.Nombre}!</h2>
                    <p style='color: #64748b; line-height: 1.6;'>
                        Gracias por dar el primer paso para cambiar una vida. Hemos recibido tu interés por adoptar a 
                        <span style='color: #5CA9DD; font-weight: bold;'>{m.Nombre}</span>.
                    </p>
                    <div style='background-color: #f8fafc; border: 1px solid #cbd5e1; border-radius: 12px; padding: 20px; margin: 25px 0;'>
                        <h3 style='color: #5CA9DD; margin-top: 0; border-bottom: 2px solid #5CA9DD; display: inline-block; padding-bottom: 5px;'>Detalles de tu futuro amigo:</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px 0; color: #475569;'><b>Raza:</b></td><td style='padding: 8px 0; color: #1e293b;'>{m.Raza}</td></tr>
                            <tr><td style='padding: 8px 0; color: #475569;'><b>Edad:</b></td><td style='padding: 8px 0; color: #1e293b;'>{edadTexto}</td></tr>
                            <tr><td style='padding: 8px 0; color: #475569;'><b>Sexo:</b></td><td style='padding: 8px 0; color: #1e293b;'>{m.Sexo}</td></tr>
                        </table>
                    </div>
                    <div style='border-left: 4px solid #82B441; padding: 15px; background-color: #f0fdf4; font-style: italic; color: #166534; margin-bottom: 25px;'>
                        ""{msg}""
                    </div>
                    <h3 style='color: #1e293b; font-size: 18px;'>¿Qué sigue ahora?</h3>
                    <ul style='color: #475569; padding-left: 20px; line-height: 1.8;'>
                        <li>El dueño de la mascota revisará tus documentos (Cédula y Recibo Público).</li>
                        <li>La persona que publicó a {m.Nombre} evaluará si tu perfil es apto para el proceso.</li>
                        <li>Si el dueño decide que eres el adoptante ideal, se pondrá en contacto contigo para concretar la entrega.</li>
                    </ul>
                    <div style='text-align: center; margin-top: 35px;'>
                        <p style='color: #94a3b8; font-size: 14px;'>¡Gracias por confiar en <b>PetParadise</b>!</p>
                        <div style='height: 2px; background: linear-gradient(to right, #5CA9DD, #82B441); margin: 10px auto; width: 50%;'></div>
                    </div>
                </div>
                <div style='background-color: #f1f5f9; padding: 20px; text-align: center; color: #94a3b8; font-size: 12px;'>
                    Este es un mensaje automático enviado desde el sistema PETPARADISE<br>© 2026 PetParadise. Todos los derechos reservados.
                </div>
            </div>";

                    await smtp.SendMailAsync(mailSolicitante);

                    if (dueno != null && !string.IsNullOrEmpty(dueno.Email))
                    {
                        var mailDueno = new MailMessage();
                        mailDueno.From = new MailAddress("jsalgadobolano@correo.unicordoba.edu.co", "PetParadise");
                        mailDueno.To.Add(dueno.Email);
                        mailDueno.Subject = $"¡Nueva Solicitud de Adopción para {m.Nombre}! 📩";
                        mailDueno.IsBodyHtml = true;
                        mailDueno.Body = $@"
                <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e2e8f0; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);'>
                    <div style='background-color: #82B441; padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 28px; letter-spacing: 1px;'>¡Nueva Solicitud! 📩</h1>
                    </div>
                    <div style='padding: 30px; background-color: white;'>
                        <h2 style='color: #1e293b; margin-top: 0;'>¡Hola!</h2>
                        <p style='color: #64748b; line-height: 1.6;'>
                            Buenas noticias: un usuario está interesado en adoptar a <span style='color: #82B441; font-weight: bold;'>{m.Nombre}</span>. 
                            Aquí tienes los detalles del solicitante para que inicies la evaluación.
                        </p>
                        <div style='background-color: #f8fafc; border: 1px solid #cbd5e1; border-radius: 12px; padding: 20px; margin: 25px 0;'>
                            <h3 style='color: #82B441; margin-top: 0; border-bottom: 2px solid #82B441; display: inline-block; padding-bottom: 5px;'>Datos del Interesado:</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td style='padding: 8px 0; color: #475569;'><b>Nombre:</b></td><td style='padding: 8px 0; color: #1e293b;'>{u.Nombre}</td></tr>
                                <tr><td style='padding: 8px 0; color: #475569;'><b>Correo:</b></td><td style='padding: 8px 0; color: #1e293b;'>{u.Email}</td></tr>
                                <tr><td style='padding: 8px 0; color: #475569;'><b>Teléfono:</b></td><td style='padding: 8px 0; color: #1e293b;'>{u.Telefono}</td></tr>
                            </table>
                        </div>
                        <div style='border-left: 4px solid #5CA9DD; padding: 15px; background-color: #f0f9ff; font-style: italic; color: #0c4a6e; margin-bottom: 25px;'>
                            <b>Mensaje de {u.Nombre}:</b><br>""{msg}""
                        </div>
                        <h3 style='color: #1e293b; font-size: 18px;'>Tus tareas como dueño:</h3>
                        <ol style='color: #475569; padding-left: 20px; line-height: 1.8;'>
                            <li>Ingresa a tu panel en <b>PetParadise</b> para revisar los documentos PDF adjuntos.</li>
                            <li>Verifica la identidad y el recibo público del solicitante.</li>
                            <li>Ponte en contacto con el interesado para concretar la entrevista o entrega.</li>
                        </ol>
                        <div style='text-align: center; margin-top: 35px;'>
                            <p style='color: #94a3b8; font-size: 14px;'>La seguridad de {m.Nombre} depende de tu evaluación.</p>
                            <div style='height: 2px; background: linear-gradient(to right, #82B441, #5CA9DD); margin: 10px auto; width: 50%;'></div>
                        </div>
                    </div>
                    <div style='background-color: #f1f5f9; padding: 20px; text-align: center; color: #94a3b8; font-size: 12px;'>
                        Este es un mensaje automático enviado desde el sistema PETPARADISE<br>© 2026 PetParadise. Todos los derechos reservados.
                    </div>
                </div>";

                        await smtp.SendMailAsync(mailDueno);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error SMTP: " + ex.Message);
            }
        }
    }
}