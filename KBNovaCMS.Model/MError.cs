namespace KBNovaCMS.Model
{
    public class MError
    {
        public string ServiceName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string AdditionalDetails { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? STR_TimeStamp { get; set; } = string.Empty;
        public DateTime? Date_TimeStamp { get; set; }
        public string? UniqueGUID { get; set; } = string.Empty;
    }
}