using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioEmpleado
    {

        Task<ResponseModel> ObtenerEmpleados();
        Task<ResponseModel> ValidarCredenciales(string cedula, string contrasenia);

    }
}
