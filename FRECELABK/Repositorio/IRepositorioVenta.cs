﻿using FRECELABK.Models;
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

    }
}
