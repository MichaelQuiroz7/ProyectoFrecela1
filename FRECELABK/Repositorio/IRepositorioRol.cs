using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioRol
    {
        Task<ResponseModel> ObtenerRoles();
        Task<ResponseModel> AgregarRol(Rol rol);
       // Task<ResponseModel> EditarRol(int id, Rol rol);
        Task<ResponseModel> EliminarRol(int id, int idrol);
    }
}
