using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadoController : ControllerBase
    {

        private readonly IRepositorioEmpleado _repository;

        public EmpleadoController(IRepositorioEmpleado repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> GetEmpleados()
        {
            var response = await _repository.ObtenerEmpleados();
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Cedula) || string.IsNullOrEmpty(request.Contrasenia))
            {
                return BadRequest(new { message = "Cédula y contraseña son requeridas" });
            }

            var response = await _repository.ValidarCredenciales(request.Cedula, request.Contrasenia);

            if (response.Code == ResponseType.Error)
            {
                return Unauthorized(new { message = response.Message });
            }

            return Ok(new
            {
                message = response.Message,
                data = response.Data
            });
        }
    }

    public class LoginRequest
    {
        public string Cedula { get; set; } = null!;
        public string Contrasenia { get; set; } = null!;
    }

}

