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
            public class Springboard
            {
                /// <summary>
                /// Image to display when a user does not have a profile.
                /// </summary>
                public const string NoProfileFile = "noprofile.png";

                /// <summary>
                /// Image to display when a user is logged in without a profile picture.
                /// </summary>
                public const string NoPhotoFile = "addphoto.png";

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
                /// The background color of the springboard.
                /// </summary>
                public const int BackgroundColor = 0x1C1C1CFF;
            }

            public class SubNavToolbar
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
                public const float SlideRate = .50f;

                /// <summary>
                /// The font to use for displaying the subNavigation toolbar's back button. (the one at the bottom)
                /// </summary>
                public const string BackButton_Font = "FontAwesome";

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
                public const string BackButton_Text = "";

                /// <summary>
                /// The size (in font points) of the sub nav toolbar back button. (the one at the bottom)
                /// </summary>
                public const int BackButton_Size = 36;
            }

            /// <summary>
            /// Settings for the primary nav bar (the one at the top)
            /// </summary>
            public class PrimaryNavBar
            {
                /// <summary>
                /// The logo to be displayed on the primary nav bar.
                /// </summary>
                public const string LogoFile = "ccvLogo.png";

                /// <summary>
                /// The character to be displayed representing the reveal button.
                /// </summary>
                public const string RevealButton_Text = "";

                /// <summary>
                /// The font to use for displaying the reveal button.
                /// </summary>
                public const string RevealButton_Font = "FontAwesome";

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
                public const uint BackgroundColor = 0x1C1C1CFF;
            }

            /// <summary>
            /// Settings for the primary container that all activities lie within.
            /// </summary>
            public class PrimaryContainer
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

            public sealed class News
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

            public sealed class Note
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
                /// The rate of scrolling "up" required to reveal the nav bar.
                /// </summary>
                public const float ScrollRateForNavBarReveal = -150;

                /// <summary>
                /// The amount to scale the navBarReveal amount by for Android.
                /// </summary>
                public const float ScrollRateForNavBarReveal_AndroidScalar = .75f;

                /// <summary>
                /// The rate of scrolling "down" required to hide the nav bar.
                /// </summary>
                public const float ScrollRateForNavBarHide = 50;
            }
        }
    }
}
