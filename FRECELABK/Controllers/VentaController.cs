using FRECELABK.Models;
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

                // Crear objeto Comprobante
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
                // Manejo de error global
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
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = ResponseType.Error,
                    Message = "Datos de entrada inválidos",
                    Data = null
                });
            }

            var response = await _repositorioVenta.InsertarEntrega(entrega);

            return Ok(response);
        }

        #endregion

    }
}
