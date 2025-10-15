namespace GTDC.Model
{
    public class ViewModelUserHomeDetails
    {
        public string ProjectCode { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string CopyRightMessage { get; set; } = string.Empty;
        public string DevelopedBy { get; set; } = string.Empty;
        public string LastUpdatedDate { get; set; } = string.Empty;
        public bool ErrorLog { get; set; } = true;
        public string DefaultUserRegistrationPassword { get; set; } = string.Empty;
        public string CustomErrorLog { get; set; } = string.Empty;
        public string StagingURL { get; set; } = string.Empty;
        public string ProductionURL { get; set; } = string.Empty;
        public string DevelopedByURL { get; set; } = string.Empty;
    }
}