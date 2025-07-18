﻿using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {

        private readonly IRepositorioVenta _repositorioVenta;

        public VentaController(IRepositorioVenta repositorioVenta)
        {
            _repositorioVenta = repositorioVenta;
        }

        [HttpPost("RegistrarPedido")]
        public async Task<IActionResult> RegistrarVenta([FromBody] Venta venta)
        {

            ApiResponse response = await _repositorioVenta.RegistrarVenta(venta);
            return Ok(response);

        }

        [HttpPost("detallePedido")]
        public async Task<IActionResult> ObtenerDetalleVenta([FromBody] DetalleVentaRequest request)
        {

            var response = await _repositorioVenta.ObtenerDetalle(request);

            if (response.Code != "00")
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        #region Controlador Genera PDF
        [HttpPost("convert-to-pdf")]
        public async Task<IActionResult> ConvertToPdf([FromBody] LatexRequest request)
        {
            var result = await _repositorioVenta.ConvertToPdf(request);
            return result;
        }

        #endregion



        #region Controlador Genera Comprobante
        [HttpPost("registrarPago")]
        public async Task<IActionResult> RegistrarPago([FromForm] ComprobanteModel model)
        {
            if (model.Imagen == null || model.Imagen.Length == 0)
            {
                return BadRequest(new ResponseModel
                {
                    Code = ResponseType.Error,
                    Message = "No se proporcionó una imagen.",
                    Data = null
                });
            }

            try
            {
                // Convertir IFormFile a byte[]
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await model.Imagen.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                var comprobante = new Comprobante
                {
                    IdVenta = model.IdVenta,
                    Imagen = imageBytes,
                    Fecha = model.Fecha,
                    Hora = model.Hora
                };

                // Llamar al repositorio
                ResponseModel response = await _repositorioVenta.EditarComprobante(comprobante);


                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Code = ResponseType.Error,
                    Message = $"Error inesperado en el servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        #endregion


        #region Obtener Estadisticas Controller

        [HttpGet("estadisticasMes")]
        public async Task<ActionResult<ResponseModel>> ObtenerEstadisticas()
        {

            ResponseModel response = await _repositorioVenta.ObtenerEstadisticasVentasPorMes();
            return Ok(response);

        }

        #endregion


        #region Controlador tipo de entrega

        [HttpPost("registrarEntrega")]
        public async Task<IActionResult> InsertarEntrega([FromBody] Entrega entrega)
        {
           

            var response = await _repositorioVenta.InsertarEntrega(entrega);

            return Ok(response);
        }

        #endregion



        #region Obtener Ventas Pagadas o Rechazadas con Detalles

        [HttpGet("ventasPagadasORechazadasConDetalles")]
        public async Task<IActionResult> ObtenerVentasPoR()
        {
            var response = await _repositorioVenta.ObtenerVentasPagadasORechazadasConDetalles();
            return Ok(response);

        }

        #endregion


        #region Venta Completa Controlador

        [HttpPost("actualizarEstadoVenta")]
        public async Task<IActionResult> ActualizarEstadoVenta([FromBody] DetalleVentaConsulta detalle)
        {
            if (detalle == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = ResponseType.Error,
                    Message = "Detalle de venta no puede ser nulo.",
                    Data = null
                });
            }
            var response = await _repositorioVenta.ActualizarEstadoVenta(detalle);
            return Ok(response);
        }

        #endregion


        #region Ventas por Aprobar

        [HttpGet("ventasXAprobar")]
        public async Task<IActionResult> pagosxAprobar()
        {
            ResponseModel response = await _repositorioVenta.ObtenerComprobantesEnEsperaConDetallesdos();
            return Ok(response);
        }


        #endregion



        #region Obtener Ventas por Clientes

        [HttpGet("ventasxCliente/{cedula}")]
        public async Task<IActionResult> GetVentasPorCedula(string cedula)
        {
            var response = await _repositorioVenta.ObtenerVentasPorCedulaCliente(cedula);
            return Ok(response);
        }

        #endregion



        #region Obtener id base de64 de las ventas

        [HttpGet("ventasIdsBase64")]
        public async Task<IActionResult> ObtenerIdsVentasBase64()
        {
            ResponseModel response = await _repositorioVenta.ObtenerIdsVentasBase64();
            return Ok(response);
        }

        #endregion


        #region Obtener Ventas por Empleado

        [HttpGet("ventasxEmpleado/{cedula}")]
        public async Task<IActionResult> GetVentasPorCedulaEmpleado(string cedula)
        {
            var response = await _repositorioVenta.ObtenerVentasPorCedulaEmpleado(cedula);
            return Ok(response);
        }

        #endregion


        #region Ingresar venta por empleado

        [HttpPost("ingresar")]
        public async Task<IActionResult> IngresarVenta([FromBody] IngresarVentaRequest request)
        {
            var response = await _repositorioVenta.IngresarVentaEmpleado(request);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        #endregion



    }
}
