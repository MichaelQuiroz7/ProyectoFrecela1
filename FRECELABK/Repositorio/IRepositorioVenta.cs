using FRECELABK.Models;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioVenta
    {

        Task<ApiResponse> RegistrarVenta(Venta venta);
        Task<DetalleVentaResponse> ObtenerDetalle(DetalleVentaRequest request);
        Task<IActionResult> ConvertToPdf(LatexRequest request);
        Task<ResponseModel> EditarComprobante(Comprobante comprobante);
        Task<ResponseModel> ObtenerEstadisticasVentasPorMes();
        Task<ResponseModel> InsertarEntrega(Entrega entrega);
        Task<ResponseModel> ObtenerVentasPagadasORechazadasConDetalles();
        Task<ResponseModel> ActualizarEstadoVenta(DetalleVentaConsulta detalle);
        Task<ResponseModel> ObtenerComprobantesEnEsperaConDetalles();
        Task<ResponseModel> ObtenerComprobantesEnEsperaConDetallesdos();
        Task<ResponseModel> ObtenerVentasPorCedulaCliente(string cedula);
        Task<ResponseModel> ObtenerIdsVentasBase64();


    }
}
