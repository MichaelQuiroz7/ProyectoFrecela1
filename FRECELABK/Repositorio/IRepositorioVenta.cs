using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioVenta
    {

        Task<ApiResponse> RegistrarVenta(Venta venta);
        Task<DetalleVentaResponse> ObtenerDetalle(DetalleVentaRequest request);

    }
}
