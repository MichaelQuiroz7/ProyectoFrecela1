using FRECELABK.Models;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;

namespace FRECELABK.Repositorio
{
    public class RepositorioVenta : IRepositorioVenta
    {

        private readonly string? cadenaConexion;

        public RepositorioVenta(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }

        #region Registrar Pedido

        public async Task<ApiResponse> RegistrarVenta(Venta venta)
        {

            using var connection = new MySqlConnection(cadenaConexion);
            try
            {
                await connection.OpenAsync();
                // Obtener la fecha y hora actual
                DateTime ahora = DateTime.Now;

                DateTime fechaActual = ahora.Date; // Solo la parte de la fecha

                // Formatear la hora como string de 6 dígitos (HHmmss)
                string horaActual = ahora.ToString("HHmmss");

                using var command = new MySqlCommand("sp_insertar_venta", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                // Parámetros de entrada
                command.Parameters.AddWithValue("p_cedula_cliente", venta.CedulaCliente);
                command.Parameters.AddWithValue("p_cedula_empleado", venta.CedulaEmpleado);
                command.Parameters.AddWithValue("p_fecha", fechaActual);   
                command.Parameters.AddWithValue("p_hora", horaActual);
                command.Parameters.AddWithValue("p_id_producto", venta.IdProducto);
                command.Parameters.AddWithValue("p_cantidad", venta.Cantidad);
                command.Parameters.AddWithValue("p_precio_unitario", venta.PrecioUnitario);

                // Parámetros de salida
                command.Parameters.Add("p_mensaje", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 2).Direction = System.Data.ParameterDirection.Output;

                await command.ExecuteNonQueryAsync();

                return new ApiResponse
                {
                    Code = command.Parameters["p_code"].Value.ToString(),
                    Message = command.Parameters["p_mensaje"].Value.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Code = "00",
                    Message = $"Error al registrar la venta: {ex.Message}"
                };
            }
            finally
            {
                await connection.CloseAsync();
            }


        }

        #endregion


        #region Obtener Pedido


        public async Task<DetalleVentaResponse> ObtenerDetalle(DetalleVentaRequest request)
        {
            using var connection = new MySqlConnection(cadenaConexion);
            try
            {
                // Validar que IdVentaBase64 no sea nulo o vacío
                if (string.IsNullOrWhiteSpace(request.IdVentaBase64))
                {
                    return new DetalleVentaResponse
                    {
                        Code = "00",
                        Message = "El ID de venta en base64 es requerido",
                        NombreProducto = null,
                        DescripcionProducto = null,
                        PrecioUnitario = null,
                        Cantidad = null,
                        PrecioTotal = null,
                        NombresCliente = null,
                        ApellidosCliente = null,
                        CedulaCliente = null
                    };
                }

                // Validar formato base64
                try
                {
                    Convert.FromBase64String(request.IdVentaBase64);
                }
                catch (FormatException)
                {
                    return new DetalleVentaResponse
                    {
                        Code = "00",
                        Message = "El ID de venta no es una cadena válida en base64",
                        NombreProducto = null,
                        DescripcionProducto = null,
                        PrecioUnitario = null,
                        Cantidad = null,
                        PrecioTotal = null,
                        NombresCliente = null,
                        ApellidosCliente = null,
                        CedulaCliente = null
                    };
                }

                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_obtener_detalle_venta", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                // Parámetro de entrada
                command.Parameters.AddWithValue("p_id_venta_base64", request.IdVentaBase64);

                // Parámetros de salida
                command.Parameters.Add("p_mensaje", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 2).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_nombre_producto", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_descripcion_producto", MySqlDbType.Text).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_precio_unitario", MySqlDbType.Decimal).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_cantidad", MySqlDbType.Int32).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_precio_total", MySqlDbType.Decimal).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_nombres_cliente", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_apellidos_cliente", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_cedula_cliente", MySqlDbType.VarChar, 20).Direction = System.Data.ParameterDirection.Output;

                await command.ExecuteNonQueryAsync();

                return new DetalleVentaResponse
                {
                    Code = command.Parameters["p_code"].Value.ToString(),
                    Message = command.Parameters["p_mensaje"].Value.ToString(),
                    NombreProducto = command.Parameters["p_nombre_producto"].Value != DBNull.Value ? command.Parameters["p_nombre_producto"].Value.ToString() : null,
                    DescripcionProducto = command.Parameters["p_descripcion_producto"].Value != DBNull.Value ? command.Parameters["p_descripcion_producto"].Value.ToString() : null,
                    PrecioUnitario = command.Parameters["p_precio_unitario"].Value != DBNull.Value ? Convert.ToDecimal(command.Parameters["p_precio_unitario"].Value) : null,
                    Cantidad = command.Parameters["p_cantidad"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["p_cantidad"].Value) : null,
                    PrecioTotal = command.Parameters["p_precio_total"].Value != DBNull.Value ? Convert.ToDecimal(command.Parameters["p_precio_total"].Value) : null,
                    NombresCliente = command.Parameters["p_nombres_cliente"].Value != DBNull.Value ? command.Parameters["p_nombres_cliente"].Value.ToString() : null,
                    ApellidosCliente = command.Parameters["p_apellidos_cliente"].Value != DBNull.Value ? command.Parameters["p_apellidos_cliente"].Value.ToString() : null,
                    CedulaCliente = command.Parameters["p_cedula_cliente"].Value != DBNull.Value ? command.Parameters["p_cedula_cliente"].Value.ToString() : null
                };
            }
            catch (Exception ex)
            {
                return new DetalleVentaResponse
                {
                    Code = "00",
                    Message = $"Error al obtener los detalles de la venta: {ex.Message}",
                    NombreProducto = null,
                    DescripcionProducto = null,
                    PrecioUnitario = null,
                    Cantidad = null,
                    PrecioTotal = null,
                    NombresCliente = null,
                    ApellidosCliente = null,
                    CedulaCliente = null
                };
            }
            finally
            {
                await connection.CloseAsync();
            }
        }


        #endregion


    }
}
