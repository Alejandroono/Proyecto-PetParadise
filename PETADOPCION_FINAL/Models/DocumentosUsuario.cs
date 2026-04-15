using System;
using System.Collections.Generic;

namespace PETADOPCION_FINAL.Models;

public partial class DocumentosUsuario
{
    public int IdDocumento { get; set; }

    public int IdUsuario { get; set; }

    public string UrlRecibo { get; set; } = null!;

    public string UrlArchivo { get; set; } = null!;
    public string? UrlFormulario { get; set; }

    public string? EstadoVerificacion { get; set; }

    public DateTime? FechaSubida { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
