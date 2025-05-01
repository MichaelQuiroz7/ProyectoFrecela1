using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertaController : ControllerBase
    {

        private readonly IRepositorioAlerta _repository;

        public AlertaController(IRepositorioAlerta repository)
        {
            _repository = repository;
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerAlertas()
        {
            var response = await _repository.ObtenerAlertas();
            if (response.Code == ResponseType.Error)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message, data = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> AgregarAlerta([FromBody] Alerta alerta)
        {
            var response = await _repository.AgregarAlerta(alerta);
            if (response.Code == ResponseType.Error)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message, data = response.Data });
        }

        [HttpPut("cambiarEstado/{idProducto}")]
        public async Task<IActionResult> CambiarEstadoAlerta(int idProducto, [FromQuery] string nombreProducto, [FromQuery] string mensaje)
        {
            var response = await _repository.CambiarEstadoAlerta(idProducto, nombreProducto, mensaje);
            if (response.Code == ResponseType.Error)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message, data = response.Data });
        }


    }
}
