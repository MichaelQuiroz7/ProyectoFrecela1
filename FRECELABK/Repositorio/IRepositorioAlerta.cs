using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioAlerta
    {
        Task<ResponseModel> ObtenerAlertas();
        Task<ResponseModel> AgregarAlerta(Alerta alerta);
        Task<ResponseModel> CambiarEstadoAlerta(int idProducto, string nombreProducto, string mensaje);
    }
}
