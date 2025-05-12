using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioRol : IRepositorioRol
    {
        private readonly string? cadenaConexion;
        ResponseModel response = new ResponseModel();

        public RepositorioRol(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }


        #region ObtenerRoles

        public async Task<ResponseModel> ObtenerRoles() {

            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {

                try
                {

                    await connection.OpenAsync();
                    string query = "SELECT * FROM rol";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    List<RolDTO> roles = new List<RolDTO>();
                    while (await reader.ReadAsync())
                    {
                        RolDTO rol = new RolDTO
                        {
                            IdRol = reader.GetInt32("id_rol"),
                            NombreRol = reader.GetString("nombre_rol")
                        };
                        roles.Add(rol);
                    }
                    response.Code = ResponseType.Success;
                    response.Message = "Roles obtenidos correctamente";
                    response.Data = roles;

                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al obtener los roles: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();

                }

                return response;

            }
        }

        #endregion


        #region AgregarRol

        public async Task<ResponseModel> AgregarRol(Rol rol)
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO rol (nombre) VALUES (@nombre)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nombre", rol.NombreRol);
                    await command.ExecuteNonQueryAsync();
                    response.Code = ResponseType.Success;
                    response.Message = "Rol agregado correctamente";
                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al agregar el rol: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return response;
            }
        }

        #endregion


        #region EliminarRol

        public async Task<ResponseModel> EliminarRol(int id, int idrol)
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM rol WHERE id_rol = @id_rol";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_rol", id);
                    await command.ExecuteNonQueryAsync();
                    response.Code = ResponseType.Success;
                    response.Message = "Rol eliminado correctamente";
                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al eliminar el rol: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return response;
            }
        }

        #endregion


    }
}
