using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioEmpleado
    {

        Task<ResponseModel> ObtenerEmpleados();
        Task<ResponseModel> ValidarCredenciales(string cedula, string contrasenia);
        Task<ResponseModel> AgregarEmpleado(EmpleadoRequest empleado);
        Task<ResponseModel> AgregarDescuento(descuentoEmpleado empleado);
        Task<ResponseModel> ObtenerDescuentos(string cedula);
        Task<ResponseModel> eliminarEmpleados(int idempleado);

    }
}
