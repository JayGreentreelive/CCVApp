using System;
using System.Reflection;
using System.Diagnostics;

namespace CCVApp.Shared
{
    public class Strings
    {
        public const string General_Yes = "Yes";
        public const string General_No = "No";
        public const string General_Ok = "Ok";
        public const string General_Cancel = "Cancel";

        public const string Error_ProfilePictureTitle = "Profile Picture";
        public const string Error_ProfilePictureMessage = "There was a problem saving your profile picture. Please try again.";

        public const string Error_NoCameraTitle = "No Camera";
        public const string Error_NoCameraMessage = "This device does not have a camera.";

        public const string Profile_SubmitChangesTitle = "Submit Changes?";
        public const string Profile_LogoutTitle = "Log Out?";

        public static string Version
        {
            get
            {
                return "0.0.1";
            }
        }

        public static string BuildTime
        {
            get
            {
                return "10-6-14";
            }
        }
    }
}
