using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Models
{
    public class Venta
    {

        public string CedulaCliente { get; set; }
        public string CedulaEmpleado { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

    }

    public class DetalleVentaRequest
    {
        public string IdVentaBase64 { get; set; }
    }

    public class DetalleVentaResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string NombreProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public int? Cantidad { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string NombresCliente { get; set; }
        public string ApellidosCliente { get; set; }
        public string CedulaCliente { get; set; }
    }

}
