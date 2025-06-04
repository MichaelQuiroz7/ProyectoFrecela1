using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { Code = "00", Message = "Datos de entrada inválidos" });
            }

            var response = await _repositorioVenta.ObtenerDetalle(request);

            if (response.Code == "01")
            {
                return Ok(response);
            }
            return BadRequest(response);
        }


        [HttpPost("convert-to-pdf")]
        public async Task<IActionResult> ConvertToPdf([FromBody] LatexRequest request)
        {
            var result = await _repositorioVenta.ConvertToPdf(request);
            return result;
        }

    }
}
