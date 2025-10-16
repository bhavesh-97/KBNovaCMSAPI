namespace KBNovaCMS.Model
{
    public class MUserLogin
    {
        public int userID { get; set; } = 0;
        public string fullName { get; set; } = string.Empty;
        public int mobileNumber { get; set; } = 0;
        public string emailID { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public bool isPasswordReset { get; set; } = false;
        public MCommonEntitiesMaster mCommonEntitiesMaster { get; set; } = new MCommonEntitiesMaster();
    }
}
