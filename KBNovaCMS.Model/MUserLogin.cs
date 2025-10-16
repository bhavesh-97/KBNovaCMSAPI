namespace KBNovaCMS.Model
{
    public class MUserLogin
    {
        public int UserID { get; set; } = 0;
        public string FullName { get; set; } = string.Empty;
        public int MobileNumber { get; set; } = 0;
        public string EmailID { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsPasswordReset { get; set; } = false;
        public MCommonEntitiesMaster MCommonEntitiesMaster { get; set; } = new MCommonEntitiesMaster();
    }
}
