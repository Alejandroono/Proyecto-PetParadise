using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PETADOPCION_FINAL.Models
{
    [Table("Noticias")] // Indica que use la tabla que creamos en SQL
    public class Noticia
    {
        [Key]
        public int IdNoticia { get; set; }

        [Required(ErrorMessage = "El título de la noticia es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 100 caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El subtítulo es obligatorio. Sirve como resumen de la noticia.")]
        [StringLength(250, MinimumLength = 10, ErrorMessage = "El subtítulo debe tener entre 10 y 250 caracteres.")]
        public string? Subtitulo { get; set; }
        [Required(ErrorMessage = "Una noticia sin contenido no es noticia. Escribe el cuerpo del mensaje.")]
        [MinLength(20, ErrorMessage = "El contenido debe ser más largo (mínimo 20 letras).")]
        public string? Contenido { get; set; }

        [Required(ErrorMessage = "La URL de la imagen es obligatoria")]
        public string ImagenUrl { get; set; }
        [Required(ErrorMessage = "El enlace de referencia es obligatorio.")]
        [Url(ErrorMessage = "El enlace debe ser una URL válida.")]
        public string? EnlaceUrl { get; set; }

        public DateTime? FechaPublicacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

       

        // --- RELACIÓN CON USUARIOS ---

        [Required]
        public int IdUsuarioPublicador { get; set; }

        // Esta propiedad permite que C# traiga los datos del usuario 
        // haciendo un simple .Include(n => n.Usuario)
        [ForeignKey("IdUsuarioPublicador")]
        public virtual Usuario? Usuario { get; set; }
    }
}