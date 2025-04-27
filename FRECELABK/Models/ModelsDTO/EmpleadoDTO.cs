namespace FRECELABK.Models.ModelsDTO
{
    public class EmpleadoDTO
    {
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Genero { get; set; }
        public int Edad { get; set; } 
        public byte[]? Foto { get; set; }
    }
}
