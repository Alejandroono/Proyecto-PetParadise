using System.Net;
using System.Net.Mail;

namespace PETADOPCION_FINAL.Services
{
    public class EmailService : IEmailService
    {
        public async Task Enviar(string emailDestino, string asunto, string cuerpoHtml, string colorHeader)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential("jsalgadobolano@correo.unicordoba.edu.co", "hfye rdlj mccp mfds");

                    var mail = new MailMessage();
                    mail.From = new MailAddress("jsalgadobolano@correo.unicordoba.edu.co", "PetParadise 🐾");
                    mail.To.Add(emailDestino);
                    mail.Subject = asunto;
                    mail.IsBodyHtml = true;
                    mail.Body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; border-radius: 15px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
                    <div style='background-color: {colorHeader}; padding: 30px; text-align: center; color: white;'>
                        <h1 style='margin: 0; font-size: 28px;'>PetParadise</h1>
                        <p style='margin: 0; opacity: 0.9;'>Uniendo huellas con corazones</p>
                    </div>
                    <div style='padding: 30px; line-height: 1.6; color: #334155;'>
                        {cuerpoHtml}
                    </div>
                    <div style='background-color: #f8fafc; padding: 20px; text-align: center; color: #94a3b8; font-size: 12px;'>
                        Este es un mensaje automático de PetParadise. Por favor no respondas a este correo.
                    </div>
                </div>";

                    await smtp.SendMailAsync(mail);
                }
            }
            catch { /* Loggear si es necesario */ }
        }
    }
}