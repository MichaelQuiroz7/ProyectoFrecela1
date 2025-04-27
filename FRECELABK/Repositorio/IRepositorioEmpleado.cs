using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioEmpleado
    {

        Task<ResponseModel> ObtenerEmpleados();
        Task<ResponseModel> ObtenerIdRolPorIdEmpleado(int idEmpleado);

    }
}
