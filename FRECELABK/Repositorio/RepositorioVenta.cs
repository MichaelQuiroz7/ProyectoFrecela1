using FRECELABK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FRECELABK.Repositorio
{
    public class RepositorioVenta : IRepositorioVenta   
    {

        private readonly string? cadenaConexion;
        ResponseModel response = new ResponseModel();


        public RepositorioVenta(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }

        #region Registrar Pedido

        public async Task<ApiResponse> RegistrarVenta(Venta venta)
        {
            using var connection = new MySqlConnection(cadenaConexion);
            MySqlTransaction transaction = null;

            try
            {
                await connection.OpenAsync();
                transaction = await connection.BeginTransactionAsync();

                // Obtener la fecha y hora actual
                DateTime ahora = DateTime.Now;
                DateTime fechaActual = ahora.Date;
                string horaActual = ahora.ToString("HHmmss");

                using var command = new MySqlCommand("sp_insertar_pedido", connection, transaction)
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
                command.Parameters.AddWithValue("p_observaciones", venta.Observaciones ?? string.Empty);

                // Parámetros de salida
                command.Parameters.Add("p_mensaje", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 2).Direction = System.Data.ParameterDirection.Output;

                await command.ExecuteNonQueryAsync();

                // Verificar el código de salida del procedimiento almacenado
                string responseCode = command.Parameters["p_code"].Value.ToString();
                if (responseCode != "01") // Suponiendo que "01" indica éxito
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse
                    {
                        Code = responseCode,
                        Message = command.Parameters["p_mensaje"].Value.ToString(),
                    };
                }

                // Obtener id_venta recién insertada (ajusta según tu procedimiento)
                int idVenta = 0;
                string ventaQuery = "SELECT LAST_INSERT_ID() as id_venta";
                using (var ventaCommand = new MySqlCommand(ventaQuery, connection, transaction))
                {
                    idVenta = Convert.ToInt32(await ventaCommand.ExecuteScalarAsync());
                    if (idVenta == 0)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse
                        {
                            Code = "00",
                            Message = "No se pudo obtener el ID de la venta",
                        };
                    }
                }

                // Obtener id_producto y cantidad de la venta
                string detalleQuery = "SELECT id_producto, cantidad FROM venta WHERE id_venta = @idVenta";
                int idProducto = 0;
                int cantidadVenta = 0;
                using (var detalleCommand = new MySqlCommand(detalleQuery, connection, transaction))
                {
                    detalleCommand.Parameters.AddWithValue("@idVenta", idVenta);
                    using (var reader = await detalleCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            idProducto = reader.GetInt32("id_producto");
                            cantidadVenta = reader.GetInt32("cantidad");
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponse
                            {
                                Code = "00",
                                Message = "Detalles de la venta no encontrados",
                            };
                        }
                    }
                }

                // Actualizar stock solo si la venta se insertó correctamente
                string updateStockQuery = @"
            UPDATE producto 
            SET stock = stock - @cantidadVenta 
            WHERE id_producto = @idProducto AND stock >= @cantidadVenta";

                using (var updateStockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                {
                    updateStockCommand.Parameters.AddWithValue("@idProducto", idProducto);
                    updateStockCommand.Parameters.AddWithValue("@cantidadVenta", cantidadVenta);
                    int stockRowsAffected = await updateStockCommand.ExecuteNonQueryAsync();
                    if (stockRowsAffected == 0)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse
                        {
                            Code = "00",
                            Message = "Producto no encontrado o stock insuficiente",
                        };
                    }
                }

                await transaction.CommitAsync();
                return new ApiResponse
                {
                    Code = responseCode,
                    Message = command.Parameters["p_mensaje"].Value.ToString(),
                };
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                return new ApiResponse
                {
                    Code = "00",
                    Message = $"Error al registrar la venta: {ex.Message}",
                };
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
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
                        CedulaCliente = null,
                        DireccionCliente = null,
                        TipoEntrega = null,
                        Observaciones = null
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
                        CedulaCliente = null,
                        DireccionCliente = null,
                        TipoEntrega = null,
                        Observaciones = null
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
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_nombre_producto", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_descripcion_producto", MySqlDbType.Text).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_precio_unitario", MySqlDbType.Decimal).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_cantidad", MySqlDbType.Int32).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_precio_total", MySqlDbType.Decimal).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_nombres_cliente", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_apellidos_cliente", MySqlDbType.VarChar, 100).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_cedula_cliente", MySqlDbType.VarChar, 20).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_direccion_cliente", MySqlDbType.VarChar, 20).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_tipo_entrega", MySqlDbType.VarChar, 20).Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add("p_observaciones", MySqlDbType.Text).Direction = System.Data.ParameterDirection.Output;

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
                    CedulaCliente = command.Parameters["p_cedula_cliente"].Value != DBNull.Value ? command.Parameters["p_cedula_cliente"].Value.ToString() : null,
                    DireccionCliente = command.Parameters["p_direccion_cliente"].Value != DBNull.Value ? command.Parameters["p_direccion_cliente"].Value.ToString() : null,
                    TipoEntrega = command.Parameters["p_tipo_entrega"].Value != DBNull.Value ? command.Parameters["p_tipo_entrega"].Value.ToString() : null,
                    Observaciones = command.Parameters["p_observaciones"].Value != DBNull.Value ? command.Parameters["p_observaciones"].Value.ToString() : null
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
                    CedulaCliente = null,
                    DireccionCliente = null,
                    TipoEntrega = null,
                    Observaciones = null
                };
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        #endregion


        #region ARMAR PDF

        public async Task<IActionResult> ConvertToPdf(LatexRequest request)
        {
            string tipoEntrega = "";
            string nombres = "";
            string apellidos = "";

            if (request == null || string.IsNullOrEmpty(request.Cliente) || request.Productos == null || request.Productos.Count == 0)
            {
                return new BadRequestObjectResult("Datos de solicitud inválidos.");
            }

            string query = "SELECT tipoentrega INTO @v_tipo_entrega FROM entrega WHERE id_venta = @p_id_venta;";
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                await connection.OpenAsync();
                var producto = request.Productos[0];

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@p_id_venta", producto.Codigo);
                    command.Parameters.Add("@v_tipo_entrega", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    await command.ExecuteNonQueryAsync();
                    tipoEntrega = command.Parameters["@v_tipo_entrega"].Value != DBNull.Value ? command.Parameters["@v_tipo_entrega"].Value.ToString() : null;
                }
            }

            string query2 = "SELECT e.nombres, e.apellidos FROM venta v JOIN empleado e ON v.id_empleado = e.id_empleado WHERE id_venta = @p_id_venta;";
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                await connection.OpenAsync();
                var producto = request.Productos[0];
                using (MySqlCommand command = new MySqlCommand(query2, connection))
                {
                    command.Parameters.AddWithValue("@p_id_venta", producto.Codigo);
                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            nombres = reader["nombres"] != DBNull.Value ? reader["nombres"].ToString() : null;
                            apellidos = reader["apellidos"] != DBNull.Value ? reader["apellidos"].ToString() : null;
                        }
                    }
                }
            }


            try
            {
                // Datos de la empresa
                string empresa = "EMPRESA FRECELA";
                string direccion = "Victor Emilio Estrada y Guayacanes (Urdesa), Ciudad de Guayaquil, Ecuador";
                string ruc = "1234567890001";

                // Tomar solo el primer producto
                var producto = request.Productos[0];

                // Calcular el costo de envío
                decimal costoEnvio = tipoEntrega == "ENTREGA A DOMICILIO" ? 5.00m : 0.00m;

                // Calcular el total final incluyendo el costo de envío
                decimal totalFinal = request.Total ;

                decimal valorDescuento = producto.Total - request.SubtotalConDescuento;

                // Generar HTML basado en los datos proporcionados
                string htmlContent = $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Orden de Pago</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ text-align: center; margin-bottom: 20px; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: bold; }}
        .company-info {{ font-size: 14px; margin-bottom: 20px; }}
        .client-info {{ margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
        th, td {{ border: 1px solid #000; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; font-weight: bold; }}
        .summary {{ width: 50%; border: none; }}
        .summary td {{ border: none; padding: 4px 0; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 20px; font-style: italic; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{empresa}</h1>
        <div class='company-info'>
            {direccion}<br>
            RUC: {ruc}
        </div>
    </div>

    <div class='client-info'>
        <strong>Cliente:</strong> {request.Cliente}<br>
        <strong>Celular:</strong> {request.Cedula}<br>
        <strong>Fecha:</strong> {request.Fecha}<br>
        <strong>Tipo de entrega:</strong> {tipoEntrega}<br>
    </div>

    <table>
        <thead>
            <tr>
                <th>Código</th>
                <th>Descripción</th>
                <th>Cantidad</th>
                <th>Precio Unitario</th>
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>{producto.Codigo}</td>
                <td>{producto.Descripcion}</td>
                <td>{producto.Cantidad}</td>
                <td>${producto.PrecioUnitario:F2}</td>
                <td>${producto.Total:F2}</td>
            </tr>
        </tbody>
    </table>

    <table class='summary'>
        <tr><td>Descuento:</td><td>${valorDescuento:F2}</td></tr>
        <tr><td>Subtotal (con descuento):</td><td>${request.SubtotalConDescuento:F2}</td></tr>
        <tr><td>IVA (15%):</td><td>${request.Iva:F2}</td></tr>
        <tr><td>Envío:</td><td>${costoEnvio:F2}</td></tr>
        <tr><td><strong>Total a Pagar:</strong></td><td><strong>${totalFinal:F2}</strong></td></tr>
    </table>
    <br>
    <br>
    <br>
    <div><strong>Vendedor:</strong> {nombres} {apellidos}</div>
    <br>
    <br>
    <br>

    <div class='footer'>
        <em>GRACIAS POR PREFERIRNOS!</em>
    </div>
</body>
</html>";

                // Descargar el navegador si no está disponible
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                // Lanzar PuppeteerSharp
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });

                await using var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlContent);

                // Actualizamos la tabla venta con el descuento
                using (var connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();

                    var updateQuery = "UPDATE venta SET descuento = @descuento, precio_total = @precioTotal, cantidad = @cantidad WHERE id_venta = @idVenta";
                    using (var command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@descuento", request.Descuento);
                        command.Parameters.AddWithValue("@idVenta", producto.Codigo);
                        command.Parameters.AddWithValue("@precioTotal", producto.Total);
                        command.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Generar el PDF
                var pdfStream = await page.PdfStreamAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    MarginOptions = new PuppeteerSharp.Media.MarginOptions
                    {
                        Top = "20mm",
                        Bottom = "20mm",
                        Left = "20mm",
                        Right = "20mm"
                    }
                });

                return new FileStreamResult(pdfStream, "application/pdf")
                {
                    FileDownloadName = "orden_de_venta.pdf"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Error al generar el PDF: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        #endregion


        #region REGISTRAR PAGO

        public async Task<ResponseModel> EditarComprobante(Comprobante comprobante)
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                MySqlTransaction transaction = null;
                try
                {
                    await connection.OpenAsync();
                    transaction = await connection.BeginTransactionAsync();

                    // Validación de datos básicos
                    if (comprobante.IdVenta <= 0 || comprobante.Fecha == default || comprobante.Hora == default)
                    {
                        return new ResponseModel
                        {
                            Message = "Datos de comprobante inválidos",
                            Code = ResponseType.Error,
                            Data = null
                        };
                    }

                    // Validar si el comprobante ya existe
                    string checkQuery = "SELECT COUNT(*) FROM comprobante WHERE id_venta = @idVenta";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection, transaction))
                    {
                        checkCommand.Parameters.AddWithValue("@idVenta", comprobante.IdVenta);
                        int count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            await transaction.RollbackAsync();
                            return new ResponseModel
                            {
                                Message = "El pago ya se realizó previamente",
                                Code = ResponseType.Error,
                                Data = null
                            };
                        }
                    }

                   


                    string updateVentaQuery = @"
    UPDATE venta 
    SET estado = 'ESPERA'
    WHERE id_venta = @idVenta";


                    using (MySqlCommand updateVentaCommand = new MySqlCommand(updateVentaQuery, connection, transaction))
                    {
                        updateVentaCommand.Parameters.AddWithValue("@idVenta", comprobante.IdVenta);
                        int updateRows = await updateVentaCommand.ExecuteNonQueryAsync();
                        if (updateRows == 0)
                        {
                            await transaction.RollbackAsync();
                            return new ResponseModel
                            {
                                Message = "No se pudo actualizar el estado de la venta (puede no estar pendiente)",
                                Code = ResponseType.Error,
                                Data = null
                            };
                        }
                    }

                    // Insertar comprobante
                    string insertComprobanteQuery = @"
                INSERT INTO comprobante (id_venta, imagen, fecha, hora)
                VALUES (@idVenta, @imagen, @fecha, @hora)";
                    using (MySqlCommand insertCommand = new MySqlCommand(insertComprobanteQuery, connection, transaction))
                    {
                        insertCommand.Parameters.AddWithValue("@idVenta", comprobante.IdVenta);
                        insertCommand.Parameters.AddWithValue("@imagen", comprobante.Imagen ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@fecha", comprobante.Fecha);
                        insertCommand.Parameters.AddWithValue("@hora", comprobante.Hora.ToString(@"hh\:mm\:ss"));
                        int rowsInserted = await insertCommand.ExecuteNonQueryAsync();
                        if (rowsInserted == 0)
                        {
                            // Revertir el estado si falla el insert
                            string revertQuery = @"
                        UPDATE venta 
                        SET estado = 'PENDIENTE' 
                        WHERE id_venta = @idVenta";
                            using (MySqlCommand revertCommand = new MySqlCommand(revertQuery, connection, transaction))
                            {
                                revertCommand.Parameters.AddWithValue("@idVenta", comprobante.IdVenta);
                                await revertCommand.ExecuteNonQueryAsync();
                            }
                            await transaction.RollbackAsync();
                            return new ResponseModel
                            {
                                Message = "Error al registrar el comprobante, estado revertido a PENDIENTE",
                                Code = ResponseType.Error,
                                Data = null
                            };
                        }
                    }

                    await transaction.CommitAsync();
                    return new ResponseModel
                    {
                        Message = "Comprobante registrado correctamente",
                        Code = ResponseType.Success,
                        Data = comprobante
                    };
                }
                catch (MySqlException ex)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    return new ResponseModel
                    {
                        Message = $"Error de base de datos: {ex.Message} (Código: {ex.Number})",
                        Code = ResponseType.Error,
                        Data = null
                    };
                }
                catch (Exception ex)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    return new ResponseModel
                    {
                        Message = $"Error inesperado: {ex.Message}",
                        Code = ResponseType.Error,
                        Data = null
                    };
                }
            }
        }


        #endregion


        #region Obtener Estadisticas


        public async Task<ResponseModel> ObtenerEstadisticasVentasPorMes()
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "CALL sp_estadisticas_ventas_por_mes();";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string jsonResult = reader.GetString("resultado");
                                var estadisticas = JsonSerializer.Deserialize<object>(jsonResult);
                                response.Message = "Estadísticas obtenidas correctamente";
                                response.Code = ResponseType.Success;
                                response.Data = estadisticas;
                            }
                            else
                            {
                                response.Message = "No se encontraron estadísticas";
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


        #region Agregar tipo de entrega

        public async Task<ResponseModel> InsertarEntrega(Entrega entrega)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO entrega (id_venta, tipoentrega, costoentrega, direccion) VALUES (@IdVenta, @TipoEntrega, @CostoEntrega, @Direccion);";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdVenta", entrega.IdVenta);
                        command.Parameters.AddWithValue("@TipoEntrega", entrega.TipoEntrega.ToString());
                        command.Parameters.AddWithValue("@CostoEntrega", (object)entrega.CostoEntrega ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Direccion", (object)entrega.Direccion ?? DBNull.Value);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            response.Message = "Entrega insertada correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = entrega;
                        }
                        else
                        {
                            response.Message = "No se pudo insertar la entrega";
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

        #endregion



        #region Obtener ventas Pagadas 

        public async Task<ResponseModel> ObtenerVentasPagadasORechazadasConDetalles()
        {
            ResponseModel response = new ResponseModel();
            List<(int IdVenta, string Estado)> ventasIds = new List<(int, string)>();
            try
            {
                // Paso 1: Obtener todos los id_venta y estado en una lista
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();
                    string queryVentas = "SELECT id_venta, estado FROM venta WHERE estado IN (@Estado1, @Estado2);";
                    using (MySqlCommand commandVentas = new MySqlCommand(queryVentas, connection))
                    {
                        commandVentas.Parameters.AddWithValue("@Estado1", "PAGADO");
                        commandVentas.Parameters.AddWithValue("@Estado2", "PENDIENTE");

                        using (MySqlDataReader readerVentas = (MySqlDataReader)await commandVentas.ExecuteReaderAsync())
                        {
                            while (await readerVentas.ReadAsync())
                            {
                                int idVenta = readerVentas.GetInt32("id_venta");
                                string estado = readerVentas.GetString("estado");
                                ventasIds.Add((idVenta, estado));
                            }
                        }
                    }
                }

                // Paso 2: Procesar cada id_venta con una nueva conexión
                List<DetalleVentaConsulta> detallesVentas = new List<DetalleVentaConsulta>();
                foreach (var (idVenta, estado) in ventasIds)
                {
                    
                    string idVentaBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(idVenta.ToString()));

                    using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                    {
                        await connection.OpenAsync();
                        using (MySqlCommand commandDetalle = new MySqlCommand("sp_obtener_detalle_venta", connection))
                        {
                            commandDetalle.CommandType = CommandType.StoredProcedure;

                            // Parámetros de entrada y salida
                            commandDetalle.Parameters.AddWithValue("@p_id_venta_base64", idVentaBase64);
                            commandDetalle.Parameters.Add("@p_mensaje", MySqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_code", MySqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_nombre_producto", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_descripcion_producto", MySqlDbType.Text).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_precio_unitario", MySqlDbType.Decimal).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_cantidad", MySqlDbType.Int32).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_precio_total", MySqlDbType.Decimal).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_nombres_cliente", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_apellidos_cliente", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_cedula_cliente", MySqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_direccion_cliente", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_tipo_entrega", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            commandDetalle.Parameters.Add("@p_observaciones", MySqlDbType.VarChar, 255).Direction = ParameterDirection.Output;

                            await commandDetalle.ExecuteNonQueryAsync();

                            // Obtener los valores de salida
                            string mensaje = commandDetalle.Parameters["@p_mensaje"].Value.ToString();
                            if (mensaje == "Detalles de la venta obtenidos exitosamente")
                            {
                                DetalleVentaConsulta detalle = new DetalleVentaConsulta
                                {
                                    Code = commandDetalle.Parameters["@p_code"].Value.ToString(),
                                    Message = mensaje,
                                    NombreProducto = commandDetalle.Parameters["@p_nombre_producto"].Value as string ?? "N/A",
                                    DescripcionProducto = commandDetalle.Parameters["@p_descripcion_producto"].Value as string,
                                    PrecioUnitario = commandDetalle.Parameters["@p_precio_unitario"].Value as decimal?,
                                    Cantidad = commandDetalle.Parameters["@p_cantidad"].Value as int?,
                                    PrecioTotal = commandDetalle.Parameters["@p_precio_total"].Value as decimal?,
                                    NombresCliente = commandDetalle.Parameters["@p_nombres_cliente"].Value as string ?? "N/A",
                                    ApellidosCliente = commandDetalle.Parameters["@p_apellidos_cliente"].Value as string ?? "N/A",
                                    CedulaCliente = commandDetalle.Parameters["@p_cedula_cliente"].Value as string ?? "N/A",
                                    DireccionCliente = commandDetalle.Parameters["@p_direccion_cliente"].Value as string ?? "N/A",
                                    TipoEntrega = commandDetalle.Parameters["@p_tipo_entrega"].Value as string ?? "N/A",
                                    EstadoEntrega = estado ,
                                    Observaciones = commandDetalle.Parameters["@p_observaciones"].Value as string ?? string.Empty
                                };
                                detallesVentas.Add(detalle);
                            }
                        }
                    }
                }

                if (detallesVentas.Any())
                {
                    response.Message = "Detalles de ventas obtenidos correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = detallesVentas;
                }
                else
                {
                    response.Message = "No se encontraron ventas con estado PAGADO o RECHAZADO";
                    response.Code = ResponseType.Success;
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Code = ResponseType.Error;
                response.Message = ex.Message;
            }
            return response;
        }

        #endregion


        #region Venta Actualizada 

        public async Task<ResponseModel> ActualizarEstadoVenta2(DetalleVentaConsulta detalle)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "UPDATE venta SET estado = @Estado WHERE id_venta = @IdVenta;";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdVenta", detalle.Code);
                        command.Parameters.AddWithValue("@Estado", detalle.EstadoEntrega);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            response.Message = "Estado de la venta actualizado correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = new { IdVenta = detalle.Code, Estado = detalle.EstadoEntrega };
                        }
                        else
                        {
                            response.Message = "No se encontró la venta o no se pudo actualizar";
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



        public async Task<ResponseModel> ActualizarEstadoVenta(DetalleVentaConsulta detalle)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                MySqlTransaction transaction = null;
                try
                {
                    await connection.OpenAsync();
                    transaction = await connection.BeginTransactionAsync();

                    string query = "UPDATE venta SET estado = @Estado WHERE id_venta = @IdVenta;";
                    using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IdVenta", detalle.Code);
                        command.Parameters.AddWithValue("@Estado", detalle.EstadoEntrega);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            // If status is RECHAZADO, update stock
                            if (detalle.EstadoEntrega == "RECHAZADO")
                            {
                                // Obtener id_producto y cantidad de la venta
                                string detalleQuery = "SELECT id_producto, cantidad FROM venta WHERE id_venta = @IdVenta";
                                int idProducto = 0;
                                int cantidadVenta = 0;
                                using (var detalleCommand = new MySqlCommand(detalleQuery, connection, transaction))
                                {
                                    detalleCommand.Parameters.AddWithValue("@IdVenta", detalle.Code);
                                    using (var reader = await detalleCommand.ExecuteReaderAsync())
                                    {
                                        if (await reader.ReadAsync())
                                        {
                                            idProducto = reader.GetInt32("id_producto");
                                            cantidadVenta = reader.GetInt32("cantidad");
                                        }
                                        else
                                        {
                                            await transaction.RollbackAsync();
                                            response.Message = "Detalles de la venta no encontrados";
                                            response.Code = ResponseType.Error;
                                            response.Data = null;
                                            return response;
                                        }
                                    }
                                }

                                // Actualizar stock sumando la cantidad
                                string updateStockQuery = @"
                            UPDATE producto 
                            SET stock = stock + @CantidadVenta 
                            WHERE id_producto = @IdProducto";
                                using (var stockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                                {
                                    stockCommand.Parameters.AddWithValue("@CantidadVenta", cantidadVenta);
                                    stockCommand.Parameters.AddWithValue("@IdProducto", idProducto);
                                    int stockRowsAffected = await stockCommand.ExecuteNonQueryAsync();
                                    if (stockRowsAffected == 0)
                                    {
                                        await transaction.RollbackAsync();
                                        response.Message = "No se pudo actualizar el stock del producto";
                                        response.Code = ResponseType.Error;
                                        response.Data = null;
                                        return response;
                                    }
                                }
                            }

                            await transaction.CommitAsync();
                            response.Message = "Estado de la venta actualizado correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = new { IdVenta = detalle.Code, Estado = detalle.EstadoEntrega };
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            response.Message = "No se encontró la venta o no se pudo actualizar";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync();
                    response.Data = null;
                    response.Code = ResponseType.Error;
                    response.Message = ex.Message;
                }
            }
            return response;
        }

        #endregion



        #region Venta en Espera

        public async Task<ResponseModel> ObtenerComprobantesEnEsperaConDetalles()
        {
            ResponseModel response = new ResponseModel();
            List<ComprobanteConsulta> comprobantes = new List<ComprobanteConsulta>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();
                    string query = @"
                SELECT 
                    v.id_venta,
                    v.estado,
                    c.imagen,
                    c.fecha
                FROM venta v
                LEFT JOIN comprobante c ON c.id_venta = v.id_venta
                WHERE v.estado = @Estado";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Estado", "ESPERA");

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int idVenta = reader.GetInt32("id_venta");
                                string estado = reader.GetString("estado");
                                byte[] imagenBytes = reader["imagen"] as byte[] ?? new byte[0]; 
                                string imagenBase64 = Convert.ToBase64String(imagenBytes);
                                DateTime? fecha = reader["fecha"] as DateTime?; 

                                ComprobanteConsulta comprobante = new ComprobanteConsulta
                                {
                                    IdVenta = idVenta,
                                    Estado = estado,
                                    ImagenBase64 = imagenBase64,
                                    Fecha = fecha
                                };
                                comprobantes.Add(comprobante);
                            }
                        }
                    }
                }

                if (comprobantes.Any())
                {
                    response.Message = "Comprobantes en espera obtenidos correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = comprobantes;
                }
                else
                {
                    response.Message = "No se encontraron comprobantes con estado ESPERA";
                    response.Code = ResponseType.Success;
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Code = ResponseType.Error;
                response.Message = ex.Message;
            }
            return response;
        }



        #endregion


        #region Obtener Comprobantes en Espera con Detalles

        public async Task<ResponseModel> ObtenerComprobantesEnEsperaConDetallesdos()
        {
            ResponseModel response = new ResponseModel();
            List<ComprobanteConsultados> comprobantes = new List<ComprobanteConsultados>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();

                    // Paso 1: Obtener id_venta, estado, imagen y fecha de las ventas en espera
                    string query = @"
                SELECT 
                    v.id_venta,
                    v.estado,
                    c.imagen,
                    c.fecha
                FROM venta v
                LEFT JOIN comprobante c ON c.id_venta = v.id_venta
                WHERE v.estado = @Estado";

                    using (MySqlCommand commandBase = new MySqlCommand(query, connection))
                    {
                        commandBase.Parameters.AddWithValue("@Estado", "ESPERA");

                        using (MySqlDataReader reader = (MySqlDataReader)await commandBase.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int idVenta = reader.GetInt32("id_venta");
                                string estado = reader.GetString("estado");
                                byte[] imagenBytes = reader["imagen"] as byte[] ?? new byte[0]; 
                                string imagenBase64 = Convert.ToBase64String(imagenBytes);
                                DateTime? fecha = reader["fecha"] as DateTime?; 

                                // Paso 2: Obtener detalles adicionales usando el stored procedure
                                string idVentaBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(idVenta.ToString()));
                                DetalleVentaResponse detalle = await ObtenerDetalle(new DetalleVentaRequest { IdVentaBase64 = idVentaBase64 });

                                    ComprobanteConsultados comprobante = new ComprobanteConsultados
                                    {
                                        IdVenta = idVenta,
                                        Estado = estado,
                                        ImagenBase64 = imagenBase64,
                                        Fecha = fecha,
                                        Code = detalle.Code,
                                        Message = detalle.Message,
                                        NombreProducto = detalle.NombreProducto,
                                        DescripcionProducto = detalle.DescripcionProducto,
                                        PrecioUnitario = detalle.PrecioUnitario,
                                        Cantidad = detalle.Cantidad,
                                        PrecioTotal = detalle.PrecioTotal,
                                        NombresCliente = detalle.NombresCliente,
                                        ApellidosCliente = detalle.ApellidosCliente,
                                        CedulaCliente = detalle.CedulaCliente,
                                        DireccionCliente = detalle.DireccionCliente,
                                        TipoEntrega = detalle.TipoEntrega,
                                        Observaciones = detalle.Observaciones
                                    };
                                    comprobantes.Add(comprobante);
                                
                            }
                        }
                    }
                }

                if (comprobantes.Any())
                {
                    response.Message = "Comprobantes en espera obtenidos correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = comprobantes;
                }
                else
                {
                    response.Message = "No se encontraron comprobantes con estado ESPERA";
                    response.Code = ResponseType.Success;
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Code = ResponseType.Error;
                response.Message = ex.Message;
            }
            return response;
        }

        #endregion


        #region Obtener IDs de Ventas en Base64

        public async Task<ResponseModel> ObtenerIdsVentasBase64()
        {
            ResponseModel response = new ResponseModel();
            List<string> idsBase64 = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query =  "SELECT CONCAT( TO_BASE64(CAST(v.id_venta AS CHAR) ), '|', ' ',c.nombres, ' ', c.apellidos) AS resultado " +
                                    "FROM venta v " +
                                    "JOIN cliente c ON v.id_cliente = c.id_cliente " +
                                    "WHERE v.estado NOT IN('RECHAZADO', 'ESPERA', 'COMPLETADA') " +
                                    "ORDER BY v.id_venta DESC; ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string idBase64 = reader.GetString("resultado");
                                idsBase64.Add(idBase64);
                            }

                            if (idsBase64.Count > 0)
                            {
                                response.Message = "IDs de ventas obtenidos correctamente";
                                response.Code = ResponseType.Success;
                                response.Data = idsBase64;
                            }
                            else
                            {
                                response.Message = "No se encontraron ventas con los criterios especificados";
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
                    response.Message = $"Error al obtener los IDs de ventas: {ex.Message}";
                }
            }

            return response;
        }

        #endregion


        #region Obtener Ventas por Cédula del Cliente

        public async Task<ResponseModel> ObtenerVentasPorCedulaCliente(string cedula)
        {
            ResponseModel response = new ResponseModel();
            List<SaleDetails> ventas = new List<SaleDetails>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();

                    string query = @"
                    SELECT 
                        v.id_venta,
                        p.nombre AS nombreProducto,
                        v.cantidad,
                        v.precio_total,
                        v.fecha,
                        v.estado
                    FROM venta v
                    INNER JOIN cliente c ON v.id_cliente = c.id_cliente
                    INNER JOIN producto p ON v.id_producto = p.id_producto
                    WHERE c.cedula = @Cedula";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Cedula", cedula);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                SaleDetails venta = new SaleDetails
                                {
                                    IdVenta = reader.GetInt32("id_venta"),
                                    NombreProducto = reader.GetString("nombreProducto"),
                                    Cantidad = reader.GetInt32("cantidad"),
                                    PrecioTotal = reader.GetDecimal("precio_total"),
                                    Fecha = reader.GetDateTime("fecha"),
                                    Estado = reader.GetString("estado")
                                };
                                ventas.Add(venta);
                            }
                        }
                    }
                }

                if (ventas.Any())
                {
                    response.Message = "Ventas obtenidas correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = ventas;
                }
                else
                {
                    response.Message = $"No se encontraron ventas para el cliente con cédula {cedula}";
                    response.Code = ResponseType.Success;
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Code = ResponseType.Error;
                response.Message = $"Error al obtener las ventas: {ex.Message}";
            }

            return response;
        }

        #endregion


        #region Obtener Ventas por Cédula del Empleado

        public async Task<ResponseModel> ObtenerVentasPorCedulaEmpleado(string cedula)
        {
            ResponseModel response = new ResponseModel();
            List<SaleDetails> ventas = new List<SaleDetails>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();

                    string query = @"
            SELECT 
                v.id_venta,
                p.nombre AS nombreProducto,
                v.cantidad,
                v.precio_total,
                v.fecha,
                v.estado
            FROM venta v
            INNER JOIN empleado e ON v.id_empleado = e.id_empleado
            INNER JOIN producto p ON v.id_producto = p.id_producto
            WHERE e.cedula = @Cedula AND v.estado NOT IN ('RECHAZADO', 'COMPLETADA')";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Cedula", cedula);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                SaleDetails venta = new SaleDetails
                                {
                                    IdVenta = reader.GetInt32("id_venta"),
                                    NombreProducto = reader.GetString("nombreProducto"),
                                    Cantidad = reader.GetInt32("cantidad"),
                                    PrecioTotal = reader.GetDecimal("precio_total"),
                                    Fecha = reader.GetDateTime("fecha"),
                                    Estado = reader.GetString("estado")
                                };
                                ventas.Add(venta);
                            }
                        }
                    }
                }

                if (ventas.Any())
                {
                    response.Message = "Ventas obtenidas correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = ventas;
                }
                else
                {
                    response.Message = $"No se encontraron ventas para el empleado con cédula {cedula}";
                    response.Code = ResponseType.Success;
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Code = ResponseType.Error;
                response.Message = $"Error al obtener las ventas: {ex.Message}";
            }

            return response;
        }

        #endregion



        #region Ingresar venta Empleado

        public async Task<ResponseModel> IngresarVentaEmpleado(IngresarVentaRequest request)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();

                    // Validar cédula del cliente
                    string queryCliente = "SELECT id_cliente FROM cliente WHERE cedula = @CedulaCliente";
                    int idCliente;
                    using (MySqlCommand commandCliente = new MySqlCommand(queryCliente, connection))
                    {
                        commandCliente.Parameters.AddWithValue("@CedulaCliente", request.CedulaCliente);
                        var result = await commandCliente.ExecuteScalarAsync();
                        if (result == null)
                        {
                            response.Message = $"No se encontró un cliente con cédula {request.CedulaCliente}";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                            return response;
                        }
                        idCliente = Convert.ToInt32(result);
                    }

                    // Validar cédula del empleado
                    string queryEmpleado = "SELECT id_empleado FROM empleado WHERE cedula = @CedulaEmpleado";
                    int idEmpleado;
                    using (MySqlCommand commandEmpleado = new MySqlCommand(queryEmpleado, connection))
                    {
                        commandEmpleado.Parameters.AddWithValue("@CedulaEmpleado", request.CedulaEmpleado);
                        var result = await commandEmpleado.ExecuteScalarAsync();
                        if (result == null)
                        {
                            response.Message = $"No se encontró un empleado con cédula {request.CedulaEmpleado}";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                            return response;
                        }
                        idEmpleado = Convert.ToInt32(result);
                    }

                    // Validar existencia del producto y stock
                    string queryProducto = "SELECT stock FROM producto WHERE id_producto = @IdProducto";
                    int stock;
                    using (MySqlCommand commandProducto = new MySqlCommand(queryProducto, connection))
                    {
                        commandProducto.Parameters.AddWithValue("@IdProducto", request.IdProducto);
                        var result = await commandProducto.ExecuteScalarAsync();
                        if (result == null)
                        {
                            response.Message = $"No se encontró un producto con ID {request.IdProducto}";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                            return response;
                        }
                        stock = Convert.ToInt32(result);
                        if (stock < request.Cantidad)
                        {
                            response.Message = $"Stock insuficiente para el producto con ID {request.IdProducto}. Stock disponible: {stock}";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                            return response;
                        }
                    }

                    // Insertar la venta
                    string query = @"
                INSERT INTO venta (
                    id_cliente, id_empleado, fecha, hora, id_producto, cantidad, 
                    precio_unitario, precio_total, estado, descuento
                ) VALUES (
                    @IdCliente, @IdEmpleado, @Fecha, @Hora, @IdProducto, @Cantidad, 
                    @PrecioUnitario, @PrecioTotal, @Estado, @Descuento
                )";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCliente", idCliente);
                        command.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                        command.Parameters.AddWithValue("@Fecha", request.Fecha);
                        command.Parameters.AddWithValue("@Hora", request.Hora);
                        command.Parameters.AddWithValue("@IdProducto", request.IdProducto);
                        command.Parameters.AddWithValue("@Cantidad", request.Cantidad);
                        command.Parameters.AddWithValue("@PrecioUnitario", request.PrecioUnitario);
                        command.Parameters.AddWithValue("@PrecioTotal", request.PrecioTotal);
                        command.Parameters.AddWithValue("@Estado", request.Estado);
                        command.Parameters.AddWithValue("@Descuento", request.Descuento / 100); // Convertir porcentaje a decimal

                        await command.ExecuteNonQueryAsync();
                    }

                    // Actualizar stock del producto
                    string updateStockQuery = "UPDATE producto SET stock = stock - @Cantidad WHERE id_producto = @IdProducto";
                    using (MySqlCommand updateCommand = new MySqlCommand(updateStockQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Cantidad", request.Cantidad);
                        updateCommand.Parameters.AddWithValue("@IdProducto", request.IdProducto);
                        await updateCommand.ExecuteNonQueryAsync();
                    }

                    response.Message = "Venta registrada correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Error al registrar la venta: {ex.Message}";
                response.Code = ResponseType.Error;
                response.Data = null;
            }

            return response;
        }

        #endregion


    }

}
