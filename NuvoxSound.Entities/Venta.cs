using System;

namespace NuvoxSound.Entities
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
