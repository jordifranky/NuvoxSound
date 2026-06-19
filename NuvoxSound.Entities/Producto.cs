namespace NuvoxSound.Entities
{
    public class Producto
    {
        public int IdPro { get; set; } // IdProducto
        public int IdCate { get; set; }// IdCategoria
        public string NomCat { get; set; } // NombreCategoria
        public string NomPro { get; set; }// NombreProducto
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string RutImag { get; set; }// RutaImagen
        public bool Activo { get; set; }
        public int DescargasRestantes { get; set; }
    }
}
