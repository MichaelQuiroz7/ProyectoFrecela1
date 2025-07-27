using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
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

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> CrearEmpleado([FromBody] EmpleadoRequest empleado)
        {
            if (empleado == null)
            {
                return BadRequest("Empleado no puede ser nulo");
            }
            ResponseModel response = await _repository.AgregarEmpleado(empleado);
            return Ok(response);
        }

        [HttpPost("descuento")]
        public async Task<ActionResult<ResponseModel>> AgregarDescuento([FromBody] descuentoEmpleado empleado)
        {
            if (empleado == null)
            {
                return BadRequest("Descuento no puede ser nulo");
            }
            ResponseModel response = await _repository.AgregarDescuento(empleado);
            return Ok(response);
        }


        [HttpGet("descuento/{cedula}")]
        public async Task<ActionResult<ResponseModel>> ObtenerDescuentos(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                return BadRequest("Cédula no puede ser nula o vacía");
            }
            ResponseModel response = await _repository.ObtenerDescuentos(cedula);
            return Ok(response);
        }


        [HttpPut("EliminarEmpleado")]
        public async Task<ActionResult<ResponseModel>> EliminarEmpleado( [FromBody]int idempleado)
        {
            if (idempleado <= 0)
            {
                return BadRequest("ID de empleado no puede ser menor o igual a cero");
            }
            ResponseModel response = await _repository.eliminarEmpleados(idempleado);
            return Ok(response);
        }



    }

    public class LoginRequest
    {
        public string Cedula { get; set; } = null!;
        public string Contrasenia { get; set; } = null!;
    }

}

