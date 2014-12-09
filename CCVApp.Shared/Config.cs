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
                /// The color of the outline circle around the profile image.
                /// </summary>
                public const uint ProfileOutlineCircleColor = 0x959595FF;

                /// <summary>
                /// The color of the profile symbols when there is no profile picture.
                /// </summary>
                public const uint ProfileSymbolColor = 0x959595FF;

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
                /// The font to use for all non-text on the sprinboard.
                /// </summary>
                public const string Font = "FontAwesome";
               
                /// <summary>
                /// The size of font to use for the element's logo.
                /// </summary>
                public const int Element_FontSize = 23;

                /// <summary>
                /// The color of the font to use for the SELECTED element's logo and text.
                /// </summary>
                public const uint Element_SelectedFontColor = 0xE4E4E4FF;

                /// <summary>
                /// The color of the font to use for the element's logo and text.
                /// </summary>
                public const uint Element_FontColor = 0x959595FF;

                /// <summary>
                /// The color of the seperating line between elements
                /// </summary>
                public const uint Element_SeperatorColor = 0x3E3E3EFF;

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

                /// <summary>
                /// The background color of the springboard.
                /// </summary>
                public const int BackgroundColor = 0x303030FF;
            }

            public class ImageCropConfig
            {
                /// <summary>
                /// The font to use for displaying the 'ok to crop' button.
                /// </summary>
                public const string CropOkButton_Font = "FontAwesome";

                /// <summary>
                /// The text to display for the 'ok to crop' button.
                /// </summary>
                public const string CropOkButton_Text = "";

                /// <summary>
                /// The size (in font points) of the 'ok to crop' button.
                /// </summary>
                public const int CropOkButton_Size = 36;

                /// <summary>
                /// The font to use for displaying the 'cancel crop' button.
                /// </summary>
                public const string CropCancelButton_Font = "FontAwesome";

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
                /// The font to use for displaying the subNavigation toolbar's back button. (the one at the bottom)
                /// </summary>
                public const string BackButton_Font = "Bh";

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's back button when enabled. (the one at the bottom)
                /// </summary>
                public const uint BackButton_EnabledColor = 0xFFFFFFFF;

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's back button when pressed. (Android Only) (the one at the bottom)
                /// </summary>
                public const uint BackButton_PressedColor = 0x444444FF;

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's back button when disabled. (the one at the bottom)
                /// </summary>
                public const uint BackButton_DisabledColor = 0x444444FF;

                /// <summary>
                /// The text to display for the subNavigation toolbar's back button. (the one at the bottom)
                /// </summary>
                public const string BackButton_Text = "";

                /// <summary>
                /// The size (in font points) of the sub nav toolbar back button. (the one at the bottom)
                /// </summary>
                public const int BackButton_Size = 36;

                /// <summary>
                /// The font to use for displaying the subNavigation toolbar's share button. (the one at the bottom)
                /// </summary>
                public const string ShareButton_Font = "Bh";

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's share button when enabled. (the one at the bottom)
                /// </summary>
                public const uint ShareButton_EnabledColor = 0xFFFFFFFF;

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's share button when pressed. (Android Only) (the one at the bottom)
                /// </summary>
                public const uint ShareButton_PressedColor = 0x444444FF;

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's share button when disabled. (the one at the bottom)
                /// </summary>
                public const uint ShareButton_DisabledColor = 0x444444FF;

                /// <summary>
                /// The text to display for the subNavigation toolbar's share button. (the one at the bottom)
                /// </summary>
                public const string ShareButton_Text = "";

                /// <summary>
                /// The size (in font points) of the sub nav toolbar share button. (the one at the bottom)
                /// </summary>
                public const int ShareButton_Size = 36;

                /// <summary>
                /// The font to use for displaying the subNavigation toolbar's create button. (the one at the bottom)
                /// </summary>
                public const string CreateButton_Font = "Bh";

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's create button when enabled. (the one at the bottom)
                /// </summary>
                public const uint CreateButton_EnabledColor = 0xFFFFFFFF;

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's create button when pressed. (Android Only) (the one at the bottom)
                /// </summary>
                public const uint CreateButton_PressedColor = 0x444444FF;

                /// <summary>
                /// The color to use for displaying the subNavigation toolbar's create button when disabled. (the one at the bottom)
                /// </summary>
                public const uint CreateButton_DisabledColor = 0x444444FF;

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
                /// The font to use for displaying the reveal button.
                /// </summary>
                public const string RevealButton_Font = "Bh";

                /// <summary>
                /// The color of the reveal button when not pressed.
                /// </summary>
                public const uint RevealButton_DepressedColor = 0x818181FF;

                /// <summary>
                /// The color of the reveal button when pressed. (Android Only)
                /// </summary>
                public const uint RevealButton_PressedColor = 0x444444FF;

                /// <summary>
                /// The color of the reveal button when disabled. (Android Only)
                /// </summary>
                public const uint RevealButton_DisabledColor = 0x444444FF;

                /// <summary>
                /// The percentage of the navbar width to slide over when revealing the Springboard. (Android Only)
                /// </summary>
                public const float RevealPercentage = .65f;

                /// <summary>
                /// The size of the character representing the reveal button.
                /// </summary>
                public const int RevealButton_Size = 36;

                /// <summary>
                /// The color of the primary nav bar background.
                /// </summary>
                public const uint BackgroundColor = 0x303030FF;
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

                /// <summary>
                /// The color of the shadow cast by the primary container on top of the springboard.
                /// </summary>
                public const uint ShadowColor = 0x000000FF;
            }

            public class NewsConfig
            {
                /// <summary>
                /// The color of the news table background. (You most likely want this to match Table_CellBackgroundColor)
                /// </summary>
                public const uint Table_BackgroundColor = 0x1C1C1CFF;

                /// <summary>
                /// The color of the news table cell seperators.
                /// </summary>
                public const uint Table_SeperatorBackgroundColor = 0x000000FF;

                /// <summary>
                /// The color of an in-use table cell. (You most likely want this to match Table_BackgroundColor)
                /// </summary>
                public const uint Table_CellBackgroundColor = 0x1C1C1CFF;

                /// <summary>
                /// The color of an in-use table cell. (You most likely want this to match Table_BackgroundColor)
                /// </summary>
                public const uint Table_CellTextColor = 0xFFFFFFFF;
            }

            public class NoteConfig
            {
                /// <summary>
                /// The suffix to use for the user note filename.
                /// </summary>
                public const string UserNoteSuffix = "_user_note.dat";

                /// <summary>
                /// The prefix to attach when building the note name.
                /// </summary>
                public const string NamePrefix = "message";

                /// <summary>
                /// The extension of the note to download.
                /// </summary>
                public const string Extension = ".xml";

                /// <summary>
                /// The base URL to look for Notes. (this may be removed in a future version)
                /// </summary>
                public const string BaseURL = "http://www.jeredmcferron.com/";

                /// <summary>
                /// The space between the border of a view and the contents.
                /// Only applies when a border is rendered.
                /// </summary>
                public const int BorderPadding = 2;

                /// <summary>
                /// The font to use for displaying the user note icon.
                /// </summary>
                public const string UserNote_IconFont = "FontAwesome";

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
                /// The font to use for displaying the delete icon on user notes.
                /// </summary>
                public const string UserNote_DeleteIconFont = "FontAwesome";

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
                /// Color of the placeholder text displayed when the note is empty.
                /// </summary>
                public const uint UserNote_PlaceholderTextColor = 0xc7c7cdFF;

                /// <summary>
                /// The amount to scale the navBarReveal amount by for Android.
                /// </summary>
                public const float ScrollRateForNavBarReveal_AndroidScalar = .75f;

                /// <summary>
                /// The rate of scrolling "up" required to reveal the nav bar.
                /// </summary>
                public const float ScrollRateForNavBarReveal = -75;

                /// <summary>
                /// The rate of scrolling "down" required to hide the nav bar.
                /// </summary>
                public const float ScrollRateForNavBarHide = 50;


                public const string Series_Table_Large_Font = "OpenSans-Regular";
                public const uint Series_Table_Large_FontSize = 23;

                public const string Series_Table_Medium_Font = "OpenSans-Regular";
                public const uint Series_Table_Medium_FontSize = 21;

                public const string Series_Table_Small_Font = "OpenSans-Regular";
                public const uint Series_Table_Small_FontSize = 16;

                public const string Series_Table_Watch_Icon = "";
                public const string Series_Table_TakeNotes_Icon = "";
                public const string Series_Table_Navigate_Icon = "";
                public const uint Series_Table_IconSize = 48;



                /// <summary>
                /// The color of the series main (first) table background.
                /// </summary>
                public const uint Series_Main_Table_BackgroundColor = 0x1C1C1CFF;

                /// <summary>
                /// The color of an in-use table cell. (You most likely want this to match Series_Main_Table_BackgroundColor)
                /// </summary>
                public const uint Series_Main_Table_CellBackgroundColor = 0x1C1C1CFF;

                /// <summary>
                /// The color of an in-use table cell. (You most likely want this to match Series_Main_Table_BackgroundColor)
                /// </summary>
                public const uint Series_Main_Table_CellTextColor = 0xFFFFFFFF;

                /// <summary>
                /// The height that an image should be within the cell
                /// </summary>
                public const float Series_Main_CellImageHeight = 70;

                /// <summary>
                /// The width that an image should be within the cell
                /// </summary>
                public const float Series_Main_CellImageWidth = 70;


                /// <summary>
                /// The color of the series details table background. (You most likely want this to match Table_CellBackgroundColor)
                /// </summary>
                public const uint Series_Details_Table_BackgroundColor = 0x1C1C1CFF;

                /// <summary>
                /// The color of an in-use table cell. (You most likely want this to match Series_Details_Table_BackgroundColor)
                /// </summary>
                public const uint Series_Details_Table_CellBackgroundColor = 0x1C1C1CFF;

                /// <summary>
                /// The color of an in-use table cell. (You most likely want this to match Series_Details_Table_BackgroundColor)
                /// </summary>
                public const uint Series_Details_Table_CellTextColor = 0xFFFFFFFF;

                /// <summary>
                /// The height that an image should be within the cell
                /// </summary>
                public const float Series_Details_CellImageHeight = 180;

                /// <summary>
                /// The width that an image should be within the cell
                /// </summary>
                public const float Series_Details_CellImageWidth = 320;
            }

            public class PrayerConfig
            {


                /// <summary>
                /// The length of the animation when a prayer card is animating.
                /// </summary>
                public const float Card_AnimationDuration = .25f;

                public const uint Card_BackgroundColor = 0x3E3E3EFF;
                public const uint Card_BorderColor = 0x9D9D9DFF;
                public const uint Card_CornerRadius = 3;
                public const float Card_BorderWidth = .5f;

                public const string Card_DateFont = "OpenSans-Regular";
                public const uint Card_DateSize = 10;
                public const uint Card_DateColor = 0x898989FF;

                public const string Card_CategoryFont = "OpenSans-Regular";
                public const uint Card_CategorySize = 10;
                public const uint Card_CategoryColor = 0x898989FF;

                public const string  Card_NameFont = "ChangaOne-Regular";
                public const uint Card_NameSize = 16;
                public const uint Card_NameColor = 0xFFFFFFFF;

                public const string Card_PrayerFont = "OpenSans-Regular";
                public const uint Card_PrayerSize = 12;
                public const uint Card_PrayerColor = 0x989898FF;

                public const string Card_ButtonFont = "OpenSans-Regular";
                public const uint Card_ButtonSize = 12;
                public const uint Card_ButtonColor_Normal = 0x989898FF;
                public const uint Card_ButtonColor_Highlighted = 0x545454FF;

                public const string PostPrayer_ResultSymbolFont = "FontAwesome";
                public const string PostPrayer_ResultSymbol_SuccessText = "";
                public const string PostPrayer_ResultSymbol_FailedText = "";
                public const uint PostPrayer_ResultSymbolSize = 32;
            }

            public class ControlStylingConfig
            {
                /// <summary>
                /// The background color for the pages
                /// </summary>
                public const uint BackgroundColor = 0x2D2D2DFF;

                /// <summary>
                /// The color of text for buttons
                /// </summary>
                public const uint Button_TextColor = 0xFFFFFFFF;

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
                /// </summary>
                public const uint BG_Layer_BorderColor = 0x767676FF;

                /// <summary>
                /// The border thickness for the layer that backs elements (like the strip behind First Name)
                /// </summary>
                public const float BG_Layer_BorderWidth = .5f;

                /// <summary>
                /// The color for text that is not placeholder (what the user types in, control labels, etc.)
                /// </summary>
                public const uint TextField_ActiveTextColor = 0xFFFFFFFF;

                /// <summary>
                /// The color for placeholder text in fields the user can type into.
                /// </summary>
                public const uint TextField_PlaceholderTextColor = 0xA7A7A7FF;

                /// <summary>
                /// The color of text in standard labels
                /// </summary>
                public const uint Label_TextColor = 0xFFFFFFFF;

                /// <summary>
                /// The color of a UI Switch when turned 'on'
                /// </summary>
                public const uint Switch_OnColor = 0x7E7E7EFF;

                public const string Icon_Font_Primary = "Bh";
                public const string Icon_Font_Secondary = "FontAwesome";

                public const uint Table_Footer_Color = 0x262626FF;
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
