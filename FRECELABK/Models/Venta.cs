using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Models
{
    public class Venta
    {

        public string? CedulaCliente { get; set; }
        public string? CedulaEmpleado { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Observaciones { get; set; } = string.Empty;

    }

    public class DetalleVentaRequest
    {
        public string? IdVentaBase64 { get; set; }
    }

    public class DetalleVentaResponse
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? NombreProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public int? Cantidad { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? NombresCliente { get; set; }
        public string? ApellidosCliente { get; set; }
        public string? CedulaCliente { get; set; }
        public string? DireccionCliente { get; set; }
        public string? TipoEntrega { get; set; }
        public string? Observaciones { get; set; } = string.Empty;
    }

    public class DetalleVentaConsulta
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? NombreProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public int? Cantidad { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? NombresCliente { get; set; }
        public string? ApellidosCliente { get; set; }
        public string? CedulaCliente { get; set; }
        public string? DireccionCliente { get; set; }
        public string? TipoEntrega { get; set; }
        public string? EstadoEntrega { get; set; } = "PENDIENTE";
        public string? Observaciones { get; set; } = string.Empty ;
    }



    public class LatexRequest
    {
        public string? Cliente { get; set; }
        public string? Cedula { get; set; }
        public string? Fecha { get; set; }
        public List<ProductoRequest>? Productos { get; set; }
        public decimal SubtotalSinDescuento { get; set; }
        public decimal Descuento { get; set; }
        public decimal SubtotalConDescuento { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductoRequest
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
    }

    public class Comprobante
    {
        public int IdVenta { get; set; }
        public byte[]? Imagen { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
    }


    public class ComprobanteModel
    {
        public int IdVenta { get; set; }
        public IFormFile? Imagen { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
    }

    //public enum TipoEntrega
    //{
    //    RETIRAR_EN_LOCAL = 'ENTREGA EN LOCAL',
    //    ENTREGA_A_DOMICILIO
    //}

    public class Entrega
    {
        public int IdVenta { get; set; }
        public string TipoEntrega { get; set; } = "ENTREGA EN LOCAL" ;
        public decimal? CostoEntrega { get; set; }
        public string? Direccion { get; set; }
    }


    public class ComprobanteConsulta
    {
        public int IdVenta { get; set; }
        public string Estado { get; set; }
        public string ImagenBase64 { get; set; } 
        public DateTime? Fecha { get; set; } 
    }

    

    public class ComprobanteConsultados
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public int IdVenta { get; set; }
        public string Estado { get; set; }
        public string ImagenBase64 { get; set; }
        public DateTime? Fecha { get; set; }
        public string NombreProducto { get; set; }
        public string DescripcionProducto { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public int? Cantidad { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string NombresCliente { get; set; }
        public string ApellidosCliente { get; set; }
        public string CedulaCliente { get; set; }
        public string DireccionCliente { get; set; }
        public string TipoEntrega { get; set; }
        public string Observaciones { get; set; }
    }


    public class SaleDetails
    {
        public int IdVenta { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioTotal { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } 
    }


}
