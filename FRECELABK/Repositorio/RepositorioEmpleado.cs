using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioEmpleado : IRepositorioEmpleado
    {

        private readonly string? cadenaConexion;
        ResponseModel response = new ResponseModel();

        public RepositorioEmpleado(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }

        #region Obtener Empleados

        public async Task<ResponseModel> ObtenerEmpleados()
        {
            ResponseModel response = new ResponseModel();
            List<EmpleadoDTO> empleados = new List<EmpleadoDTO>();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT nombres, apellidos, genero, fecha_nacimiento, foto, telefono FROM empleado WHERE id_rol != 1";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                DateTime fechaNacimiento = reader.GetDateTime(reader.GetOrdinal("fecha_nacimiento"));
                                int edad = DateTime.Today.Year - fechaNacimiento.Year;
                                if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;

                                EmpleadoDTO empleado = new EmpleadoDTO
                                {
                                    Nombres = reader.GetString(reader.GetOrdinal("nombres")),
                                    Apellidos = reader.GetString(reader.GetOrdinal("apellidos")),
                                    Genero = reader.GetString(reader.GetOrdinal("genero")),
                                    Edad = edad,
                                    Foto = reader.IsDBNull(reader.GetOrdinal("foto")) ? null : reader.GetString("foto"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono")
                                };
                                empleados.Add(empleado);
                            }
                        }
                    }
                    response.Message = "Empleados obtenidos correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = empleados;
                }
                catch (Exception ex)
                {
                    response.Data = null;
                    response.Code = ResponseType.Error;
                    response.Message = ex.Message;
                }
            }
            return response;
        }

        #endregion

        public async Task<ResponseModel> ObtenerIdRolPorIdEmpleado(int idEmpleado)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT id_rol FROM empleado WHERE id_empleado = @idEmpleado";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idEmpleado", idEmpleado);

                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            int idRol = Convert.ToInt32(result);
                            response.Message = "ID del rol obtenido correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = idRol;
                        }
                        else
                        {
                            response.Message = "Empleado no encontrado";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Data = null;
                    response.Code = ResponseType.Error;
                    response.Message = ex.Message;
                }
            }
            return response;
        }


    }
}
