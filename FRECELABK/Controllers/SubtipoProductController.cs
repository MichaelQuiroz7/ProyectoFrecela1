using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubtipoProductController : ControllerBase
    {

        private readonly IRepositorioTiposProduct _repository;
        public SubtipoProductController(IRepositorioTiposProduct repositorio)
        {
            _repository = repositorio;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> GetTiposSubproduct()
        {
            ResponseModel response = await _repository.ObtenerTiposSubproduct();
            return Ok(response);
        }

    }
}
