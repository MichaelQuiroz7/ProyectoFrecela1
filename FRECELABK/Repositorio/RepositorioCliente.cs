using FRECELABK.Models;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioCliente : IRepositorioCliente
    {

        readonly string? cadenaConexion;

        public RepositorioCliente(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }

        public async Task<ApiResponse> RegistrarCliente(ClienteRequest request)
        {
            using var connection = new MySqlConnection(cadenaConexion);
            try
            {
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_insertar_cliente", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                // Parámetros de entrada
                command.Parameters.AddWithValue("p_cedula", request.Cedula);
                command.Parameters.AddWithValue("p_nombres", request.Nombres);
                command.Parameters.AddWithValue("p_apellidos", request.Apellidos);
                command.Parameters.AddWithValue("p_direccion", request.Direccion);
                command.Parameters.AddWithValue("p_correo_electronico", request.CorreoElectronico);
                command.Parameters.AddWithValue("p_clave", request.Clave);
                command.Parameters.AddWithValue("p_telefono", string.IsNullOrEmpty(request.Telefono) ? (object)DBNull.Value : request.Telefono);

                // Parámetros de salida
                command.Parameters.Add("p_mensaje", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 2).Direction = System.Data.ParameterDirection.Output;

                await command.ExecuteNonQueryAsync();

                // Obtener valores de salida
                var response = new ApiResponse
                {
                    Code = command.Parameters["p_code"].Value.ToString(),
                    Message = command.Parameters["p_mensaje"].Value.ToString()
                };

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Code = "00",
                    Message = $"Error al registrar el cliente: {ex.Message}"
                };
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<ResponseModel> IniciarSesion(LoginCliente request)
        {
            ResponseModel response = new ResponseModel();
            using var connection = new MySqlConnection(cadenaConexion);
            try
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("sp_iniciar_sesion", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("p_correo_electronico", request.CorreoElectronico);
                command.Parameters.AddWithValue("p_clave", request.Clave);
                command.Parameters.Add("p_mensaje", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_nombres", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_apellidos", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_cedula", MySqlDbType.VarChar, 20).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_direccion", MySqlDbType.VarChar, 200).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_telefono", MySqlDbType.VarChar, 15).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 2).Direction = System.Data.ParameterDirection.Output;
                await command.ExecuteNonQueryAsync();
                if (command.Parameters["p_code"].Value.ToString() == "00")
                {
                    response.Code = ResponseType.Error;
                    response.Message = command.Parameters["p_mensaje"].Value.ToString();
                    return response;
                }
                else
                {
                    response.Code = ResponseType.Success;
                    response.Message = command.Parameters["p_mensaje"].Value.ToString();
                    response.Data = new
                    {
                        Nombres = command.Parameters["p_nombres"].Value.ToString(),
                        Apellidos = command.Parameters["p_apellidos"].Value.ToString(),
                        Cedula = command.Parameters["p_cedula"].Value.ToString(),
                        Direccion = command.Parameters["p_direccion"].Value.ToString(),
                        Telefono = command.Parameters["p_telefono"].Value.ToString()
                    };
                }

            }
            catch (Exception ex)
            {
                response.Code = ResponseType.Error;
                response.Message = $"Error al iniciar sesión: {ex.Message}";
            }
            finally
            {
                await connection.CloseAsync();
            }
            return response;
        }

    }
}
