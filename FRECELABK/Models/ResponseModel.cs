namespace FRECELABK.Models
{
    public class ResponseModel
    {
        public ResponseType Code { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }

    public enum ResponseType
    {
        Error = 00,
        Success = 01
    }
}
