namespace PETADOPCION_FINAL.Services
{
    public interface IEmailService
    {
        Task Enviar(string emailDestino, string asunto, string cuerpoHtml, string colorHeader);
    }
}