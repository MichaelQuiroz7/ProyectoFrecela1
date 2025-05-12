using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {

        private readonly IRepositorioRol _repository;

        public RolController(IRepositorioRol repository)
        {
            _repository = repository;
        }

        //[Route("GetRoles")]
        [HttpGet]
        public async Task<ActionResult<ResponseModel>> GetRoles()
        {
            ResponseModel response = await _repository.ObtenerRoles();
            return Ok(response);
        }

        //[Route("AddrRol")]
        [HttpPost]
        public async Task<ActionResult<ResponseModel>> AgregarRol(Rol rol)
        {
            ResponseModel response = await _repository.AgregarRol(rol);
            return CreatedAtAction(nameof(GetRoles), new { id = ((Rol)response.Data).IdRol }, response);
        }

        //[Route("DeleteRol")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseModel>> EliminarRol(int id, [FromQuery] int idrol)
        {
            ResponseModel response = await _repository.EliminarRol(id, idrol);
            return Ok(response);
        }

    }
}
