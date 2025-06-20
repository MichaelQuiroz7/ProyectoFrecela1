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
            try
            {
                await connection.OpenAsync();
                // Obtener la fecha y hora actual
                DateTime ahora = DateTime.Now;

                DateTime fechaActual = ahora.Date; 

                // Formatear la hora como string de 6 dígitos (HHmmss)
                string horaActual = ahora.ToString("HHmmss");

                using var command = new MySqlCommand("sp_insertar_pedido", connection)
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
                command.Parameters.Add("p_code", MySqlDbType.VarChar, 255).Direction = System.Data.ParameterDirection.Output;
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


        #region ARMAR PDF

        public async Task<IActionResult> ConvertToPdf(LatexRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Cliente) || request.Productos == null || request.Productos.Count == 0)
            {
                return new BadRequestObjectResult("Datos de solicitud inválidos.");
            }

            try
            {


                // Datos de la empresa hardcodeados
                string empresa = "EMPRESA FRECELA";
                string direccion = "Av. 9 de Octubre, Ciudad de Guayaquil, Ecuador";
                string ruc = "1234567890001";

                // Tomar solo el primer producto (por si acaso se envían más)
                var producto = request.Productos[0];

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
        <strong>Cédula:</strong> {request.Cedula}<br>
        <strong>Fecha:</strong> {request.Fecha}<br>
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
        <tr><td>Subtotal (sin descuento):</td><td>${request.SubtotalSinDescuento:F2}</td></tr>
        <tr><td>Descuento:</td><td>${request.Descuento:F2}</td></tr>
        <tr><td>Subtotal (con descuento):</td><td>${request.SubtotalConDescuento:F2}</td></tr>
        <tr><td>IVA (14%):</td><td>${request.Iva:F2}</td></tr>
        <tr><td><strong>Total:</strong></td><td><strong>${request.Total:F2}</strong></td></tr>
    </table>
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

                    var updateQuery = "UPDATE venta SET descuento = @descuento WHERE id_venta = @idVenta";
                    using (var command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@descuento", request.Descuento);
                        command.Parameters.AddWithValue("@idVenta", producto.Codigo);
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

                    // Obtener id_producto y cantidad de la venta
                    string ventaQuery = "SELECT id_producto, cantidad FROM venta WHERE id_venta = @idVenta";
                    int idProducto = 0;
                    int cantidadVenta = 0;
                    using (MySqlCommand ventaCommand = new MySqlCommand(ventaQuery, connection, transaction))
                    {
                        ventaCommand.Parameters.AddWithValue("@idVenta", comprobante.IdVenta);
                        using (MySqlDataReader reader = (MySqlDataReader)await ventaCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                idProducto = reader.GetInt32("id_producto");
                                cantidadVenta = reader.GetInt32("cantidad");
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                                return new ResponseModel
                                {
                                    Message = "Venta no encontrada",
                                    Code = ResponseType.Error,
                                    Data = null
                                };
                            }
                        }
                    }

                    // Actualizar stock
                    string updateStockQuery = @"
                UPDATE producto 
                SET stock = stock - @cantidadVenta 
                WHERE id_producto = @idProducto AND stock >= @cantidadVenta";

                    using (MySqlCommand updateStockCommand = new MySqlCommand(updateStockQuery, connection, transaction))
                    {
                        updateStockCommand.Parameters.AddWithValue("@idProducto", idProducto);
                        updateStockCommand.Parameters.AddWithValue("@cantidadVenta", cantidadVenta);
                        int stockRowsAffected = await updateStockCommand.ExecuteNonQueryAsync();
                        if (stockRowsAffected == 0)
                        {
                            await transaction.RollbackAsync();
                            return new ResponseModel
                            {
                                Message = "Producto no encontrado o stock insuficiente",
                                Code = ResponseType.Error,
                                Data = null
                            };
                        }
                    }

                    // Cambiar estado de venta a PAGADO (valor fijo y seguro)

                    string updateVentaQuery = @"
    UPDATE venta 
    SET estado = 'PAGADO'
    WHERE id_venta = @idVenta AND estado = 'PENDIENTE'";


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




    }

}
