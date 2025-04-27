using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
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


        [HttpGet("rol/{id}")]
        public async Task<ActionResult<ResponseModel>> GetIdRol(int id)
        {
            var response = await _repository.ObtenerIdRolPorIdEmpleado(id);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

    }
}
