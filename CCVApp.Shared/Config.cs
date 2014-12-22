﻿using System;
using System.Drawing;

namespace CCVApp
{
    namespace Shared
    {
        /// <summary>
        /// Config contains values for customizing various aspects of CCVApp.
        /// </summary>
        namespace Config
        {
            public class GeneralConfig
            {
                public const string ImageCacheDirectory = "cache";

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
            }

            public class SpringboardConfig
            {
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
                public const string SettingsSymbol = "";

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
                public const string CropOkButton_Text = "";

                /// <summary>
                /// The size (in font points) of the 'ok to crop' button.
                /// </summary>
                public const int CropOkButton_Size = 36;

                /// <summary>
                /// The text to display for the 'cancel crop' button.
                /// </summary>
                public const string CropCancelButton_Text = "";

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
                public const uint BackgroundColor = 0x1C1C1CFF;

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
                public const float RevealPercentage = .65f;

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
                /// The suffix to use for the user note filename.
                /// </summary>
                public const string UserNoteSuffix = "_user_note.dat";

                /// <summary>
                /// The base URL to look for Notes. (this may be removed in a future version)
                /// </summary>
                public const string BaseURL = "http://www.jeredmcferron.com/ccv/";

                /// <summary>
                /// The space between the border of a view and the contents.
                /// Only applies when a border is rendered.
                /// </summary>
                public const int BorderPadding = 2;

                /// <summary>
                /// The icon to use for displaying the user note icon.
                /// </summary>
                public const string UserNote_Icon = "";

                /// <summary>
                /// The size of the font/icon when displaying the user note icon.
                /// </summary>
                public const int UserNote_IconSize = 24;

                /// <summary>
                /// The max height of a user note.
                /// </summary>
                public const float UserNote_MaxHeight = 200;

                /// <summary>
                /// The color of the font/icon when displaying the user note icon.
                /// </summary>
                public const uint UserNote_IconColor = 0x7a1315FF;

                /// <summary>
                /// The icon to use for displaying the delete icon on user notes.
                /// </summary>
                public const string UserNote_DeleteIcon = "";

                /// <summary>
                /// The color of the font/icon when displaying the delete icon on user notes.
                /// </summary>
                public const uint UserNote_DeleteIconColor = 0x444444FF;

                /// <summary>
                /// The size of the font/icon when displaying the delete icon on user notes.
                /// </summary>
                /// 
                public const int UserNote_DeleteIconSize = 24;

                /// <summary>
                /// The rate of scrolling "up" required to reveal the nav bar.
                /// </summary>
                public const float ScrollRateForNavBarReveal = -75;

                /// <summary>
                /// The rate of scrolling "down" required to hide the nav bar.
                /// </summary>
                public const float ScrollRateForNavBarHide = 50;

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
                public const float Series_Main_CellImageHeight = 70;

                /// <summary>
                /// The width that an image should be within the cell
                /// </summary>
                public const float Series_Main_CellImageWidth = 70;

                /// <summary>
                /// Make Private
                /// </summary>
                public const uint Details_Table_IconSize = 42;
            }

            public class PrayerConfig
            {
                /// <summary>
                /// The length of the animation when a prayer card is animating.
                /// </summary>
                public const float Card_AnimationDuration = .25f;

                /// <summary>
                /// The symbol to use when a prayer is successfully posted.
                /// </summary>
                public const string PostPrayer_ResultSymbol_SuccessText = "";

                /// <summary>
                /// The symbol to use when there was an error posting a prayer.
                /// </summary>
                public const string PostPrayer_ResultSymbol_FailedText = "";

                /// <summary>
                /// The size of the symbol used to representing a prayer post result.
                /// </summary>
                public const uint PostPrayer_ResultSymbolSize = 32;
            }

            public class ControlStylingConfig
            {
                public const uint SpringboardBackgroundColor = 0x2D2D2DFF;

                public const uint Springboard_ActiveElementColor = 0xFFFFFFFF;

                /// <summary>
                /// The color for placeholder text in fields the user can type into.
                /// </summary>
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
                public const uint Button_CornerRadius = 3;

                /// <summary>
                /// The background color for the layer that backs elements (like the strip behind First Name)
                /// </summary>
                public const uint BG_Layer_Color = 0x3E3E3EFF;

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
                public const string Url = "http://www.ccvonline.com/Arena/default.aspx?page=17623";
            }
        }
    }
}
