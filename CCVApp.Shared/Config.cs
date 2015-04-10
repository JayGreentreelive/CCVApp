using System;
using System.Drawing;
using Rock.Mobile.PlatformUI;

namespace CCVApp
{
    namespace Shared
    {
        /// <summary>
        /// Config contains values for customizing various aspects of CCVApp.
        /// </summary>
        namespace Config
        {
            public class OOBEConfig
            {
                public const string Welcome = "Welcome";
                public const string HaveAccount = "I have a {0} Account";
                public const string WantAccount = "Create a {0} Account";
                public const string SkipAccount = "Do this Later";
            }

            public class GeneralConfig
            {
                /// <summary>
                /// The full name of your organization
                /// </summary>
                public const string OrganizationName = "Christ's Church of the Valley";

                /// <summary>
                /// The abbreviated name of your organization. (You can set this to OrganizationName if desired)
                /// </summary>
                public const string OrganizationShortName = "CCV";

                /// <summary>
                /// The size (in pixels) of the profile image to download from Rock
                /// </summary>
                public const uint ProfileImageSize = 200;

                /// <summary>
                /// The Facebook app ID reprsenting the Facebook app that will get things on behalf of the user.
                /// </summary>
                public const string FBAppID = "495461873811179";

                /// <summary>
                /// The permissions the Facebook App should request. You probably shouldn't change this.
                /// </summary>
                public const string FBAppPermissions = "public_profile, user_friends, email";

                /// <summary>
                /// Used when creating new addresses that will be sent to Rock. If your app will run in another country,
                /// set CountryCode to the ISO country code.
                /// </summary>
                public const string CountryCode = "US";

                /// <summary>
                /// Defined in Rock, this should NEVER change, and is the key the mobile app uses so Rock
                /// knows who it's talking to.
                /// </summary>
                public const string RockMobileAppAuthorizationKey = "hWTaZ7buziBcJQH31KCm3Pzz";

                /// <summary>
                /// These are values that, while generated when the Rock database is created,
                /// are extremely unlikely to ever change. If they do change, simply update them here to match
                /// Rock.
                /// </summary>
                public const int CellPhoneValueId = 12;
                public const int NeighborhoodGroupGeoFenceValueId = 48;
                public const int NeighborhoodGroupValueId = 49;
                public const int GroupLocationTypeHomeValueId = 19;
                public const int GroupMemberStatus_Pending_ValueId = 2;
                public const int ApplicationGroup_PhotoRequest_ValueId = 1207885;
                public const int GroupMemberRole_Member_ValueId = 59;
                public const int GeneralDataTimeValueId = 2623;
                public const int UserLoginEntityTypeId = 27;
                public const int GroupRegistrationValueId = 52;

                /// <summary>
                /// iOS only, this controls what style of keyboard is used for PLATFORM textFields.
                /// Meaning the ones dynamically created in code via Rock.Mobile. 
                /// Any normal iOS text field needs to have its style explicitely set.
                /// Although this is an int, it should match the KeyboardAppearance enum.
                /// </summary>
                public const KeyboardAppearanceStyle iOSPlatformUIKeyboardAppearance = KeyboardAppearanceStyle.Dark;

                public const string NewsMainPlaceholder = "placeholder_news_main.png";
                public const string NewsDetailsPlaceholder = "placeholder_news_details.png";

                public const string NotesMainPlaceholder = "placeholder_notes_main.png";
                public const string NotesThumbPlaceholder = "placeholder_notes_thumb.png";
            }

            public class SpringboardConfig
            {
                /// <summary>
                /// The number of hours until the app will resync data with Rock when it is resumed.
                /// This allows someone to put the app in the background (but not kill it), and come back
                /// later to updated rock data (news, notes, etc.)
                /// </summary>
                public const int SyncRockHoursFrequency = 12;

                /// <summary>
                /// The text glyph to use as a symbol when the user does not have a profile.
                /// </summary>
                public const string NoProfileSymbol = "";

                /// <summary>
                /// The text glyph to use as a symbol when the user doesn't have a photo.
                /// </summary>
                public const string NoPhotoSymbol = "";

                /// <summary>
                /// The size of font to use for the no photo symbol
                /// </summary>
                public const float ProfileSymbolFontSize = 48;

                /// <summary>
                /// When we store their profile pic, thisi s what it's called.
                /// When the HasProfileImage flag is true, we'll load it from this file.
                /// </summary>
                public const string ProfilePic = "userPhoto.jpg";

                /// <summary>
                /// The symbol to use representing the settings button.
                /// </summary>
                public const string SettingsSymbol = "";

                /// <summary>
                /// The size of the symbol representing the settings button.
                /// </summary>
                public const float SettingsSymbolSize = 14;
               
                /// <summary>
                /// The size of font to use for the element's logo.
                /// </summary>
                public const int Element_FontSize = 23;

                /// <summary>
                /// The X offset to place the CENTER of the element's logo.
                /// </summary>
                public const int Element_LogoOffsetX = 20;

                /// <summary>
                /// The X offset to place the LEFT EDGE of the element's text.
                /// </summary>
                public const int Element_LabelOffsetX = 40;

                /// <summary>
                /// The background color of a selected element.
                /// </summary>
                public const int Element_SelectedColor = 0x7a1315FF;

                /// <summary>
                /// The icon to use representing the News element
                /// </summary>
                public const string Element_News_Icon = "";

                /// <summary>
                /// The icon to use representing the Connect element
                /// </summary>
                public const string Element_Connect_Icon = "";

                /// <summary>
                /// The icon to use representing the Messages element
                /// </summary>
                public const string Element_Messages_Icon = "";

                /// <summary>
                /// The icon to use representing the Prayer element
                /// </summary>
                public const string Element_Prayer_Icon = "";

                /// <summary>
                /// The icon to use representing the Give element
                /// </summary>
                public const string Element_Give_Icon = "";

                /// <summary>
                /// The icon to use representing the More element
                /// </summary>
                public const string Element_More_Icon = "";
            }

            public class ImageCropConfig
            {
                /// <summary>
                /// The text to display for the 'ok to crop' button.
                /// </summary>
                public const string CropOkButton_Text = "";

                /// <summary>
                /// The size (in font points) of the 'ok to crop' button.
                /// </summary>
                public const int CropOkButton_Size = 36;

                /// <summary>
                /// The text to display for the 'cancel crop' button.
                /// </summary>
                public const string CropCancelButton_Text = "";//""; (This commented out one is the X with a circle)

                /// <summary>
                /// The size (in font points) of the 'cancel crop' button.
                /// </summary>
                public const int CropCancelButton_Size = 36;
            }

            public class SubNavToolbarConfig
            {
                /// <summary>
                /// The height of the subNavigation toolbar (the one at the bottom)
                /// </summary>
                public const float Height = 44;

                /// <summary>
                /// The color of the subNavigation toolbar (the one at the bottom)
                /// </summary>
                public const uint BackgroundColor = 0x505050FF;

                /// <summary>
                /// The amount of opacity (see throughyness) of the subNavigation toolbar (the one at the bottom)
                /// 1 = fully opaque, 0 = fully transparent.
                /// </summary>
                public const float Opacity = .75f;

                /// <summary>
                /// The amount of time (in seconds) it takes for the subNavigation toolbar (the one at the bottom)
                /// to slide up or down.
                /// </summary>
                public const float SlideRate = .30f;

                /// <summary>
                /// On iOS only, the amount of space between buttons on the subNavigation toolbar (the one at the bottom)
                /// </summary>
                public const float iOS_ButtonSpacing = 5.0f;

                /// <summary>
                /// The text to display for the subNavigation toolbar's back button. (the one at the bottom)
                /// </summary>
                public const string BackButton_Text = "";

                /// <summary>
                /// The size (in font points) of the sub nav toolbar back button. (the one at the bottom)
                /// </summary>
                public const int BackButton_Size = 36;

                /// <summary>
                /// The text to display for the subNavigation toolbar's share button. (the one at the bottom)
                /// </summary>
                public const string ShareButton_Text = "";

                /// <summary>
                /// The size (in font points) of the sub nav toolbar share button. (the one at the bottom)
                /// </summary>
                public const int ShareButton_Size = 36;

                /// <summary>
                /// The text to display for the subNavigation toolbar's create button. (the one at the bottom)
                /// </summary>
                public const string CreateButton_Text = "";

                /// <summary>
                /// The size (in font points) of the sub nav toolbar create button. (the one at the bottom)
                /// </summary>
                public const int CreateButton_Size = 36;
            }

            /// <summary>
            /// Settings for the primary nav bar (the one at the top)
            /// </summary>
            public class PrimaryNavBarConfig
            {
                /// <summary>
                /// The logo to be displayed on the primary nav bar.
                /// </summary>
                public const string LogoFile = "ccvLogo.png";

                /// <summary>
                /// The character to be displayed representing the reveal button.
                /// </summary>
                public const string RevealButton_Text = "";

                /// <summary>
                /// The percentage of the navbar width to slide over when revealing the Springboard. (Android Only)
                /// </summary>
                public const float RevealPercentage = .35f;

                /// <summary>
                /// The size of the character representing the reveal button.
                /// </summary>
                public const int RevealButton_Size = 36;
            }

            /// <summary>
            /// Settings for the primary container that all activities lie within.
            /// </summary>
            public class PrimaryContainerConfig
            {
                /// <summary>
                /// Time (in seconds) it takes for the primary container to slide in/out to reveal the springboard.
                /// </summary>
                public const float SlideRate = .50f;

                /// <summary>
                /// The amount to slide when revelaing the springboard.
                /// </summary>
                public const float SlideAmount = 230;

                /// <summary>
                /// The max amount to darken the panel when revealing the springboard. ( 0 - 1 )
                /// </summary>
                public const float SlideDarkenAmount = .75f;

                /// <summary>
                /// The darkness of the shadow cast by the primary container on top of the springboard.
                /// 1 = fully opaque, 0 = fully transparent.
                /// </summary>
                public const float ShadowOpacity = .60f;

                /// <summary>
                /// The offset of the shadow cast by the primary container on top of the springboard.
                /// </summary>
                public static SizeF ShadowOffset = new SizeF( 0.0f, 5.0f );
            }

            public class NoteConfig
            {
                /// <summary>
                /// The image to display for the tutorial screen.
                /// </summary>
                public const string TutorialOverlayImage = "note_tutorial.png";
                public const string TutorialOverlayImageIPadLS = "note_tutorial_ipad_ls.png";
                public const string TutorialOverlayImageIPadPort = "note_tutorial_ipad_port.png";

                /// <summary>
                /// The suffix to use for the user note filename.
                /// </summary>
                public const string UserNoteSuffix = "_user_note.dat";

                /// <summary>
                /// The base URL to look for Notes. (this may be removed in a future version)
                /// </summary>
                public const string BaseURL = "http://ccv.church/ccvmobile/";

                /// <summary>
                /// The space between the border of a view and the contents.
                /// Only applies when a border is rendered.
                /// </summary>
                public const int BorderPadding = 2;

                /// <summary>
                /// The icon to use for displaying the citation icon.
                /// </summary>
                public const string CitationUrl_Icon = "";

                /// <summary>
                /// The size of the font/icon when displaying the citation icon.
                /// </summary>
                public const int CitationUrl_IconSize = 24;

                /// <summary>
                /// The color of the font/icon when displaying the citation icon.
                /// </summary>
                public const uint CitationUrl_IconColor = 0x878686BB;

                /// <summary>
                /// The icon to use for displaying the user note icon.
                /// </summary>
                public const string UserNote_Icon = "";

                /// <summary>
                /// The color of the user note anchor (which is what the user interacts with to move, open, close and delete the note)
                /// </summary>
                public const uint UserNote_AnchorColor = 0x77777777;

                /// <summary>
                /// The color of the user note anchor (which is what the user interacts with to move, open, close and delete the note)
                /// when the note is in delete mode.
                /// </summary>
                public const uint UserNote_DeleteAnchorColor = 0x7A1315FF;

                /// <summary>
                /// The size of the font/icon when the usernote is OPEN.
                /// </summary>
                public const int UserNote_IconOpenSize = 30;

                /// <summary>
                /// The size of the font/icon when the usernote is CLOSED.
                /// </summary>
                public const int UserNote_IconClosedSize = 46;

                /// <summary>
                /// The max height of a user note.
                /// </summary>
                public const float UserNote_MaxHeight = 200;

                /// <summary>
                /// The color of the font/icon when displaying the user note icon.
                /// </summary>
                public const uint UserNote_IconColor = 0x8c8c8cFF;

                /// <summary>
                /// The icon to use for displaying the delete icon on user notes.
                /// </summary>
                public const string UserNote_DeleteIcon = "X";//"";

                /// <summary>
                /// The color of the font/icon when displaying the delete icon on user notes.
                /// </summary>
                public const uint UserNote_DeleteIconColor = 0xFFFFFFFF;

                /// <summary>
                /// The size of the font/icon when displaying the delete icon on user notes.
                /// </summary>
                /// 
                public const int UserNote_DeleteIconSize = 32;

                /// <summary>
                /// The icon to use representing "Listen to this Message"
                /// </summary>
                public const string Series_Table_Listen_Icon = "";

                /// <summary>
                /// The icon to use representing "Watch this Message"
                /// </summary>
                public const string Series_Table_Watch_Icon = "";

                /// <summary>
                /// The icon to use representing the action to "Take Notes"
                /// </summary>
                public const string Series_Table_TakeNotes_Icon = "";

                /// <summary>
                /// The icon to use representing that tapping the element will take you to a new page. (Like a > symbol)
                /// </summary>
                public const string Series_Table_Navigate_Icon = "";

                /// <summary>
                /// The size of icons in the Series table.
                /// </summary>
                public const uint Series_Table_IconSize = 36;

                /// <summary>
                /// The height that an image should be within the cell
                /// </summary>
                public const float Series_Main_CellHeight = 70;

                /// <summary>
                /// The width that an image should be within the cell
                /// </summary>
                public const float Series_Main_CellWidth = 70;

                /// <summary>
                /// Make Private
                /// </summary>
                public const uint Details_Table_IconSize = 62;
                public const uint Details_Table_IconColor = 0xc43535FF;
            }

            public class PrayerConfig
            {
                /// <summary>
                /// The interval to download prayer requests. (Between this time they will be cached in memory)
                /// They WILL be redownloaded if the app is quit and re-run.
                /// </summary>
                public static TimeSpan PrayerDownloadFrequency = new TimeSpan( 1, 0, 0 );

                /// <summary>
                /// The length of the animation when a prayer card is animating.
                /// </summary>
                public const float Card_AnimationDuration = .25f;

                /// <summary>
                /// The size of the symbol used to representing a prayer post result.
                /// </summary>
                public const uint PostPrayer_ResultSymbolSize = 64;
            }

            public class ConnectConfig
            {
                public static string[] WebViews = 
                    {
                        "Baptisms", "http://www.ccvonline.com/Arena/default.aspx?page=17655&campus=1", "baptism_thumb.png",
                        "Starting Point", "http://www.ccvonline.com/Arena/default.aspx?page=17400&campus=1", "starting_point_thumb.png",
                        "Foundations", "http://www.ccvonline.com/Arena/default.aspx?page=17659&campus=1", "foundations_thumb.png"
                    };

                public const string GroupFinder_IconImage = "groupfinder_thumb.png";

                public const string MainPageHeaderImage = "connect_banner.jpg";

                /// <summary>
                /// The width/height of the image used as a thumbnail for each entry in the "Other ways to connect"
                /// </summary>
                public const float MainPage_ThumbnailDimension = 70;

                /// <summary>
                /// The icon to use representing that tapping the element will take you to a new page. (Like a > symbol)
                /// </summary>
                public const string MainPage_Table_Navigate_Icon = "";

                /// <summary>
                /// The size of icons navigate icon in each row.
                /// </summary>
                public const uint MainPage_Table_IconSize = 36;

                /// <summary>
                /// The default latitude the group finder map will be positioned to before a search is performed.
                /// </summary>
                public const double GroupFinder_DefaultLatitude = 33.6054149;

                /// <summary>
                /// The default longitude the group finder map will be positioned to before a search is performed. 
                /// </summary>
                public const double GroupFinder_DefaultLongitude = -112.125051;

                /// <summary>
                /// The default latitude scale (how far out) the map will be scaled to before a search is performed.
                /// </summary>
                public const int GroupFinder_DefaultScale_iOS = 100000;

                /// <summary>
                /// The default longitude scale (how far out) the map will be scaled to before a search is performed. 
                /// </summary>
                public const float GroupFinder_DefaultScale_Android = 9.25f;

                /// <summary>
                /// The icon to use representing the search button
                /// </summary>
                public const string GroupFinder_SearchIcon = "";

                /// <summary>
                /// The icon to use representing the join button.
                /// </summary>
                public const string GroupFinder_JoinIcon = "";

                public const uint GroupFinder_Join_IconSize = 64;

                /// <summary>
                /// The color for the row of the group that's closest to the address searched
                /// </summary>
                public const int GroupFinder_ClosestGroupColor = 0x5B1013FF;
            }

            public class GiveConfig
            {
                public const string GiveUrl = "https://www.ccvonline.com/Arena/default.aspx?page=18485&campus=1";
            }

            public class ControlStylingConfig
            {
                public const uint SpringboardBackgroundColor = 0x2D2D2DFF;

                public const uint Springboard_ActiveElementColor = 0xFFFFFFFF;

                public const uint Springboard_InActiveElementColor = 0xA7A7A7FF;

                public const uint PrimaryNavBarBackgroundColor = 0x191919FF;

                /// <summary>
                /// The background color for the pages (basically the darkest area)
                /// </summary>
                public const uint BackgroundColor = 0x212121FF;

                /// <summary>
                /// The color of text for buttons
                /// </summary>
                public const uint Button_TextColor = 0xCCCCCCFF;

                /// <summary>
                /// The background color for buttons
                /// </summary>
                public const uint Button_BGColor = 0x7E7E7EFF;

                /// <summary>
                /// The corner roundedness for buttons (0 is no curvature)
                /// </summary>
                #if __IOS__
                public const uint Button_CornerRadius = 3;
                #else
                public const uint Button_CornerRadius = 0;
                #endif

                /// <summary>
                /// The background color for the layer that backs elements (like the strip behind First Name)
                /// </summary>
                public const uint BG_Layer_Color = 0x3E3E3EFF;

                /// <summary>
                /// The background color for a text layer that has invalid input. This lets the user know
                /// there is something wrong with the particular field.
                /// </summary>
                public const uint BadInput_BG_Layer_Color = 0x5B1013FF;

                /// <summary>
                /// The border color for the layer that backs elements (like the strip behind First Name)
                /// This can also be used to highlight certain elements, as it is a bright color.
                /// </summary>
                public const uint BG_Layer_BorderColor = 0x595959FF;

                /// <summary>
                /// The border thickness for the layer that backs elements (like the strip behind First Name)
                /// </summary>
                public const float BG_Layer_BorderWidth = .5f;

                /// <summary>
                /// The color for text that is not placeholder (what the user types in, control labels, etc.)
                /// </summary>
                public const uint TextField_ActiveTextColor = 0xCCCCCCFF;

                /// <summary>
                /// The color for placeholder text in fields the user can type into.
                /// </summary>
                public const uint TextField_PlaceholderTextColor = 0x878686FF;

                /// <summary>
                /// The color of text in standard labels
                /// </summary>
                public const uint Label_TextColor = 0xCCCCCCFF;

                /// <summary>
                /// The color of a UI Switch when turned 'on'
                /// </summary>
                public const uint Switch_OnColor = 0x7E7E7EFF;

                /// <summary>
                /// The primary (most commonly used) icon font
                /// </summary>
                public const string Icon_Font_Primary = "FontAwesome";

                /// <summary>
                /// The secondary (used in occasional spots) icon font
                /// </summary>
                public const string Icon_Font_Secondary = "Bh";

                /// <summary>
                /// The symbol to use for a result that was successful.
                /// </summary>
                public const string Result_Symbol_Success = "";

                /// <summary>
                /// The symbol to use for a result that failed.
                /// </summary>
                public const string Result_Symbol_Failed = "";

                /// <summary>
                /// The color for the footers of primary table cells. (Like the footer in the Messages->Series primary cell that says "Previous Messages")
                /// </summary>
                public const uint Table_Footer_Color = 0x262626FF;

                /// <summary>
                /// The font to use representing a large bold font throughout the app.
                /// </summary>
                public const string Large_Font_Bold = "OpenSans-Semibold";

                /// <summary>
                /// The font to use representing a large regular font throughout the app.
                /// </summary>
                public const string Large_Font_Regular = "OpenSans-Regular";

                /// <summary>
                /// The font to use representing a large light font throughout the app.
                /// </summary>
                public const string Large_Font_Light = "OpenSans-Light";

                /// <summary>
                /// The size of to use for the large font throughout the app.
                /// </summary>
                public const uint Large_FontSize = 23;

                /// <summary>
                /// The font to use representing a medium bold font throughout the app.
                /// </summary>
                public const string Medium_Font_Bold = "OpenSans-Bold";

                /// <summary>
                /// The font to use representing a medium regular font throughout the app.
                /// </summary>
                public const string Medium_Font_Regular = "OpenSans-Regular";

                /// <summary>
                /// The font to use representing a medium light font throughout the app.
                /// </summary>
                public const string Medium_Font_Light = "OpenSans-Light";

                /// <summary>
                /// The size of to use for the medium font throughout the app.
                /// </summary>
                public const uint Medium_FontSize = 19;

                /// <summary>
                /// The font to use representing a small bold font throughout the app.
                /// </summary>
                public const string Small_Font_Bold = "OpenSans-Bold";

                /// <summary>
                /// The font to use representing a small regular font throughout the app.
                /// </summary>
                public const string Small_Font_Regular = "OpenSans-Regular";

                /// <summary>
                /// The font to use representing a small light font throughout the app.
                /// </summary>
                public const string Small_Font_Light = "OpenSans-Light";

                /// <summary>
                /// The size of to use for the small font throughout the app.
                /// </summary>
                public const uint Small_FontSize = 16;
            }

            public class AboutConfig
            {
                /// <summary>
                /// The page to navigate to in the About's embedded webview.
                /// </summary>
                public const string Url = "http://www.ccvonline.com/Arena/default.aspx?page=17623&campus=1";
            }
        }
    }
}
