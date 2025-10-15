using KBNovaCMS.Common.Enums;
using KBNovaCMS.Common.Security.EncryptDecrypt;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace KBNovaCMS.Common
{

    public static class MyServer
    {
        /// <summary>
        /// Maps the relative path to an absolute path based on the application's content root path.
        /// </summary>
        /// <param name="path">The relative path to be mapped.</param>
        /// <returns>The absolute path combined with the content root path.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the content root path is not set.</exception>
        public static string MapPath(string path)
        {
            // Validate input: Ensure the path is not null or just whitespace
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            // Attempt to get the content root path from AppDomain data
            string? contentRootPath = AppDomain.CurrentDomain.GetData("ContentRootPath") as string;

            // If ContentRootPath is not set, throw an exception
            if (string.IsNullOrWhiteSpace(contentRootPath))
            {
                throw new InvalidOperationException("ContentRootPath is not set.");
            }

            // Return the absolute path by combining the content root path with the provided path
            return Path.Combine(contentRootPath, path);
        }
    }

    public static class Utility
    {
        // Static fields for reuse across the application
        public static readonly StringBuilder StringBuilder = new StringBuilder();
        public static readonly Dictionary<string, object> Dictionary = new Dictionary<string, object>();
        public static readonly Regex Regex = new Regex(string.Empty); // Placeholder initialization for Regex
        public static readonly Random Random = new Random();
        public static readonly string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static readonly string SpecialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,"; // Special characters for various checks
        public static readonly string XSSChar = DescriptionAttr(ValidationDataType.XSSPrevent); // XSS Prevention Characters

        // This Below Is Used To Logged User Loggin Information !
        public static readonly Dictionary<string, (Browser, string)> BrowserCache = new();
        public static readonly Dictionary<string, (Enums.OperatingSystem, string)> OsCache = new();
        public static readonly Dictionary<string, (DeviceType, string)> DeviceCache = new();

        // Common date/time formats for consistent formatting across the app
        public static readonly string DateFormat = "dd/MM/yyyy";
        public static readonly string DateTimeFormat = "dd/MM/yyyy hh:mm:ss fffff";
        public static readonly string TimeFormat = "hh:mm tt";

        // SQL injection prevention list of potentially harmful SQL keywords
        public static readonly string[] StrArraySqlCheckList =
        {
        "--", ";--", ";", "/*", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar",
        "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end",
        "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects",
        "syscolumns", "table", "update"
        };

        // Dictionary for mapping file extensions to MIME types for better content-type handling
        public static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".txt", "text/plain" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".mp4", "video/mp4" },
            { ".avi", "video/x-msvideo" },
            { ".mkv", "video/x-matroska" }
        };

        /// <summary>
        /// Gets the description attribute of a given enumeration value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="source">The enum value.</param>
        /// <returns>The description if it exists, otherwise an empty string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided enum value is null.</exception>
        public static string DescriptionAttr<T>(this T source)
        {
            // Ensure that the source is not null
            if (source == null) throw new ArgumentNullException(nameof(source));

            var fieldName = source.ToString();

            // Return an empty string if the enum value has no associated description
            if (string.IsNullOrWhiteSpace(fieldName)) return string.Empty;

            // Get the field info for the enum value
            var fieldInfo = typeof(T).GetField(fieldName);

            // If the field info is not found, return an empty string
            if (fieldInfo == null) return string.Empty;

            // Retrieve the DescriptionAttribute if it exists
            var attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? string.Empty; // Return the description or empty string if none exists
        }

        /// <summary>
        /// Converts an object's public properties into a dictionary.
        /// </summary>
        /// <param name="obj">The object to be converted to a dictionary.</param>
        /// <returns>A dictionary containing the property names and their values.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided object is null.</exception>
        public static Dictionary<string, object> ObjectToDictionary(object obj)
        {
            // Ensure the object is not null
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var dictionary = new Dictionary<string, object>();

            // Iterate over the object's properties and add them to the dictionary
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propName = prop.Name;

                // Skip properties with empty or null names
                if (string.IsNullOrWhiteSpace(propName)) continue;

                var value = prop.GetValue(obj);

                // Add the property name and value to the dictionary if the value is not null
                if (value != null)
                {
                    dictionary[propName] = value;
                }
            }

            return dictionary; // Return the populated dictionary
        }

        #region Random String
        /// <summary>
        /// Generates a random string of specified length using alphanumeric characters.
        /// </summary>
        /// <param name="length">The length of the random string to generate.</param>
        /// <returns>A random string with the specified length, or an empty string if the length is zero or negative.</returns>
        public static string RandomString(int length)
        {
            // Return empty string if the requested length is less than or equal to zero
            if (length <= 0)
            {
                return string.Empty;
            }

            // Use StringBuilder for efficient string manipulation instead of using concatenation
            var stringBuilder = new System.Text.StringBuilder(length);

            // Generate the random string by appending random characters from the 'Chars' string
            for (int i = 0; i < length; i++)
            {
                char randomChar = Chars[Random.Next(Chars.Length)];
                stringBuilder.Append(randomChar); // Append the random character
            }

            return stringBuilder.ToString(); // Return the generated random string
        }

        /// <summary>
        /// Generates a random 6-digit number string, always returning 6 digits.
        /// </summary>
        /// <returns>A random 6-digit number string, padded with leading zeros if necessary.</returns>
        public static string GetRandomNumberString()
        {
            // Generate a random number between 0 and 999999, then format it to always return 6 digits
            return Random.Next(0, 1000000).ToString("D6");
        }

        /// <summary>
        /// Generates a unique transaction-style random string based on the current date and time.
        /// </summary>
        /// <returns>A string combining the current date-time and a random 6-digit number, formatted as 'ddMMyyyyHHmmssfff' followed by a 6-digit number.</returns>
        public static string GetRandomNumberTransString()
        {
            // Generate a string with current date-time in the format 'ddMMyyyyHHmmssfff' followed by a 6-digit random number
            return $"{DateTime.Now:ddMMyyyyHHmmssfff}{Random.Next(0, 1000000):D6}";
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validates all regex patterns defined in the ValidationDataType enum by checking each one for correctness.
        /// This method ensures that the regex patterns are valid and outputs a message indicating the result.
        /// </summary>
        public static void ValidateRegexPatterns()
        {
            // Iterate over all enum values of ValidationDataType
            foreach (var enumValue in Enum.GetValues(typeof(ValidationDataType)))
            {
                var enumMember = (ValidationDataType)enumValue;

                // Get the DescriptionAttribute for the enum member
                var descriptionAttribute = enumMember.GetType()
                    .GetField(enumMember.ToString())
                    .GetCustomAttribute<DescriptionAttribute>();

                // If a description is found, use it as the regex pattern
                if (descriptionAttribute != null)
                {
                    var regexPattern = descriptionAttribute.Description;

                    try
                    {
                        // Attempt to compile the regex to validate its pattern
                        Regex.IsMatch("", regexPattern);  // Compiling the regex with an empty string
                        Console.WriteLine($"Pattern for {enumMember}: Valid");
                    }
                    catch (ArgumentException)
                    {
                        // If the regex pattern is invalid, catch the exception and output an error message
                        Console.WriteLine($"Pattern for {enumMember}: Invalid");
                    }
                }
            }
        }

        /// <summary>
        /// Validates the input against a specified regex pattern based on the ValidationDataType enum.
        /// This method will check if the input matches the predefined pattern, and return true if it does.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <param name="validationType">The ValidationDataType enum indicating the type of validation to apply.</param>
        /// <returns>True if the input matches the regex pattern, false otherwise.</returns>
        public static bool ValidateWithRegex(string input, ValidationDataType validationType)
        {
            try
            {
                // Retrieve the regex pattern associated with the provided ValidationDataType
                var pattern = DescriptionAttr(validationType);

                // Validate if the input is not null or whitespace and if it matches the regex pattern
                return !string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                // Log or handle the exception, if any occurs during validation
                // Re-throwing the exception with a more descriptive message
                throw new InvalidOperationException($"Validation for {validationType} failed.", ex);
            }
        }

        #region Individual Validation Based On Regex

        /// <summary>
        /// Validates if the given email ID is in a correct format.
        /// </summary>
        /// <param name="emailId">The email ID to be validated.</param>
        /// <returns>True if the email ID is valid, otherwise false.</returns>
        public static bool ValidateEmailId(string emailId)
        {
            return ValidateWithRegex(emailId, ValidationDataType.EmailID);
        }

        /// <summary>
        /// Validates if the given age is above 18 years.
        /// </summary>
        /// <param name="age">The age to be validated.</param>
        /// <returns>True if the age is valid and above 18 years, otherwise false.</returns>
        public static bool ValidateAge_Above_18Years(string age)
        {
            return ValidateWithRegex(age, ValidationDataType.AgeAbove18Years);
        }

        /// <summary>
        /// Validates if the text contains only letters (no numbers or special characters).
        /// </summary>
        /// <param name="text">The text to be validated.</param>
        /// <returns>True if the text contains only letters, otherwise false.</returns>
        public static bool ValidateLettersOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.LettersOnly);
        }

        /// <summary>
        /// Validates if the text contains only letters and spaces.
        /// </summary>
        /// <param name="text">The text to be validated.</param>
        /// <returns>True if the text contains only letters and spaces, otherwise false.</returns>
        public static bool ValidateLettersWithWhiteSpace(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.LettersWithWhiteSpace);
        }

        /// <summary>
        /// Validates if the text contains only Gujarati letters.
        /// </summary>
        /// <param name="text">The Gujarati text to be validated.</param>
        /// <returns>True if the text contains only Gujarati letters, otherwise false.</returns>
        public static bool ValidateLettersOnlyGujarati(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.LettersOnlyGujarati);
        }

        /// <summary>
        /// Validates if the text contains only Gujarati letters and spaces.
        /// </summary>
        /// <param name="text">The Gujarati text to be validated.</param>
        /// <returns>True if the text contains only Gujarati letters and spaces, otherwise false.</returns>
        public static bool ValidateLettersWithWhiteSpaceGujarati(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.LettersWithWhiteSpaceGujarati);
        }

        /// <summary>
        /// Validates if the text contains only alphanumeric characters (letters and numbers).
        /// </summary>
        /// <param name="text">The alphanumeric text to be validated.</param>
        /// <returns>True if the text contains only alphanumeric characters, otherwise false.</returns>
        public static bool ValidateAlphanumericOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.AlphanumericOnly);
        }

        /// <summary>
        /// Validates if the text contains alphanumeric characters and spaces.
        /// </summary>
        /// <param name="text">The alphanumeric text with spaces to be validated.</param>
        /// <returns>True if the text contains alphanumeric characters and spaces, otherwise false.</returns>
        public static bool ValidateAlphanumericWithWhiteSpace(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.AlphanumericWithWhiteSpace);
        }

        /// <summary>
        /// Validates if the text contains only Gujarati alphanumeric characters (letters and numbers).
        /// </summary>
        /// <param name="text">The Gujarati alphanumeric text to be validated.</param>
        /// <returns>True if the text contains only Gujarati alphanumeric characters, otherwise false.</returns>
        public static bool ValidateAlphanumericOnlyGujarati(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.AlphanumericOnlyGujarati);
        }

        /// <summary>
        /// Validates if the text contains Gujarati alphanumeric characters and spaces.
        /// </summary>
        /// <param name="text">The Gujarati alphanumeric text with spaces to be validated.</param>
        /// <returns>True if the text contains Gujarati alphanumeric characters and spaces, otherwise false.</returns>
        public static bool ValidateAlphanumericWithWhiteSpaceGujarati(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.AlphanumericWithWhiteSpaceGujarati);
        }

        /// <summary>
        /// Validates if the text contains only numeric values (positive and negative integers).
        /// </summary>
        /// <param name="text">The numeric text to be validated.</param>
        /// <returns>True if the text contains only numeric values, otherwise false.</returns>
        public static bool ValidateNumberOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.NumberOnly);
        }
        public static bool ValidateNumberOnlyWithoutZero(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.NumberOnlyWithoutZero);
        }
        /// <summary>
        /// Validates if the text is a valid Aadhaar number.
        /// </summary>
        /// <param name="text">The Aadhaar number to be validated.</param>
        /// <returns>True if the Aadhaar number is valid, otherwise false.</returns>
        public static bool ValidateAadharCardNumber(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.AadharNumber);
        }

        /// <summary>
        /// Validates if the text contains only numeric values (positive or negative integers).
        /// </summary>
        /// <param name="text">The numeric text to be validated.</param>
        /// <returns>True if the text contains only numeric values (positive or negative), otherwise false.</returns>
        public static bool ValidateNumberOnlyPositiveAndNegative(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.NumberOnlyPositiveAndNegative);
        }

        /// <summary>
        /// Validates if the text contains only decimal values (positive and negative).
        /// </summary>
        /// <param name="text">The decimal text to be validated.</param>
        /// <returns>True if the text contains only decimal values, otherwise false.</returns>
        public static bool ValidateDecimalOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.DecimalOnly);
        }

        /// <summary>
        /// Validates if the text contains only decimal values (positive and negative).
        /// </summary>
        /// <param name="text">The decimal text to be validated.</param>
        /// <returns>True if the text contains only decimal values (positive and negative), otherwise false.</returns>
        public static bool ValidateDecimalOnlyPositiveAndNegative(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.DecimalOnlyPositiveAndNegative);
        }

        /// <summary>
        /// Validates if the text is a valid mobile number.
        /// </summary>
        /// <param name="text">The mobile number to be validated.</param>
        /// <returns>True if the mobile number is valid, otherwise false.</returns>
        public static bool ValidateMobileNo(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.MobileNo);
        }

        /// <summary>
        /// Validates if the text is a valid mobile number with a specific series (e.g., country code, etc.).
        /// </summary>
        /// <param name="text">The mobile number to be validated.</param>
        /// <returns>True if the mobile number with series is valid, otherwise false.</returns>
        public static bool ValidateMobileNoWithSeries(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.MobileNoWithSeries);
        }

        /// <summary>
        /// Validates if the text is a valid fax or phone number.
        /// </summary>
        /// <param name="text">The fax or phone number to be validated.</param>
        /// <returns>True if the fax or phone number is valid, otherwise false.</returns>
        public static bool ValidateFaxOrPhoneNo(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.FaxOrPhoneNo);
        }

        /// <summary>
        /// Validates if the text is a valid GST number.
        /// </summary>
        /// <param name="text">The GST number to be validated.</param>
        /// <returns>True if the GST number is valid, otherwise false.</returns>
        public static bool ValidateGSTNumber(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.GSTNumber);
        }

        /// <summary>
        /// Validates if the text is a valid PAN number.
        /// </summary>
        /// <param name="text">The PAN number to be validated.</param>
        /// <returns>True if the PAN number is valid, otherwise false.</returns>
        public static bool ValidatePANNumber(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.PANNumber);
        }

        /// <summary>
        /// Validates if the text is a valid IFSC code.
        /// </summary>
        /// <param name="text">The IFSC code to be validated.</param>
        /// <returns>True if the IFSC code is valid, otherwise false.</returns>
        public static bool ValidateIFSCCode(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.IFSCCode);
        }
        public static bool ValidateHouseOrSurveyNumber(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.HouseOrSurveyNumber);
        }
        /// <summary>
        /// Validates if the text is a valid account number.
        /// </summary>
        /// <param name="text">The account number to be validated.</param>
        /// <returns>True if the account number is valid, otherwise false.</returns>
        public static bool ValidateAccountNumber(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.AccountNumber);
        }

        /// <summary>
        /// Validates if the text is a valid date in the 'dd/MM/yyyy' format.
        /// </summary>
        /// <param name="text">The date text to be validated.</param>
        /// <returns>True if the date is valid, otherwise false.</returns>
        public static bool ValidateDateOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.DateOnly);
        }

        /// <summary>
        /// Validates if the text represents valid months (e.g., "01" to "12").
        /// </summary>
        /// <param name="text">The month text to be validated.</param>
        /// <returns>True if the month text is valid, otherwise false.</returns>
        public static bool ValidateMonthsOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.MonthOnly);
        }

        /// <summary>
        /// Validates if the text represents valid years (e.g., "2022", "2023").
        ///
        /// </summary>
        /// <param name="text">The year text to be validated.</param>
        /// <returns>True if the year text is valid, otherwise false.</returns>
        public static bool ValidateYearsOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.YearOnly);
        }

        /// <summary>
        /// Validates if the text represents a valid month and year (e.g., "MM/yyyy").
        /// </summary>
        /// <param name="text">The month and year text to be validated.</param>
        /// <returns>True if the month and year text is valid, otherwise false.</returns>
        public static bool ValidateMonthYearsOnly(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.MonthYearOnly);
        }

        /// <summary>
        /// Validates if the text represents a valid age (numeric value between 0-150).
        /// </summary>
        /// <param name="text">The age text to be validated.</param>
        /// <returns>True if the age text is valid, otherwise false.</returns>
        public static bool ValidateAge(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.Age);
        }

        /// <summary>
        /// Prevents Cross-Site Scripting (XSS) attacks by validating input text against malicious patterns.
        /// </summary>
        /// <param name="text">The text to be validated.</param>
        /// <returns>True if the text is safe from XSS, otherwise false.</returns>
        public static bool ValidateXSSPrevent(string text)
        {
            return ValidateWithRegex(text, ValidationDataType.XSSPrevent);
        }
        #endregion

        /// <summary>
        /// Checks if the input string contains any special characters from a predefined collection.
        /// </summary>
        /// <param name="input">The input string to check.</param>
        /// <returns>True if the input contains special characters, otherwise false.</returns>
        public static bool HasSpecialChar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;  // Return false if the input is null or empty
            }

            // Check if any character from SpecialChar collection exists in the input string
            return SpecialChar.Any(specialChar => input.Contains(specialChar));
        }

        /// <summary>
        /// Validates if a given value is defined in an enum type.
        /// </summary>
        /// <param name="enumType">The enum type to check.</param>
        /// <param name="value">The value to check against the enum.</param>
        /// <returns>The name of the enum value if valid, otherwise an empty string.</returns>
        public static string EnumHasValue(Type enumType, object value)
        {
            // Validate that the enum type is not null and is actually an enum
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType), "Enum type cannot be null.");
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Provided type is not an enum.", nameof(enumType));
            }

            // Validate that the value is not null
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }

            // Check if the value is defined in the enum
            return Enum.IsDefined(enumType, value) ? Enum.GetName(enumType, value) : string.Empty;
        }

        /// <summary>
        /// Validates if a given string contains any potential XSS (Cross-site Scripting) characters.
        /// </summary>
        /// <param name="strData">The data string to validate for XSS characters.</param>
        /// <param name="strFieldName">The name of the field to include in the error message if XSS is found.</param>
        /// <returns>A JsonResponseModel indicating whether there was an error and an appropriate message.</returns>
        public static JsonResponseModel ValidateFieldXSS(string strData, string strFieldName)
        {
            var jsonResponseModel = new JsonResponseModel();

            try
            {
                // Validate input parameters for null or empty strings
                if (string.IsNullOrWhiteSpace(strData) || string.IsNullOrWhiteSpace(strFieldName))
                {
                    throw new ArgumentException("Both data and field name must be non-empty.");
                }

                // Regex pattern to match potential XSS characters
                var regex = new Regex(XSSChar, RegexOptions.IgnoreCase);

                // Check for potential XSS characters in the input data
                if (regex.IsMatch(strData))
                {
                    jsonResponseModel.IsError = true;
                    jsonResponseModel.StrMessage = $"Please enter a valid {strFieldName} value.";
                    jsonResponseModel.Type = PopupMessageType.Error.ToString();
                }
                else
                {
                    jsonResponseModel.IsError = false;
                    jsonResponseModel.StrMessage = string.Empty;  // No error
                    jsonResponseModel.Type = PopupMessageType.Success.ToString();
                }
            }
            catch (ArgumentException ex)
            {
                // Catch specific argument exceptions and set appropriate error message
                jsonResponseModel.IsError = true;
                jsonResponseModel.StrMessage = ex.Message;
                jsonResponseModel.Type = PopupMessageType.Error.ToString();
            }
            catch (Exception)
            {
                // General exception handling for unexpected errors
                jsonResponseModel.IsError = true;
                jsonResponseModel.StrMessage = "An unexpected error occurred. Please try again.";
                jsonResponseModel.Type = PopupMessageType.Error.ToString();
            }

            return jsonResponseModel;
        }

        public static bool ValidateMyModel(string propertyName, object propertyValue, int validationType, out JsonResponseModel jsonResponseModel)
        {
            jsonResponseModel = new JsonResponseModel();
            bool allow = false;

            // Convert PropertyValue to string once using invariant culture to ensure consistent formatting
            string? propertyValueString = Convert.ToString(propertyValue, CultureInfo.InvariantCulture);

            // Ensure both propertyName and propertyValueString are non-null and non-whitespace
            if (!string.IsNullOrWhiteSpace(propertyName) && !string.IsNullOrEmpty(propertyValueString))
            {
                // Validate based on the provided validationType using a switch statement
                switch (validationType)
                {
                    // Case for validating email addresses
                    case (int)ValidationControlInputType.EmailId:
                        allow = ValidateEmailId(propertyValueString);
                        break;

                    #region Letters Regex Validation
                    case (int)ValidationControlInputType.LettersOnly:
                        allow = ValidateLettersOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.LettersWithWhiteSpace:
                        allow = ValidateLettersWithWhiteSpace(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.LettersOnlyGujarati:
                        allow = ValidateLettersOnlyGujarati(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.LettersWithWhiteSpaceGujarati:
                        allow = ValidateLettersWithWhiteSpaceGujarati(propertyValueString);
                        break;
                    #endregion

                    #region Alpha Numeric Regex Validation
                    case (int)ValidationControlInputType.AlphanumericOnly:
                        allow = ValidateAlphanumericOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.AlphanumericWithWhiteSpace:
                        allow = ValidateAlphanumericWithWhiteSpace(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.AlphanumericOnlyGujarati:
                        allow = ValidateAlphanumericOnlyGujarati(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.AlphanumericWithWhiteSpaceGujarati:
                        allow = ValidateAlphanumericWithWhiteSpaceGujarati(propertyValueString);
                        break;
                    #endregion

                    #region Numbers Validation
                    case (int)ValidationControlInputType.NumberOnly:
                        allow = int.TryParse(propertyValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                                ValidateNumberOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.NumberOnlyWithoutZero:
                        allow = int.TryParse(propertyValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                                ValidateNumberOnlyWithoutZero(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.AadharCardNumber:
                        allow = ValidateAadharCardNumber(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.NumberOnlyPositiveAndNegative:
                        allow = int.TryParse(propertyValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                                ValidateNumberOnlyPositiveAndNegative(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.DecimalOnly:
                        allow = double.TryParse(propertyValueString, NumberStyles.Float, CultureInfo.InvariantCulture, out _) &&
                                ValidateDecimalOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.DecimalOnlyPositiveAndNegative:
                        allow = double.TryParse(propertyValueString, NumberStyles.Float, CultureInfo.InvariantCulture, out _) &&
                                ValidateDecimalOnlyPositiveAndNegative(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.MobileNo:
                        allow = long.TryParse(propertyValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                                ValidateMobileNo(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.MobileNoWithSeries:
                        allow = long.TryParse(propertyValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                                ValidateMobileNoWithSeries(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.FaxOrPhoneNo:
                        allow = long.TryParse(propertyValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
                                ValidateFaxOrPhoneNo(propertyValueString);
                        break;
                    #endregion

                    // Other types of validations (GST, PAN, Account number, etc.)
                    case (int)ValidationControlInputType.GSTNumber:
                        allow = ValidateGSTNumber(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.PANNumber:
                        allow = ValidatePANNumber(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.AccountNumber:
                        allow = ValidateAccountNumber(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.IFSCCode:
                        allow = ValidateIFSCCode(propertyValueString);
                        break;

                    case (int)ValidationControlInputType.HouseOrSurveyNumber:
                        allow = ValidateHouseOrSurveyNumber(propertyValueString);
                        break;
                    #region Calendar Validation
                    case (int)ValidationControlInputType.DateOnly:
                        allow = ValidateDateOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.MonthsOnly:
                        allow = ValidateMonthsOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.YearsOnly:
                        allow = ValidateYearsOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.MonthYearOnly:
                        allow = ValidateMonthYearsOnly(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.Age:
                        allow = ValidateAge(propertyValueString);
                        break;
                    case (int)ValidationControlInputType.AgeAbove18Years:
                        allow = ValidateAge_Above_18Years(propertyValueString);
                        break;
                    #endregion

                    // Security validation to prevent XSS attacks
                    case (int)ValidationControlInputType.XSSPrevent:
                        allow = !ValidateXSSPrevent(propertyValueString);
                        break;

                    // Handle undefined or unsupported validation types
                    default:
                        jsonResponseModel.IsError = true;
                        jsonResponseModel.Type = PopupMessageType.Error.ToString();
                        jsonResponseModel.StrMessage = "Invalid validation type";
                        return false;
                }
            }

            // Prepare the response based on validation result
            if (allow)
            {
                jsonResponseModel.IsError = false;
                jsonResponseModel.Type = PopupMessageType.Success.ToString();
                jsonResponseModel.StrMessage = string.Empty;
            }
            else
            {
                jsonResponseModel.IsError = true;
                jsonResponseModel.Type = PopupMessageType.Error.ToString();
                jsonResponseModel.StrMessage = $"Enter a valid value for {propertyName}";
            }

            return allow;
        }

        public static bool ValidateWithRegex(string input, ValidationDataType validationType, out JsonResponseModel jsonResponseModel)
        {
            jsonResponseModel = new JsonResponseModel();

            // Check if the input is null or empty upfront to avoid unnecessary processing
            if (string.IsNullOrWhiteSpace(input))
            {
                jsonResponseModel.IsError = true;
                jsonResponseModel.Title = PopupMessageType.Error.ToString();
                jsonResponseModel.Type = PopupMessageType.Error.ToString();
                jsonResponseModel.StrMessage = "Input cannot be empty or null.";
                return false;
            }

            try
            {
                // Perform validation based on the specified validation type
                bool isValid = ValidateWithRegex(input, validationType);

                // If validation fails, return false and set an appropriate error message
                if (!isValid)
                {
                    jsonResponseModel.IsError = true;
                    jsonResponseModel.Title = PopupMessageType.Error.ToString();
                    jsonResponseModel.Type = PopupMessageType.Error.ToString();
                    jsonResponseModel.StrMessage = $"Invalid input for validation type: {validationType}.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions and log them in the response model
                jsonResponseModel.IsError = true;
                jsonResponseModel.Title = PopupMessageType.Error.ToString();
                jsonResponseModel.Type = PopupMessageType.Error.ToString();
                jsonResponseModel.StrMessage = $"Validation error: {ex.Message}";
                return false;
            }

            // If validation passes, return true and set success message
            jsonResponseModel.IsError = false;
            jsonResponseModel.Title = PopupMessageType.Success.ToString();
            jsonResponseModel.Type = PopupMessageType.Success.ToString();
            jsonResponseModel.StrMessage = string.Empty;

            return true;
        }

        /// <summary>
        /// Validates the properties of the provided model based on a set of validation rules.
        /// </summary>
        /// <typeparam name="T">The type of the model being validated.</typeparam>
        /// <param name="model">The model object whose properties are to be validated.</param>
        /// <param name="validationRules">An array of validation rules to apply to the model fields.</param>
        /// <param name="jsonResponse">A response object to store validation error messages.</param>
        /// <returns>Returns true if all validation checks pass, otherwise false.</returns>
        public static bool ValidateModel<T>(T model,
            (string FieldName, object Value, ValidationDataType? ValidationType, bool IsMandatory)[] validationRules,
            ref JsonResponseModel jsonResponse)
        {
            try
            {
                // Iterate through all validation rules to validate each field in the model.
                foreach (var (fieldName, value, validationType, isMandatory) in validationRules)
                {
                    // Retrieve the value of the field from the model using reflection.
                    //var fieldValue = model.GetType().GetProperty(fieldName)?.GetValue(model);

                    // Directly use the 'value' provided in the validation rule
                    var fieldValue = value;

                    // Only proceed if the field is mandatory or has a non-null, non-empty value
                    if (isMandatory || !string.IsNullOrEmpty(fieldValue?.ToString()))
                    {
                        // If a validation type (regex) is specified, apply the corresponding validation rule.
                        if (validationType.HasValue)
                        {
                            // Validate the field value against the specified validation type (regex).
                            if (!ValidateWithRegex(fieldValue?.ToString(), validationType.Value, out jsonResponse))
                            {
                                // If validation fails, set the error message and return false.
                                jsonResponse.IsError = true;
                                jsonResponse.Title = PopupMessageType.Error.ToString();
                                jsonResponse.Type = PopupMessageType.Error.ToString();
                                jsonResponse.StrMessage = $"Please enter a valid value for '{fieldName}'!";
                                return false;
                            }
                        }
                        // If no validation type is specified and the field is mandatory, check if it's empty.
                        else if (isMandatory)
                        {
                            if (string.IsNullOrWhiteSpace(fieldValue?.ToString()))
                            {
                                // Set a specific error message when a mandatory field is empty.
                                jsonResponse.IsError = true;
                                jsonResponse.Title = PopupMessageType.Error.ToString();
                                jsonResponse.Type = PopupMessageType.Error.ToString();
                                jsonResponse.StrMessage = $"The field '{fieldName}' is required and cannot be empty!";
                                return false;
                            }
                        }
                    }
                }

                // If all validation checks pass successfully, return true.
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion

        #region HTML Utilities

        public static string GetMyTable<T>(IEnumerable<T> list, params Expression<Func<T, object>>[] fxns)
        {
            if (list == null || fxns == null || !fxns.Any())
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            try
            {
                stringBuilder.Append("<table>\n");

                // Add header row
                stringBuilder.Append("<tr>\n");
                foreach (var fxn in fxns)
                {
                    stringBuilder.Append("<td>");
                    stringBuilder.Append(GetName(fxn));
                    stringBuilder.Append("</td>");
                }
                stringBuilder.Append("</tr>\n");

                // Add data rows
                foreach (var item in list)
                {
                    stringBuilder.Append("<tr>\n");
                    foreach (var fxn in fxns)
                    {
                        stringBuilder.Append("<td>");
                        var value = fxn.Compile()(item);
                        stringBuilder.Append(value?.ToString() ?? string.Empty);
                        stringBuilder.Append("</td>");
                    }
                    stringBuilder.Append("</tr>\n");
                }
                stringBuilder.Append("</table>");

                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                // Log or handle the exception if needed
                throw new InvalidOperationException("An error occurred while generating the table.", ex);
            }
        }

        public static string GetName<T>(Expression<Func<T, object>> expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException(nameof(expr));
            }

            var member = expr.Body as MemberExpression ?? (expr.Body as UnaryExpression)?.Operand as MemberExpression;
            return member != null ? GetName2(member) : string.Empty;
        }

        private static string GetName2(MemberExpression member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            var attribute = member.Member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? fieldInfo.Name,
                PropertyInfo propertyInfo => propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? propertyInfo.Name,
                _ => string.Empty
            };

            return attribute;
        }

        public static string ConvertDataTableToHtml(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            try
            {
                stringBuilder.Append("<table class=\"table\" id=\"dvReportTable\">\n<thead>\n<tr>\n");

                // Add header row
                foreach (DataColumn column in dataTable.Columns)
                {
                    stringBuilder.Append($"<th>{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(column.ColumnName)}</th>\n");
                }

                stringBuilder.Append("</tr>\n</thead>\n<tbody>\n");

                // Add rows
                foreach (DataRow row in dataTable.Rows)
                {
                    stringBuilder.Append("<tr>\n");
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        stringBuilder.Append($"<td>{row[column]?.ToString() ?? string.Empty}</td>\n");
                    }
                    stringBuilder.Append("</tr>\n");
                }

                stringBuilder.Append("</tbody></table>\n");

                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                // Log or handle the exception if needed
                throw new InvalidOperationException("An error occurred while converting the DataTable to HTML.", ex);
            }
        }

        public static string ToHtmlTable<T>(this List<T> listOfClassObjects)
        {
            if (listOfClassObjects == null || !listOfClassObjects.Any())
            {
                return string.Empty;
            }

            try
            {
                var stringBuilder = new StringBuilder();
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (properties.Length == 0)
                {
                    return string.Empty;
                }

                // Build the HTML table header
                stringBuilder.Append("<table style='overflow: scroll; display: block;'>\n<thead>\n");
                stringBuilder.Append(properties.Select(p => p.Name).ToList().ToColumnHeaders());
                stringBuilder.Append("\n</thead>\n<tbody>\n");

                // Build the HTML table rows
                foreach (var item in listOfClassObjects)
                {
                    stringBuilder.Append(item.ToHtmlTableRow(properties));
                }

                stringBuilder.Append("\n</tbody>\n</table>\n");

                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                // Log or handle the exception if needed
                throw new InvalidOperationException("An error occurred while generating the HTML table.", ex);
            }
        }

        public static string ToColumnHeaders(this List<string> headers)
        {
            var sb = new StringBuilder();
            sb.Append("<tr>\n");
            foreach (var header in headers)
            {
                sb.Append($"<th>{header}</th>\n");
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string ToHtmlTableRow<T>(this T item, PropertyInfo[] properties)
        {
            var sb = new StringBuilder();
            sb.Append("<tr>\n");
            foreach (var property in properties)
            {
                var value = property.GetValue(item, null);
                var cellValue = value != null ? value.ToString() : string.Empty;
                sb.Append($"<td>{cellValue}</td>\n");
            }
            sb.Append("</tr>");
            return sb.ToString();
        }


        // Extracts the browser name from the User-Agent string using regex
        public static Browser GetBrowserName(string userAgent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userAgent)) return Browser.Unknown;

                if (BrowserCache.TryGetValue(userAgent, out var cachedResult))
                {
                    return cachedResult.Item1;
                }

                Browser browser = userAgent switch
                {
                    var ua when ua.Contains("Firefox") => Browser.Firefox,
                    var ua when ua.Contains("Chrome") && !ua.Contains("Chromium") => Browser.Chrome,
                    var ua when ua.Contains("Safari") && !ua.Contains("Chrome") => Browser.Safari,
                    var ua when ua.Contains("MSIE") || ua.Contains("Trident") => Browser.InternetExplorer,
                    var ua when ua.Contains("Edge") => Browser.Edge,
                    var ua when ua.Contains("OPR") => Browser.Opera,
                    var ua when ua.Contains("Brave") => Browser.Brave,
                    _ => Browser.Unknown
                };

                // Cache the result
                BrowserCache[userAgent] = (browser, userAgent);
                return browser;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Extracts the browser version from the User-Agent string using regex patterns
        public static string GetBrowserVersion(string userAgent)
        {
            try
            {
                if (string.IsNullOrEmpty(userAgent)) return "Unknown Version";

                var patterns = new Dictionary<Browser, string>
            {
                { Browser.Chrome, @"Chrome/(\d+\.\d+\.\d+\.\d+)" },
                { Browser.Firefox, @"Firefox/(\d+\.\d+)" },
                { Browser.Safari, @"Version/(\d+\.\d+).*Safari" },
                { Browser.Edge, @"Edge/(\d+\.\d+)" },
                { Browser.InternetExplorer, @"MSIE (\d+\.\d+)" },
                { Browser.Opera, @"OPR/(\d+\.\d+)" },
                { Browser.Brave, @"Brave/(\d+\.\d+)" }
            };

                foreach (var browser in patterns)
                {
                    var match = Regex.Match(userAgent, browser.Value);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }

                return "Unknown Version";  // Return "Unknown Version" if no match is found
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetOperatingSystemName(string userAgent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

                if (userAgent.Contains("Windows NT")) return "Windows";
                if (userAgent.Contains("Macintosh")) return "Mac OS";
                if (userAgent.Contains("Linux")) return "Linux";
                if (userAgent.Contains("Android")) return "Android";
                if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";

                return "Unknown";
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Extracts the operating system version from the User-Agent string
        public static string GetOperatingSystemVersion(string userAgent)
        {
            try
            {
                if (string.IsNullOrEmpty(userAgent))
                    return "Unknown OS";

                // Patterns for different operating systems
                var windowsPattern = @"Windows NT (\d+\.\d+)";
                var macPattern = @"Mac OS X (\d+_\d+)";
                var linuxPattern = @"Linux";
                var androidPattern = @"Android (\d+\.\d+)";
                var iosPattern = @"iPhone OS (\d+_\d+)";

                // Try to match the operating system in the User-Agent string
                var match = Regex.Match(userAgent, windowsPattern);
                if (match.Success)
                {
                    var version = match.Groups[1].Value;
                    if (version == "10.0")
                    {
                        // Special handling for Windows 11, which uses "Windows NT 10.0" in the User-Agent
                        if (userAgent.Contains("Windows NT 10.0; Win64; x64") && userAgent.Contains("Trident/"))
                        {
                            return "Windows 11";  // Correctly identify Windows 11
                        }
                        return "Windows 10";
                    }
                }

                match = Regex.Match(userAgent, macPattern);
                if (match.Success)
                    return "Mac OS X " + match.Groups[1].Value.Replace('_', '.');

                match = Regex.Match(userAgent, linuxPattern);
                if (match.Success)
                    return "Linux";

                match = Regex.Match(userAgent, androidPattern);
                if (match.Success)
                    return "Android " + match.Groups[1].Value;

                match = Regex.Match(userAgent, iosPattern);
                if (match.Success)
                    return "iOS " + match.Groups[1].Value.Replace('_', '.');

                return "Unknown OS";
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Simplified method to get basic device type (Mobile, Tablet, Desktop)
        public static DeviceType GetDeviceType(string userAgent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userAgent)) return DeviceType.Unknown;

                if (DeviceCache.TryGetValue(userAgent, out var cachedResult))
                {
                    return cachedResult.Item1;
                }

                DeviceType deviceType = userAgent switch
                {
                    var ua when ua.Contains("Mobile") || ua.Contains("Android") || ua.Contains("iPhone") => DeviceType.Mobile,
                    var ua when ua.Contains("Tablet") => DeviceType.Tablet,
                    _ => DeviceType.Desktop
                };

                // Cache the result
                DeviceCache[userAgent] = (deviceType, userAgent);
                return deviceType;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Detailed method to get specific device type info (e.g., Android Mobile, iPad, etc.)
        public static string GetDeviceTypeDetails(string userAgent)
        {
            try
            {
                if (string.IsNullOrEmpty(userAgent)) return "Unknown Device Type";

                if (DeviceCache.TryGetValue(userAgent, out var cachedResult))
                {
                    return cachedResult.Item2;
                }

                string deviceDetails = userAgent switch
                {
                    var ua when ua.Contains("Android") => ua.Contains("Mobile") ? "Android Mobile" : "Android Tablet",
                    var ua when ua.Contains("iPhone") => "iPhone",
                    var ua when ua.Contains("iPad") => "iPad",
                    var ua when ua.Contains("iPod") => "iPod Touch",
                    var ua when ua.Contains("Windows NT") => ua.Contains("Windows Phone") ? "Windows Phone" : "Windows PC",
                    var ua when ua.Contains("Mac OS") => ua.Contains("Macintosh") ? "Macintosh" : "MacOS Device",
                    var ua when ua.Contains("Linux") => ua.Contains("Chrome") ? "Chromebook" : "Linux Device",
                    var ua when ua.Contains("PlayStation") => "PlayStation",
                    var ua when ua.Contains("Xbox") => "Xbox Console",
                    var ua when ua.Contains("Nintendo") => "Nintendo Console",
                    var ua when ua.Contains("AppleTV") => "Apple TV",
                    _ => "Unknown Device Type"
                };

                // Cache the result with both device type and details
                DeviceCache[userAgent] = (DeviceType.Unknown, deviceDetails);
                return deviceDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Method to sanitize input by removing special characters
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Define a regex pattern to match any non-alphanumeric character
            string pattern = @"[^a-zA-Z0-9\s\-]";

            // Replace all characters not matching the pattern with an empty string
            return Regex.Replace(input, pattern, string.Empty);
        }


        #endregion

        public static JsonResponseModel CreateJsonResponseModel(bool IsError, string StrMessage = "", string Title = "", string Type = "", object? Result = null)
        {
            return new JsonResponseModel
            {
                IsError = IsError,
                Title = Title,
                Type = Type,
                StrMessage = StrMessage,
                Result = Result
            };
        }

        // Helper method to create a consistent error response
        public static JsonResponseModel CreateErrorResponse(string message = "")
        {
            return new JsonResponseModel
            {
                IsError = true,
                Title = PopupMessageType.Error.ToString(),
                Type = PopupMessageType.Error.ToString(),
                StrMessage = message,
                Result = null
            };
        }

        // Helper method to create a consistent success response
        public static JsonResponseModel CreateSuccessResponse(string message = "", object Result = null)
        {
            return new JsonResponseModel
            {
                IsError = false,
                Title = PopupMessageType.Success.ToString(),
                Type = PopupMessageType.Success.ToString(),
                StrMessage = message,
                Result = Result // Add actual result if needed
            };
        }

        // Helper method to create a consistent success response
        public static JsonResponseModel CreateWarningResponse(string message = "")
        {
            return new JsonResponseModel
            {
                IsError = false,
                Title = PopupMessageType.Warning.ToString(),
                Type = PopupMessageType.Warning.ToString(),
                StrMessage = message,
                Result = null // Add actual result if needed
            };
        }

        // Helper method to create a consistent success response
        public static JsonResponseModel CreateInfoResponse(string message = "")
        {
            return new JsonResponseModel
            {
                IsError = false,
                Title = PopupMessageType.Info.ToString(),
                Type = PopupMessageType.Info.ToString(),
                StrMessage = message,
                Result = null // Add actual result if needed
            };
        }

        // Helper method to create a consistent success response
        public static JsonResponseModel CreateQuestionResponse(string message = "")
        {
            return new JsonResponseModel
            {
                IsError = false,
                Title = PopupMessageType.Question.ToString(),
                Type = PopupMessageType.Question.ToString(),
                StrMessage = message,
                Result = null // Add actual result if needed
            };
        }

        public static string GetMimeTypeFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            // Extract the file extension
            string extension = Path.GetExtension(fileName)?.ToLower();

            // If no extension is found, return "application/octet-stream" (generic binary data)
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream";
            }

            // Check if the MIME type exists for the given extension
            if (MimeTypes.ContainsKey(extension))
            {
                return MimeTypes[extension];
            }

            // Return a default MIME type if no match is found
            return "application/octet-stream"; // Generic MIME type for unknown files
        }
    }
    // Utility class for JSON validation
    public class JsonUtility
    {
        public static bool ValidateIncomingJsonString<T>(string jsonString, ref JsonResponseModel jsonResponseModel, out T resultObject)
        {
            resultObject = default(T);

            if (string.IsNullOrEmpty(jsonString))
            {
                jsonResponseModel = Utility.CreateErrorResponse("Unable to process your request. The provided Data is empty.");
                return false;
            }

            try
            {
                // Decrypt the JSON string (if necessary)
                jsonString = EncryptDecrypt.FrontDecryptDecode(jsonString); // Decrypt the JSON string before processing

                // Deserialize the JSON into the target object of type T
                resultObject = JsonConvert.DeserializeObject<T>(jsonString);

                // Automatically map flat properties to nested objects
                MapFlatPropertiesToNested(resultObject, jsonString);

            }
            catch (JsonException jsonEx)
            {
                // jsonResponseModel = Utility.CreateErrorResponse($"JSON processing error: {jsonEx.Message}");
                jsonResponseModel = Utility.CreateErrorResponse($"We are unable to process your request at the moment due to missing or incomplete data. Please try again later. We apologize for any inconvenience caused.");
                Console.WriteLine(jsonEx.ToString());
                return false;
            }
            catch (Exception ex)
            {
                // jsonResponseModel = Utility.CreateErrorResponse($"An unexpected error occurred while processing the request: {ex.Message}");
                jsonResponseModel = Utility.CreateErrorResponse($"We are unable to process your request at the moment due to missing or incomplete data. Please try again later. We apologize for any inconvenience caused.");
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        private static void MapFlatPropertiesToNested<T>(T resultObject, string jsonString)
        {
            var objectType = typeof(T);
            var properties = objectType.GetProperties();

            // Get the flat properties from the JSON string (for example: MCommonEntitiesMaster_IsActive)
            var flatProperties = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            foreach (var property in properties)
            {
                // Check if the property is a nested object
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var nestedPropertyName = property.Name;
                    var nestedProperty = property.GetValue(resultObject);

                    // Ensure the nested object is initialized
                    if (nestedProperty == null)
                    {
                        nestedProperty = Activator.CreateInstance(property.PropertyType);
                        property.SetValue(resultObject, nestedProperty);
                    }

                    // Now recursively check the flat property names and map them
                    foreach (var flatProperty in flatProperties)
                    {
                        if (flatProperty.Key.Contains(nestedPropertyName))
                        {
                            // Strip out the nested part and check if it matches
                            var parts = flatProperty.Key.Split('_');

                            if (parts.Length > 1 && parts[0] == nestedPropertyName)
                            {
                                var childPropertyName = parts.Last();
                                SetNestedProperty(nestedProperty, childPropertyName, flatProperty.Value);
                            }
                        }
                    }
                }
            }
        }

        private static void SetNestedProperty(object parentObject, string childPropertyName, object value)
        {
            var childProperty = parentObject.GetType().GetProperty(childPropertyName);

            if (childProperty != null && childProperty.CanWrite)
            {
                // Ensure proper type conversion
                if (value != null && childProperty.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    childProperty.SetValue(parentObject, Convert.ChangeType(value, childProperty.PropertyType));
                }
            }
        }




        // Common function to process the incoming JSON data
        public static string ProcessIncomingJson(string jsonString)
        {
            try
            {
                // Check if the incoming JSON data is null or empty
                if (string.IsNullOrEmpty(jsonString))
                {
                    // Return error response if the input is invalid
                    var errorResponse = Utility.CreateErrorResponse("Unable to process your request, try again!");
                    return JsonConvert.SerializeObject(errorResponse); // Return an error response
                }

                // Decrypt the incoming JSON data (if necessary)
                jsonString = EncryptDecrypt.FrontDecryptDecode(jsonString);

                // Parse the decrypted JSON string into a JObject
                JObject jObject = JObject.Parse(jsonString);

                // Restructure properties by replacing underscores with dots and handling nested objects
                RestructureProperties(jObject);

                // Return the modified JSON as a string
                return jObject.ToString();
            }
            catch (Exception ex)
            {
                // Handle any unexpected exceptions and return an error message
                var errorResponse = Utility.CreateErrorResponse($"An error occurred while processing the request: {ex.Message}");
                return JsonConvert.SerializeObject(errorResponse);
            }
        }

        // Method to restructure the properties of the JObject (handling underscores and grouping)
        public static void RestructureProperties(JObject jObject)
        {
            // List to store properties that need restructuring
            List<JProperty> propertiesToRemove = new List<JProperty>();

            try
            {
                // Iterate over each property in the JObject
                foreach (var property in jObject.Properties())
                {
                    // Check if the property name contains an underscore
                    if (property.Name.Contains('_'))
                    {
                        // Split the property name into parts using the underscore
                        string[] parts = property.Name.Split('_');

                        // If we have more than one part, we need to handle it as a nested structure
                        if (parts.Length >= 2)
                        {
                            // Check for a parent object (e.g., MCommonEntitiesMaster)
                            string parentName = parts[0];

                            // Ensure the parent property exists; create it if not
                            if (!jObject.ContainsKey(parentName))
                            {
                                jObject[parentName] = new JObject();
                            }

                            // Now we handle nested properties
                            JObject parentObject = (JObject)jObject[parentName];

                            // Iterate over the remaining parts and create nested properties accordingly
                            JObject currentObject = parentObject;
                            for (int i = 1; i < parts.Length; i++)
                            {
                                string part = parts[i];

                                // If this part is the last part, assign the value to the nested object
                                if (i == parts.Length - 1)
                                {
                                    currentObject[part] = property.Value;
                                }
                                else
                                {
                                    // If it's not the last part, create a nested object if it doesn't exist
                                    if (currentObject[part] == null)
                                    {
                                        currentObject[part] = new JObject();
                                    }
                                    // Move to the next level of nesting
                                    currentObject = (JObject)currentObject[part];
                                }
                            }

                            // Mark the original property for removal after restructuring
                            propertiesToRemove.Add(property);
                        }
                    }
                    else if (property.Value.Type == JTokenType.Object)
                    {
                        // Recursively restructure nested objects
                        RestructureProperties((JObject)property.Value);
                    }
                }

                // Remove the old properties that were restructured (after the process is done)
                foreach (var property in propertiesToRemove)
                {
                    jObject.Remove(property.Name);
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions related to property restructuring
                Console.WriteLine($"Error occurred while restructuring properties: {ex.Message}");
            }
        }
    }

    public class ListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public object ExtraDetails { get; set; } = new object(); // Use object instead of dynamic
    }
    public class JsonResponseModel
    {
        public bool IsError { get; set; } = true;
        public string StrMessage { get; set; } = string.Empty;
        public string Title { get; set; } = PopupMessageType.Error.ToString();
        public string Type { get; set; } = PopupMessageType.Error.ToString();
        public object? Result { get; set; } // Use object instead of dynamic
    }
    public class PopupModel
    {
        public string MsgTitle { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int TimeOutInMinSec { get; set; } = 0;
        public PopupMessageType MsgType { get; set; }
        public bool MsgShowButton { get; set; } = true;
    }
    public class ParentChildList
    {
        public int Level { get; set; } = 0;
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
    }
    public class MonthYearModel
    {
        public int Month { get; set; } = 0;
        public int Year { get; set; } = 0;
    }

    // To Save The File In Net Core Using #IFormFile#
    public class InMemoryFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string FilePathName { get; set; } = string.Empty;
        public byte[]? Content { get; set; }
    }
    public class PageRightsModel
    {
        public bool Insert { get; set; } = false;
        public bool Update { get; set; } = false;
        public bool Delete { get; set; } = false;
        public bool View { get; set; } = false;
    }

    public class FileService
    {
        /// <summary>
        /// Converts a Base64 encoded string to an IFormFile.
        /// </summary>
        /// <param name="base64FileData">Base64 encoded file data.</param>
        /// <param name="fileName">Name of the file (optional).</param>
        /// <param name="contentType">MIME type of the file (default is "application/octet-stream").</param>
        /// <returns>IFormFile containing the file data.</returns>
        public static IFormFile ConvertBase64ToIFormFile(string? base64FileData = "", string? fileName = "", string contentType = "application/octet-stream")
        {
            try
            {
                // Convert Base64 string to byte array
                byte[] fileBytes = Convert.FromBase64String(base64FileData);

                using (var stream = new MemoryStream(fileBytes))
                {
                    // Create IFormFile from the byte array
                    IFormFile formFile = new Microsoft.AspNetCore.Http.Internal.FormFile(stream, 0, fileBytes.Length, fileName, fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = contentType
                    };

                    return formFile;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while converting Base64 to IFormFile.", ex);
            }
        }

        /// <summary>
        /// Converts an IFormFile to a byte array asynchronously.
        /// </summary>
        /// <param name="formFile">The IFormFile to convert.</param>
        /// <returns>A byte array containing the file data.</returns>
        public static async Task<byte[]> ConvertIFormFileToByteArray(IFormFile formFile)
        {
            try
            {
                if (formFile == null)
                {
                    throw new ArgumentNullException(nameof(formFile), "The form file cannot be null.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    // Asynchronously copy the contents of the form file to memory stream
                    await formFile.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();  // Return byte array
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while converting IFormFile to byte array.", ex);
            }
        }

        /// <summary>
        /// Converts a byte array to a Base64 encoded string.
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>Base64 encoded string.</returns>
        public static string ConvertByteArrayToBase64String(byte[] byteArray)
        {
            try
            {
                if (byteArray == null || byteArray.Length == 0)
                {
                    throw new ArgumentException("The byte array cannot be null or empty.");
                }

                return Convert.ToBase64String(byteArray);  // Convert and return the Base64 string
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while converting byte array to Base64 string.", ex);
            }
        }

        /// <summary>
        /// Converts a Base64 encoded string to a byte array.
        /// </summary>
        /// <param name="base64String">Base64 encoded string to convert.</param>
        /// <returns>Byte array.</returns>
        public static byte[] ConvertBase64StringToByteArray(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                {
                    throw new ArgumentException("The Base64 string cannot be null or empty.");
                }

                return Convert.FromBase64String(base64String);  // Convert to byte array
            }
            catch (FormatException ex)
            {
                throw new FormatException("The provided string is not a valid Base64 encoded string.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while converting Base64 string to byte array.", ex);
            }
        }

        /// <summary>
        /// Generates a download link (in data URI format) from a byte array.
        /// </summary>
        /// <param name="fileBytes">The byte array representing the file.</param>
        /// <param name="fileName">The name of the file (default is "file").</param>
        /// <param name="contentType">MIME type of the file (default is "application/octet-stream").</param>
        /// <returns>A download link in data URI format.</returns>
        public static string GenerateDownloadLink(byte[] fileBytes, string fileName = "file", string contentType = "application/octet-stream")
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    throw new ArgumentException("File bytes cannot be null or empty.");
                }

                string base64String = Convert.ToBase64String(fileBytes);  // Convert byte array to Base64 string
                string downloadLink = $"data:{contentType};base64,{base64String}";  // Create data URI

                return downloadLink;  // Return the download link
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while generating the download link.", ex);
            }
        }

        /// <summary>
        /// Converts a file type string to a corresponding FileSize enum.
        /// </summary>
        /// <param name="fileTypeString">The file type string to convert.</param>
        /// <returns>The corresponding FileSize enum value.</returns>
        public static FileSize GetFileSizeEnumFromString(string fileTypeString)
        {
            if (Enum.TryParse(fileTypeString, true, out FileType fileType))
            {
                return GetFileSizeEnum(fileType);  // Get the corresponding FileSize enum
            }

            return FileSize.Unknown;  // Return Unknown if the parsing fails
        }

        /// <summary>
        /// Gets the corresponding FileSize enum based on the FileType.
        /// </summary>
        /// <param name="fileType">The file type.</param>
        /// <returns>The corresponding FileSize enum value.</returns>
        public static FileSize GetFileSizeEnum(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Document: return FileSize.Document;
                case FileType.PDF: return FileSize.PDF;
                case FileType.Image: return FileSize.Image;
                case FileType.Audio: return FileSize.Audio;
                case FileType.Video: return FileSize.Video;
                default: return FileSize.Unknown;  // Return Unknown if no match found
            }
        }

        /// <summary>
        /// Validates whether the length of a given value is valid (non-null, non-empty).
        /// </summary>
        /// <param name="controlValue">The value to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool ValidLength(object controlValue)
        {
            try
            {
                switch (controlValue)
                {
                    case string str:
                        return !string.IsNullOrEmpty(str);  // Check if string is not empty
                    case ICollection<object> collection:
                        return collection.Count > 0;  // Check if collection is not empty
                    default:
                        return false;  // Invalid type
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Length validation failed.", ex);
            }
        }

        /// <summary>
        /// Validates the file size in MB against the allowed file size based on file type.
        /// </summary>
        /// <param name="fileSizeInMB">File size in MB.</param>
        /// <param name="fileTypeStr">File type string (e.g., "Image", "PDF").</param>
        /// <returns>True if the file size is valid; otherwise, false.</returns>
        public static bool ValidateFileSize(double fileSizeInMB, string fileTypeStr)
        {
            try
            {
                if (!Enum.TryParse(fileTypeStr, true, out FileType fileType))
                {
                    throw new ArgumentException($"Invalid file type: {fileTypeStr}");
                }

                FileSize fileSizeEnum = GetFileSizeEnum(fileType);  // Get the corresponding FileSize enum
                return ValidateFileSize(fileSizeInMB, fileSizeEnum);  // Validate the file size
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while validating file size.", ex);
            }
        }

        /// <summary>
        /// Validates the file size in MB against the allowed file size based on file size enum.
        /// </summary>
        /// <param name="fileSizeInMB">File size in MB.</param>
        /// <param name="fileSize">The file size enum.</param>
        /// <returns>True if the file size is valid; otherwise, false.</returns>
        public static bool ValidateFileSize(double fileSizeInMB, FileSize fileSize)
        {
            try
            {
                if (fileSizeInMB == 0)
                {
                    return false;  // Invalid file size
                }

                var allowedSize = Convert.ToDecimal(Utility.DescriptionAttr<FileSize>(fileSize));

                return allowedSize >= Convert.ToDecimal(fileSizeInMB);  // Validate if allowed size is >= file size
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("File size validation failed.", ex);
            }
        }

        /// <summary>
        /// Determines the file type from a string (e.g., "image", "document").
        /// </summary>
        /// <param name="fileTypeString">File type as a string.</param>
        /// <returns>Corresponding FileType enum.</returns>
        public static FileType GetFileTypeFromString(string fileTypeString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileTypeString))
                {
                    throw new ArgumentException("File type string cannot be null or empty.");
                }

                if (Enum.TryParse(fileTypeString, true, out FileType fileType))
                {
                    return fileType;  // Return the corresponding enum value
                }
                else
                {
                    throw new ArgumentException($"Invalid file type: {fileTypeString}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while determining FileType.", ex);
            }
        }

        /// <summary>
        /// Validates whether the provided file extension is allowed based on the file type.
        /// </summary>
        /// <param name="fileExtension">File extension to check (e.g., ".jpg").</param>
        /// <param name="fileTypeString">The file type string (e.g., "image", "document").</param>
        /// <returns>True if the extension is valid; otherwise, false.</returns>
        public static bool ValidateFileExtension(string fileExtension, string fileTypeString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileExtension) || string.IsNullOrWhiteSpace(fileTypeString))
                {
                    return false;  // Invalid input
                }

                if (!Enum.TryParse(fileTypeString, true, out FileType fileType))
                {
                    return false;  // Invalid file type
                }

                var allowedExtensions = Utility.DescriptionAttr<FileType>(fileType).ToLower().Split(',');
                return allowedExtensions.Contains(fileExtension.ToLower());  // Check if the extension is allowed
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("File extension validation failed.", ex);
            }
        }

        /// <summary>
        /// Gets the file type based on the file extension.
        /// </summary>
        /// <param name="fileName">The name of the file, including its extension.</param>
        /// <returns>The corresponding file type as a string.</returns>
        /// <exception cref="ArgumentException">Thrown if the file name is null, empty, or lacks an extension.</exception>
        public static string GetFileTypeFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            // Extract and normalize the extension from the file name
            string extension = Path.GetExtension(fileName)?.TrimStart('.').ToLower();

            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("File name must have an extension", nameof(fileName));
            }

            // Iterate over the FileType enum and check the associated extension(s)
            foreach (FileType fileType in Enum.GetValues(typeof(FileType)))
            {
                // Retrieve the description attribute of the enum value
                var description = fileType.GetType()
                                          .GetField(fileType.ToString())
                                          .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                          .FirstOrDefault() as DescriptionAttribute;

                if (description != null)
                {
                    // Split the description into possible file extensions
                    var extensionsList = description.Description.Split(',');

                    // Check if the file extension matches any of the extensions in the list
                    if (extensionsList.Contains(extension))
                    {
                        return fileType.ToString(); // Return the matching FileType name
                    }
                }
            }

            // If no match is found, return a default file type (could also throw an exception if preferred)
            return FileType.Document.ToString();
        }

        /// <summary>
        /// Extracts the file extension from the file name.
        /// </summary>
        /// <param name="fileName">The full file name, including extension.</param>
        /// <returns>The file extension including the dot, or an empty string if extraction fails.</returns>
        public static string GetFileExtension(string fileName)
        {
            try
            {
                // Extract the file extension, ensuring it's in lowercase
                return Path.GetExtension(fileName)?.ToLower() ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Log the error and return an empty string in case of failure
                Console.WriteLine($"Error extracting file extension: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Extracts the file name without its extension.
        /// </summary>
        /// <param name="fileNameWithExtension">The full file name, including extension.</param>
        /// <returns>The file name without the extension, or an empty string in case of error.</returns>
        public static string GetFileNameWithoutExtension(string fileNameWithExtension)
        {
            try
            {
                // Extract and return the file name without its extension
                return Path.GetFileNameWithoutExtension(fileNameWithExtension);
            }
            catch (Exception ex)
            {
                // Log the error and return an empty string if extraction fails
                Console.WriteLine($"Error extracting file name without extension: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates if the provided file extension matches the allowed extensions for a given file type.
        /// </summary>
        /// <param name="fileExtension">The file extension to validate.</param>
        /// <param name="fileType">The file type to check against.</param>
        /// <returns>True if the extension is valid for the file type, otherwise false.</returns>
        public static bool ValidateFileExtension(string fileExtension, FileType fileType)
        {
            // Normalize file extension to lowercase
            fileExtension = fileExtension.ToLower();

            // Get the description of valid extensions for the given FileType
            string[] validExtensions = Utility.DescriptionAttr<FileType>(fileType).ToLower().Split(',');

            // Check if the provided file extension matches any of the valid extensions
            return validExtensions.Contains(fileExtension);
        }

        public static string GetFileExtensionFromBase64(string base64String)
        {
            // First, extract the base64 header (fixed-length signatures for different file types)
            string header = base64String.Substring(0, 20);

            // Dictionary for file signature to extension mappings
            var fileSignatures = new Dictionary<string, string>
            {
                { "/9j/", "jpg" },       // JPEG
                { "iVBORw0KGgo=", "png" }, // PNG
                { "JVBERi0x", "pdf" },   // PDF
                { "UklGR", "webp" },     // WebP
                { "R0lGODdh", "gif" },   // GIF (matching first part)
                { "R0lGODlh", "gif" },   // GIF (matching second part)
                { "AAAAF", "mp4" },      // MP4
                { "fLaC", "flac" }       // FLAC
            };

            // Check for the signature in the dictionary
            foreach (var signature in fileSignatures)
            {
                if (header.StartsWith(signature.Key))
                {
                    return signature.Value;
                }
            }

            // If no known signature is found, return "unknown"
            return "unknown";
        }

    }

}
namespace KBNovaCMS.Common.StringUtilities
{
    public static class StringExtensions
    {
        // Extension method to make the first character uppercase
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        // Extension method to make the first character lowercase
        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return char.ToLower(input[0]) + input.Substring(1);
        }

        // Extension method to reverse a string
        public static string Reverse(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        // Extension method to check if a string is a palindrome
        public static bool IsPalindrome(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string cleanInput = Regex.Replace(input.ToLower(), @"[\W_]+", ""); // Remove non-word characters
            return cleanInput == cleanInput.Reverse();
        }

        // Extension method to truncate a string to a specified length
        public static string Truncate(this string input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }

        // Extension method to convert a string to title case
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        // Extension method to convert a string to snake case
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return Regex.Replace(input, "(?<=.)([A-Z])", "_$0").ToLower();
        }

        // Extension method to check if a string is null or empty
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        // Extension method to remove diacritics from a string
        public static string RemoveDiacritics(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var ch in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(ch);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        #region String To Date Common Utilities

        #region PreDifined Format
        // Convert "dd/MM/yyyy" to DateTime
        public static bool TryParseDate(this string input, out DateTime date)
        {
            return DateTime.TryParseExact(input, Utility.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        // Convert "dd/MM/yyyy HH:mm" to DateTime
        public static bool TryParseDateTime(this string input, out DateTime date)
        {
            return DateTime.TryParseExact(input, Utility.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        // Convert "HH:mm" to TimeSpan
        public static bool TryParseTime(this string input, out TimeSpan time)
        {
            return TimeSpan.TryParseExact(input, Utility.TimeFormat, CultureInfo.InvariantCulture, out time);
        }
        #endregion

        #region Custom Format
        public static bool TryParseDate(this string input, string dateFormat, out DateTime date)
        {
            return DateTime.TryParseExact(input, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
        public static bool TryParseDateTime(this string input, string dateTimeFormat, out DateTime date)
        {
            return DateTime.TryParseExact(input, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
        public static bool TryParseTime(this string input, string timeFormat, out TimeSpan time)
        {
            return TimeSpan.TryParseExact(input, timeFormat, CultureInfo.InvariantCulture, out time);
        }
        #endregion

        // Validate if the string is a valid date
        public static bool IsValidDate(this string input)
        {
            return TryParseDate(input, out _);
        }

        // Validate if the string is a valid DateTime
        public static bool IsValidDateTime(this string input)
        {
            return TryParseDateTime(input, out _);
        }

        // Validate if the string is a valid time
        public static bool IsValidTime(this string input)
        {
            return TryParseTime(input, out _);
        }

        #endregion

        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }
    }
}

namespace KBNovaCMS.Common.NumberUtilities
{
    public static class NumberUtilities
    {
        static readonly string[] Ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
        static readonly string[] Teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        static readonly string[] ThousandsGroups = { "", " Thousand", " Million", " Billion" };

        static readonly string[] OnesGuj = { "", "એક", "બે", "ત્રણ", "ચાર", "પાંચ", "છ", "સાત", "આઠ", "નવ" };
        static readonly string[] TeensGuj = { "દસ", "અગિયાર", "બાર", "તેર", "ચૌદ", "પંદર", "સોળ", "સત્તર", "અઢાર", "ઓગણીસ" };
        static readonly string[] TensGuj = { "", "", "વિસ", "ત્રીસ", "ચૌકસ", "પંચાસ", "છલસ", "સપ્ટાસ", "અઠ્ઠાસ", "ઓગણસઠ" };
        static readonly string[] ThousandsGroupsGuj = { "", " હજાર", " મિલિયન", " બિલિયન" };

        public static string FriendlyInteger(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Negative " + FriendlyInteger(-number);

            return ConvertToWords(number, Ones, Teens, Tens, ThousandsGroups);
        }

        public static string FriendlyIntegerGuj(int number)
        {
            if (number == 0)
                return "શૂન્ય";

            if (number < 0)
                return "ઋણ " + FriendlyIntegerGuj(-number);

            return ConvertToWords(number, OnesGuj, TeensGuj, TensGuj, ThousandsGroupsGuj);
        }

        private static string ConvertToWords(int number, string[] ones, string[] teens, string[] tens, string[] thousandsGroups)
        {
            if (number == 0)
                return ones[0];

            var words = new StringBuilder();

            if (number >= 1000)
            {
                int thousandIndex = 0;
                while (number >= 1000)
                {
                    int currentGroup = number % 1000;
                    number /= 1000;

                    if (currentGroup > 0)
                    {
                        words.Insert(0, ConvertToWordsGroup(currentGroup, ones, teens, tens) + thousandsGroups[thousandIndex] + " ");
                    }
                    thousandIndex++;
                }
            }
            else
            {
                words.Append(ConvertToWordsGroup(number, ones, teens, tens));
            }

            return words.ToString().Trim();
        }

        private static string ConvertToWordsGroup(int number, string[] ones, string[] teens, string[] tens)
        {
            var words = new StringBuilder();

            if (number >= 100)
            {
                words.Append(ones[number / 100] + " Hundred ");
                number %= 100;
            }

            if (number >= 20)
            {
                words.Append(tens[number / 10] + " ");
                number %= 10;
            }

            if (number >= 10)
            {
                words.Append(teens[number - 10] + " ");
            }
            else if (number > 0)
            {
                words.Append(ones[number] + " ");
            }

            return words.ToString();
        }

        public static string DateToWritten(DateTime date)
        {
            return $"{IntegerToWritten(date.Day)} {date.ToString("MMMM", CultureInfo.InvariantCulture)} {IntegerToWritten(date.Year)}";
        }

        public static string DateToWrittenGuj(DateTime date)
        {
            return $"{IntegerToWrittenGuj(date.Day)} {MonthInGuj(date.ToString("MMMM"))} {IntegerToWrittenGuj(date.Year)}";
        }

        private static string IntegerToWritten(int number)
        {
            return FriendlyInteger(number);
        }

        private static string IntegerToWrittenGuj(int number)
        {
            return FriendlyIntegerGuj(number);
        }

        private static string MonthInGuj(string month)
        {
            return month switch
            {
                "January" => "જાન્યુઆરી",
                "February" => "ફેબ્રુઆરી",
                "March" => "માર્ચ",
                "April" => "એપ્રિલ",
                "May" => "મે",
                "June" => "જૂન",
                "July" => "જુલાઈ",
                "August" => "ઓગસ્ટ",
                "September" => "સપ્ટેમ્બર",
                "October" => "ઓક્ટોબર",
                "November" => "નવેમ્બર",
                "December" => "ડિસેમ્બર",
                _ => string.Empty
            };
        }
    }
}

namespace KBNovaCMS.Common.DateUtilities
{
    public static class DateUtilities
    {
        /// <summary>
        /// Generates a sequence of months and years between the specified start and end dates.
        /// </summary>
        /// <param name="startDate">The starting date of the range.</param>
        /// <param name="endDate">The ending date of the range.</param>
        /// <returns>An enumerable collection of MonthYearModel representing each month and year in the range.</returns>
        public static IEnumerable<MonthYearModel> MonthsYearBetween(DateTime startDate, DateTime endDate)
        {
            // Determine the correct start and end dates based on the input range
            DateTime iterator = startDate <= endDate
                ? new DateTime(startDate.Year, startDate.Month, 1)  // Start from the first day of the start month
                : new DateTime(endDate.Year, endDate.Month, 1);     // Start from the first day of the end month

            DateTime limit = startDate <= endDate
                ? new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddDays(-1)  // End at the last day of the end month
                : new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1); // Same for reversed order

            // Iterate through the months, yielding the month and year
            while (iterator <= limit)
            {
                yield return new MonthYearModel
                {
                    Month = iterator.Month,
                    Year = iterator.Year
                };

                iterator = iterator.AddMonths(1);  // Move to the next month
            }
        }
        /// <summary>
        /// Attempts to parse a date string using a predefined date format.
        /// </summary>
        /// <param name="strDate">The date string to parse.</param>
        /// <param name="resultDateTime">The parsed DateTime result, or the default value if parsing fails.</param>
        /// <returns>True if parsing was successful, otherwise false.</returns>
        public static bool DateParse(string strDate, out DateTime resultDateTime)
        {
            return DateTime.TryParseExact(strDate, Utility.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out resultDateTime);
        }

        /// <summary>
        /// Attempts to parse a date string using a custom date format.
        /// </summary>
        /// <param name="strDate">The date string to parse.</param>
        /// <param name="dateFormat">The format to use for parsing the date.</param>
        /// <param name="resultDateTime">The parsed DateTime result, or the default value if parsing fails.</param>
        /// <returns>True if parsing was successful, otherwise false.</returns>
        public static bool DateParse(string strDate, string dateFormat, out DateTime resultDateTime)
        {
            return DateTime.TryParseExact(strDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out resultDateTime);
        }
    }
}