namespace NuvoxSound.Entities
{
    public class ItemCarrito
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal => (Producto?.Precio ?? 0) * Cantidad;
    }
}
