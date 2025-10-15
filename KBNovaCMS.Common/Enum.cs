using System.ComponentModel;

namespace KBNovaCMS.Common.Enums
{
    public enum ValidationDataType
    {
        [Description(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
        EmailID,

        [Description(@"^[A-Za-z0-9/]+$")]
        AllowURL,

        #region Letters Regex

        [Description(@"^[A-Za-z]+$")]
        LettersOnly,

        /*[Description(@"^(?=.*[a-zA-Z])[a-zA-Z\s]{2,}$")]*/
        [Description(@"^[a-zA-Z\s]*$")]
        LettersWithWhiteSpace,

        [Description(@"^[\u0A80-\u0AE5\u0AF0-\u0AFF]+$")]
        LettersOnlyGujarati,

        [Description(@"^(?=.*[\u0A80-\u0AE5\u0AF0-\u0AFF])[\u0A80-\u0AE5\u0AF0-\u0AFF\s]{2,}$")]
        LettersWithWhiteSpaceGujarati,

        #endregion

        #region Alpha Numeric Regex

        [Description(@"^[a-zA-Z0-9]+$")]
        AlphanumericOnly,

        // [Description(@"^[a-zA-Z0-9\s]+$")] // This will allowed only letters , numbers , space
        // [Description(@"^[a-zA-Z0-9]+(\s[a-zA-Z0-9]+)*$")] // prevent leading or trailing spaces
        [Description(@"^[a-zA-Z0-9\s]+$")]
        AlphanumericWithWhiteSpace,

        // [Description(@"^[\u0A80-\u0AFF0-9]+$")]
        [Description(@"^[\u0A80-\u0AE5\u0AF0-\u0AFF0-9]+$")]
        AlphanumericOnlyGujarati,

        // [Description(@"^[\u0A80-\u0AFF0-9\s]+$")] // This will allowed only letters , numbers , space in only gujarati language
        // [Description(@"^[\u0A80-\u0AFF0-9]+(\s[\u0A80-\u0AFF0-9]+)*$")] // prevent leading or trailing spaces
        [Description(@"^[\u0A80-\u0AFF0-9]+(\s[\u0A80-\u0AFF0-9]+)*$")]
        AlphanumericWithWhiteSpaceGujarati,

        #endregion

        #region Numbers Regex

        [Description(@"^[0-9\b]+$")]
        NumberOnly,

        [Description(@"^[1-9\b]+$")]
        NumberOnlyWithoutZero,

        [Description(@"^(|-?\d+)$")]
        NumberOnlyPositiveAndNegative,

        [Description(@"^([0-9]*\.?[0-9]*)$")]
        DecimalOnly,

        [Description(@"^-?[0-9]\d*(\.\d+)?$")]
        DecimalOnlyPositiveAndNegative,

        [Description(@"^[0-9]{10}$")]
        MobileNo,

        [Description(@"^(?:(?:\+|0{0,2})91(\s*[\-]\s*)?|[0]?)?[6789]\d{9}$")]
        MobileNoWithSeries,

        [Description(@"^(?=(?:\D*\d){10,12}\D*$)[0-9 \-()\\\/]{1,16}$")]
        FaxOrPhoneNo,

        #endregion

        [Description(@"^[0-9]{2}[a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}[1-9a-zA-Z]{1}Z[0-9a-zA-Z]{1}$")]
        GSTNumber,

        [Description(@"^([\w\s\/#.,'-]{3,})(?:,\s?([\w\s\/#.,'-]{3,}))?(?:,\s?([\w\s'-]+))?(?:,\s?([A-Za-z\s'-]+))?(?:,\s?(\d{6}))?$")]
        Address,

        [Description(@"^[A-Za-z]{5}\d{4}[A-Za-z]{1}$")]
        PANNumber,

        [Description(@"^\d{12}$")]
        AadharNumber,

        [Description(@"^\d{15}$")]
        RationNumber,

        [Description(@"^\d{8,18}$")]
        AccountNumber,

        [Description(@"[A-Z|a-z]{4}[0][a-zA-Z0-9]{6}$")]
        IFSCCode,

        #region Calendar Validation Regex

        [Description(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((17|18|19|20)\d\d))$")]
        DateOnly,

        [Description(@"^[0]*[1-9]$|^[0]*1[0-2]$")]
        MonthOnly,

        [Description(@"^\d{4}$")]
        YearOnly,

        [Description(@"^(20[0-9]{2}|19[0-9]{2})-(0[1-9]|1[0-2])$")]
        MonthYearOnly,

        [Description(@"^[0]*[1-9]{1}$|^[0-9]{2}$")]
        Age,

        [Description(@"^(1[89]|[2-9]\d)$")]
        AgeAbove18Years,

        #endregion

        [Description(@"[<>]")]
        XSSPrevent,

        [Description(@"^[0-9]{6}$")]
        Pincode,

        [Description(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?!.*\s).{8,15}$")]
        Password,

        [Description(@"^((0|[1-9]\d?)(\.\d{1,2})?|100(\.00?)?)$$")]
        Percentage,

        [Description(@"^[a-zA-Z0-9\s]+$")]
        NotAllowedAnySpecialCharacters,

        [Description(@"^[A-Za-z0-9\/\-\s]+$")]
        HouseOrSurveyNumber

    }

    public enum InformationStatus
    {
        Pending = 1,
        Submitted = 2,
        Edited = 3
    }

    public enum ServiceFlowType
    {
        Static,
        Dynamic,
        NoFlow
    }

    public enum PopupMessageType
    {
        Success,
        Error,
        Warning,
        Info,
        Question
    }

    public enum MenuType
    {
        ParentMenu,
        ChildMenu,
        InnerPage
    }

    public enum CMSMenuResType
    {
        StaticPage,
        DynamicPage
    }

    public enum CMSMenuType
    {
        ParentMenu,
        ChildMenu,
        InnerPage
    }

    public enum FileType
    {
        [Description("pdf,doc,docx,ppt,pptx,xls,xlsx,txt")]
        Document,  // For documents (pdf, doc, docx, ppt, pptx, xls, xlsx, txt)

        [Description("pdf,PDF")]
        PDF,  // For PDFs only

        [Description("jpg,jpeg,png")]
        Image,  // For images (jpg, jpeg, png)

        [Description("mp3,wav")]
        Audio,  // For audio files (mp3, wav)

        [Description("mp4,avi,mkv")]
        Video  // For video files (mp4, avi, mkv)
    }

    public enum FileSize
    {
        [Description("5")]
        Document,  // Max size for documents (5MB)

        [Description("5")]
        PDF,  // Max size for PDFs (5MB)

        [Description("2")]
        Image,  // Max size for images (2MB)

        [Description("10")]
        Audio,  // Max size for audio files (10MB)

        [Description("50")]
        Video,  // Max size for video files (50MB)

        Unknown
    }

    public enum ServiceDisplayType
    {
        Single,
        MultiTab
    }

    public enum FormType
    {
        MainForm,
        SubFormAsGrid,
        SubForm
    }

    public enum Month
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public enum TenderDocumentType
    {
        PreBid,
        Other
    }

    public enum RequestStatus
    {
        Return = 1,
        Reject,
        Forward,
        Recommend
    }

    public enum DecisionType
    {
        Yes = 1,
        No
    }

    [Description("Gender Type")]
    public enum GenderType
    {
        Male = 1,
        Female,
        Transgender
    }

    [Description("Salutation Type")]
    public enum SalutationType
    {
        Mr = 1,
        Mrs
    }

    public enum ControlInputType
    {
        None,
        Text,
        Number,
        Email,
        Password,
        MobileNo,
        Pincode,
        Dropdown,
        Date,
        Time,
        Age
    }

    public enum ValidationControlInputType
    {
        EmailId,
        LettersOnly,
        LettersWithWhiteSpace,
        LettersOnlyGujarati,
        LettersWithWhiteSpaceGujarati,
        AlphanumericOnly,
        AlphanumericWithWhiteSpace,
        AlphanumericOnlyGujarati,
        AlphanumericWithWhiteSpaceGujarati,
        NumberOnly,
        NumberOnlyWithoutZero,
        AadharCardNumber,
        NumberOnlyPositiveAndNegative,
        DecimalOnly,
        DecimalOnlyPositiveAndNegative,
        MobileNo,
        MobileNoWithSeries,
        FaxOrPhoneNo,
        GSTNumber,
        PANNumber,
        AccountNumber,
        IFSCCode,
        DateOnly,
        MonthsOnly,
        YearsOnly,
        MonthYearOnly,
        Age,
        AgeAbove18Years,
        XSSPrevent,
        HouseOrSurveyNumber
    }

    #region Key-List

    public enum KeysListMCouchDB
    {
        AllowCouchDBStore,
        CouchDBURL,
        CouchDBDbName,
        CouchDBUser,
        FolderName
    }

    public enum KeysListSMTPServer
    {
        SMTPServer,
        SMTPPort,
        SMTPAccount,
        SMTPPassword,
        SMTPFromEmail,
        SMTPIsSecure,
        SMTPIsTest,
        TestSMTPServer,
        TestSMTPPort,
        TestSMTPAccount,
        TestSMTPPassword,
        TestSMTPFromEmail,
        TestSMTPIsSecure
    }

    public enum KeysListWorkFlow
    {
        WorkFlowConnection,
        WorkFlowDomainURL,
        ApplicationKey,
        ApplicationURL
    }

    public enum KeysListOther
    {
        MISPOLetter,
        DOIMailSent,
        SGSTLetter,
        StartingDepartment,
        StartingDesignation,
        EndingDesignation
    }

    #endregion

    /// <summary>
    /// Enum for defining log file names based on the log level.
    /// </summary>
    public enum NLogErrorFileName
    {
        /// <summary>
        /// Log file for Trace level logs, used for detailed internal system debugging.
        /// </summary>
        TraceLog,

        /// <summary>
        /// Log file for Debug level logs, useful for debugging and development.
        /// </summary>
        DebugLog,

        /// <summary>
        /// Log file for Info level logs, used for regular application status and information.
        /// </summary>
        InfoLog,

        /// <summary>
        /// Log file for Warn level logs, used for non-critical issues that may need attention.
        /// </summary>
        WarnLog,

        /// <summary>
        /// Log file for Error level logs, indicating critical issues or failures.
        /// </summary>
        ErrorLog,

        /// <summary>
        /// Log file for Fatal level logs, indicating severe issues that cause application crashes.
        /// </summary>
        FatalLog,

        /// <summary>
        /// General log file for capturing NLog events.
        /// </summary>
        GeneralLog
    }

    public enum NLogType
    {
        /// <summary>
        /// Trace level log, typically used for detailed internal system debugging.
        /// </summary>
        Trace = 1,

        /// <summary>
        /// Debug level log, useful for debugging and development. Should be turned off in production.
        /// </summary>
        Debug,

        /// <summary>
        /// Informational messages. Used for regular operations or status updates.
        /// </summary>
        Info,

        /// <summary>
        /// Warning level logs for non-critical issues that may require attention.
        /// </summary>
        Warn,

        /// <summary>
        /// Error level logs for critical issues, typically indicating that something went wrong.
        /// </summary>
        Error,

        /// <summary>
        /// Fatal level logs, for critical errors that result in application crash or severe malfunction.
        /// </summary>
        Fatal,

        /// <summary>
        /// Critical level log for very high-severity errors requiring immediate intervention.
        /// </summary>
        Critical
    }
    
    public enum Browser
    {
        Unknown,
        Firefox,
        Chrome,
        Safari,
        InternetExplorer,
        Edge,
        Opera,
        Brave
    }

    public enum OperatingSystem
    {
        Unknown,
        Windows,
        MacOS,
        Linux,
        Android,
        iOS
    }

    public enum DeviceType
    {
        Unknown,
        Mobile,
        Tablet,
        Desktop,
        AndroidMobile,
        AndroidTablet,
        iPhone,
        iPad,
        iPodTouch,
        WindowsPhone,
        WindowsPC,
        Macintosh,
        LinuxDevice,
        Chromebook,
        PlayStation,
        XboxConsole,
        NintendoConsole,
        AppleTV
    }
}