using System;
using System.Collections.Generic;

namespace NuvoxSound.Entities
{
    public class BoletaFisica
    {
        public int IdVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string ClienteNombres { get; set; } = string.Empty;
        public string ClienteCorreo { get; set; } = string.Empty;

        
        public List<BoletaDetalle> Detalles { get; set; } = new List<BoletaDetalle>();
    }

    public class BoletaDetalle
    {
        public string NombreProducto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
    }
}