using System;
using Rock.Mobile.PlatformUI;
using System.Drawing;
using CCVApp.Shared.Config;

namespace CCVApp.Shared
{
    public class UIJoinGroup
    {
        PlatformView View { get; set; }

        PlatformLabel GroupTitle { get; set; }

        PlatformLabel GroupDetails { get; set; }
        PlatformView GroupDetailsLayer { get; set; }

        PlatformTextView FirstName { get; set; }
        PlatformView FirstNameLayer { get; set; }

        PlatformTextView LastName { get; set; }
        PlatformView LastNameLayer { get; set; }

        PlatformTextView SpouseName { get; set; }
        PlatformView SpouseNameLayer { get; set; }

        PlatformTextView Email { get; set; }
        PlatformView EmailLayer { get; set; }

        public PlatformTextView CellPhone { get; set; }
        PlatformView CellPhoneLayer { get; set; }

        PlatformButton JoinButton { get; set; }

        public UIJoinGroup( )
        {
        }

        public void Create( object masterView, RectangleF frame )
        {
            View = PlatformView.Create( );
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.AddAsSubview( masterView );

            GroupTitle = PlatformLabel.Create( );
            GroupTitle.AddAsSubview( masterView );
            GroupTitle.SetFont( ControlStylingConfig.Large_Font_Light, ControlStylingConfig.Large_FontSize );
            GroupTitle.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            GroupTitle.TextAlignment = TextAlignment.Center;

            GroupDetailsLayer = PlatformView.Create( );
            GroupDetailsLayer.AddAsSubview( masterView );
            GroupDetailsLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            GroupDetailsLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            GroupDetailsLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            GroupDetails = PlatformLabel.Create( );
            GroupDetails.AddAsSubview( masterView );
            GroupDetails.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            GroupDetails.TextColor = ControlStylingConfig.TextField_ActiveTextColor;



            // Name Info
            FirstNameLayer = PlatformView.Create( );
            FirstNameLayer.AddAsSubview( masterView );
            FirstNameLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            FirstNameLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            FirstNameLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            FirstName = PlatformTextView.Create( );
            FirstName.AddAsSubview( masterView );
            FirstName.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            FirstName.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            FirstName.Placeholder = "First Name";
            FirstName.TextColor = ControlStylingConfig.TextField_ActiveTextColor;


            LastNameLayer = PlatformView.Create( );
            LastNameLayer.AddAsSubview( masterView );
            LastNameLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            LastNameLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            LastNameLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            LastName = PlatformTextView.Create( );
            LastName.AddAsSubview( masterView );
            LastName.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            LastName.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            LastName.Placeholder = "Last Name";
            LastName.TextColor = ControlStylingConfig.TextField_ActiveTextColor;


            SpouseNameLayer = PlatformView.Create( );
            SpouseNameLayer.AddAsSubview( masterView );
            SpouseNameLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            SpouseNameLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            SpouseNameLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            SpouseName = PlatformTextView.Create( );
            SpouseName.AddAsSubview( masterView );
            SpouseName.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            SpouseName.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            SpouseName.Placeholder = "Spouse Name";
            SpouseName.TextColor = ControlStylingConfig.TextField_ActiveTextColor;


            // Contact Info
            EmailLayer = PlatformView.Create( );
            EmailLayer.AddAsSubview( masterView );
            EmailLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            EmailLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            EmailLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            Email = PlatformTextView.Create( );
            Email.AddAsSubview( masterView );
            Email.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            Email.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            Email.Placeholder = "Email";
            Email.TextColor = ControlStylingConfig.TextField_ActiveTextColor;


            CellPhoneLayer = PlatformView.Create( );
            CellPhoneLayer.AddAsSubview( masterView );
            CellPhoneLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            CellPhoneLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            CellPhoneLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            CellPhone = PlatformTextView.Create( );
            CellPhone.AddAsSubview( masterView );
            CellPhone.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            CellPhone.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            CellPhone.Placeholder = "Cellphone";
            CellPhone.TextColor = ControlStylingConfig.TextField_ActiveTextColor;


            // Join Button
            JoinButton = PlatformButton.Create( );
            JoinButton.AddAsSubview( masterView );
            JoinButton.ClickEvent = JoinGroup;
            JoinButton.BackgroundColor = ControlStylingConfig.Button_BGColor;
            JoinButton.TextColor = ControlStylingConfig.Button_TextColor;
            JoinButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
        }

        public void DisplayView( string groupTitle, string distance, string meetingTime, int groupId )
        {
            // set the group title
            GroupTitle.Text = groupTitle;
            GroupTitle.Frame = new RectangleF( 0, 0, View.Frame.Width, 50 );

            // set the details for the group (distance, meeting time, etc)
            GroupDetailsLayer.Frame = new RectangleF( 0, GroupTitle.Frame.Bottom, View.Frame.Width, 62 );
            GroupDetails.Frame = new RectangleF( 0, GroupDetailsLayer.Frame.Top + 2, View.Frame.Width, 60 );
            GroupDetails.Text = meetingTime + "\n" + distance;
            GroupDetails.TextAlignment = TextAlignment.Center;

            float sectionSpacing = 25;


            // Name Info
            FirstNameLayer.Frame = new RectangleF( 0, GroupDetailsLayer.Frame.Bottom + sectionSpacing, View.Frame.Width, 44 );
            FirstName.Frame = new RectangleF( 0, FirstNameLayer.Frame.Top + 2, View.Frame.Width, 40 );
            FirstName.Text = CCVApp.Shared.Network.RockMobileUser.Instance.Person.NickName;

            LastNameLayer.Frame = new RectangleF( 0, FirstNameLayer.Frame.Bottom, View.Frame.Width, 44 );
            LastName.Frame = new RectangleF( 0, LastNameLayer.Frame.Top + 2, View.Frame.Width, 40 );
            LastName.Text = CCVApp.Shared.Network.RockMobileUser.Instance.Person.LastName;

            SpouseNameLayer.Frame = new RectangleF( 0, LastNameLayer.Frame.Bottom, View.Frame.Width, 44 );
            SpouseName.Frame = new RectangleF( 0, SpouseNameLayer.Frame.Top + 2, View.Frame.Width, 40 );

            // Contact Info
            EmailLayer.Frame = new RectangleF( 0, SpouseNameLayer.Frame.Bottom + sectionSpacing, View.Frame.Width, 44 );
            Email.Frame = new RectangleF( 0, EmailLayer.Frame.Top + 2, View.Frame.Width, 40 );
            Email.Text = CCVApp.Shared.Network.RockMobileUser.Instance.Person.Email;

            CellPhoneLayer.Frame = new RectangleF( 0, EmailLayer.Frame.Bottom, View.Frame.Width, 44 );
            CellPhone.Frame = new RectangleF( 0, CellPhoneLayer.Frame.Top + 2, View.Frame.Width, 40 );
            CellPhone.Text = CCVApp.Shared.Network.RockMobileUser.Instance.CellPhoneNumberDigits( );

            // Join Button
            JoinButton.Text = "Join Group";
            JoinButton.SizeToFit( );
            JoinButton.Frame = new RectangleF( (View.Frame.Width - 122) / 2, CellPhoneLayer.Frame.Bottom + sectionSpacing, 122, 44 );
        }

        void JoinGroup( PlatformButton button )
        {
        }
    }
}

