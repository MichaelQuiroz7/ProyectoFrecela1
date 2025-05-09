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
            ResponseModel response = await _repository.ObtenerEmpleados();

            return Ok(response);

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            ResponseModel response = await _repository.ValidarCredenciales(request.Cedula, request.Contrasenia);
            return Ok(response);
        }
    }

    public class LoginRequest
    {
        public string Cedula { get; set; } = null!;
        public string Contrasenia { get; set; } = null!;
    }

}

