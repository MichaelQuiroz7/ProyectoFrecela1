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
                    string query = "SELECT id_empleado, nombres, apellidos, cedula, fecha_nacimiento, genero, id_rol, telefono, contrasenia FROM empleado";
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
                                    Telefono = reader.GetString(reader.GetOrdinal("telefono"))
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

        #region Validar Credenciales de acceso

        public async Task<ResponseModel> ValidarCredenciales(string cedula, string contrasenia)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = @"
                    SELECT e.id_empleado, e.nombres, e.apellidos, e.cedula, e.fecha_nacimiento, e.genero, e.id_rol, e.telefono, e.contrasenia, r.nombre_rol
                    FROM empleado e
                    JOIN rol r ON e.id_rol = r.id_rol
                    WHERE e.cedula = @cedula";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@cedula", cedula);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Empleado empleado = new Empleado
                                {
                                    IdEmpleado = reader.GetInt32("id_empleado"),
                                    Nombres = reader.GetString("nombres"),
                                    Apellidos = reader.GetString("apellidos"),
                                    Cedula = reader.GetString("cedula"),
                                    FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime("fecha_nacimiento")),
                                    Genero = reader.GetString("genero"),
                                    IdRol = reader.GetInt32("id_rol"),
                                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                                    Contrasenia = reader.IsDBNull(reader.GetOrdinal("contrasenia")) ? null : reader.GetString("contrasenia")
                                };
         
                                 if (empleado.Contrasenia != contrasenia)
                                {
                                    response.Message = "Contraseña incorrecta";
                                    response.Code = ResponseType.Error;
                                    response.Data = null;
                                }
                                else
                                {
                                    // Credenciales válidas
                                    response.Message = "Inicio de sesión exitoso";
                                    response.Code = ResponseType.Success;
                                    response.Data = new
                                    {
                                        empleado.IdEmpleado,
                                        empleado.Nombres,
                                        empleado.Apellidos,
                                        empleado.Cedula,
                                        empleado.IdRol
                                    };
                                }
                            }
                            else
                            {
                                response.Message = "Cédula no encontrada";
                                response.Code = ResponseType.Error;
                                response.Data = null;
                            }
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

        #endregion


    }
}
