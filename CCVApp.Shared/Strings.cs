using System;
using System.Reflection;
using System.Diagnostics;

namespace CCVApp.Shared
{
    namespace Strings
    {
        public class Build
        {
            public static string Version
            {
                get
                {
                    return "0.0.2";
                }
            }

            public static string BuildTime
            {
                get
                {
                    return "11-4-14";
                }
            }
        }

        public class General
        {
            public const string Yes = "Yes";
            public const string No = "No";
            public const string Ok = "Ok";
            public const string Cancel = "Cancel";
        }

        public class Springboard
        {
            public const string ProfilePicture_SourceTitle = "Profile Picture";
            public const string ProfilePicture_SourceDescription = "Select a source for your profile picture.";
            public const string ProfilePicture_SourcePhotoLibrary = "Photo Library";
            public const string ProfilePicture_SourceCamera = "Take Photo";

            public const string ProfilePicture_Error_Title = "Profile Picture";
            public const string ProfilePicture_Error_Message = "There was a problem saving your profile picture. Please try again.";

            public const string Camera_Error_Title = "No Camera";
            public const string Camera_Error_Message = "This device does not have a camera.";
        }

        public class Profile
        {
            public const string SubmitChangesTitle = "Submit Changes?";
            public const string LogoutTitle = "Log Out?";
        }
    }
}
