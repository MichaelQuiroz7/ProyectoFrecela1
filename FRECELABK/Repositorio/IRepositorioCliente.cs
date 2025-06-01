using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioCliente
    {

         Task<ApiResponse> RegistrarCliente(ClienteRequest request);
        Task<ResponseModel> IniciarSesion(LoginCliente request);

    }
}
