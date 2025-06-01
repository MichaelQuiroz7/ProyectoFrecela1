using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IRepositorioCliente _repository;

        public ClienteController(IRepositorioCliente repositorioCliente)
        {
            _repository = repositorioCliente;
        }

        [HttpPost("RegistrarCliente")]
        public async Task<IActionResult> RegistrarCliente([FromBody] ClienteRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse
                {
                    Code = "00",
                    Message = "Solicitud inválida"
                });
            }
            var response = await _repository.RegistrarCliente(request);
            if (response.Code == "00")
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("IniciarSesion")]
        public async Task<ActionResult<ResponseModel>> IniciarSesion([FromBody] LoginCliente request) 
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse
                {
                    Code = "00",
                    Message = "Solicitud inválida"
                });
            }
            else
            {
                ResponseModel response = await _repository.IniciarSesion(request);
                return Ok(response);
            }
        }
    }
}
