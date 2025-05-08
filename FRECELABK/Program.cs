using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Agregar DbContext
builder.Services.AddDbContext<EmpresaContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("Conexion"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Conexion"))));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registro el repositorio
builder.Services.AddScoped<IRepositorioProducto, RepositorioProducto>();

// Registro el repositorio de empleado
builder.Services.AddScoped<IRepositorioEmpleado, RepositorioEmpleado>();

// Registro el repositorio de Imagen
builder.Services.AddScoped<IRepositorioImagen, RepositorioImagen>();

// Registro el repositorio de Alerta
builder.Services.AddScoped<IRepositorioAlerta, RepositorioAlerta>();

//Add Cors
builder.Services.AddCors(options => options.AddPolicy("AllowWebApp",
    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWebApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
