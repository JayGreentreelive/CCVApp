﻿using System;
using Rock.Mobile.PlatformUI;
using System.Drawing;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.Animation;
using CCVApp.Shared.Network;
using Rock.Mobile.Util.Strings;

namespace CCVApp.Shared.UI
{
    public class UIJoinGroup
    {
        public PlatformView View { get; set; }

        int GroupID { get; set; }

        PlatformLabel GroupTitle { get; set; }

        PlatformLabel GroupDetails { get; set; }
        PlatformView GroupDetailsLayer { get; set; }

        PlatformTextField FirstName { get; set; }
        PlatformView FirstNameLayer { get; set; }

        PlatformTextField LastName { get; set; }
        PlatformView LastNameLayer { get; set; }

        PlatformTextField SpouseName { get; set; }
        PlatformView SpouseNameLayer { get; set; }

        PlatformTextField Email { get; set; }
        PlatformView EmailLayer { get; set; }

        // Make CellPhone public so we can attach a platform specific delegate
        public PlatformTextField CellPhone { get; set; }
        PlatformView CellPhoneLayer { get; set; }

        PlatformButton JoinButton { get; set; }

        UIResultView ResultView { get; set; }

        UIBlockerView BlockerView { get; set; }

        public UIJoinGroup( )
        {
        }

        public void Create( object masterView, RectangleF frame )
        {
            View = PlatformView.Create( );
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

            FirstName = PlatformTextField.Create( );
            FirstName.AddAsSubview( masterView );
            FirstName.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            FirstName.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            FirstName.Placeholder = ConnectStrings.JoinGroup_FirstNamePlaceholder;
            FirstName.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            FirstName.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            FirstName.AutoCapitalizationType = AutoCapitalizationType.Words;
            FirstName.AutoCorrectionType = AutoCorrectionType.No;


            LastNameLayer = PlatformView.Create( );
            LastNameLayer.AddAsSubview( masterView );
            LastNameLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            LastNameLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            LastNameLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            LastName = PlatformTextField.Create( );
            LastName.AddAsSubview( masterView );
            LastName.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            LastName.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            LastName.Placeholder = ConnectStrings.JoinGroup_LastNamePlaceholder;
            LastName.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            LastName.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            LastName.AutoCapitalizationType = AutoCapitalizationType.Words;
            LastName.AutoCorrectionType = AutoCorrectionType.No;

            SpouseNameLayer = PlatformView.Create( );
            SpouseNameLayer.AddAsSubview( masterView );
            SpouseNameLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            SpouseNameLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            SpouseNameLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            SpouseName = PlatformTextField.Create( );
            SpouseName.AddAsSubview( masterView );
            SpouseName.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            SpouseName.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            SpouseName.Placeholder = ConnectStrings.JoinGroup_SpouseNamePlaceholder;
            SpouseName.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            SpouseName.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            SpouseName.AutoCapitalizationType = AutoCapitalizationType.Words;
            SpouseName.AutoCorrectionType = AutoCorrectionType.No;

            // Contact Info
            EmailLayer = PlatformView.Create( );
            EmailLayer.AddAsSubview( masterView );
            EmailLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            EmailLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            EmailLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            Email = PlatformTextField.Create( );
            Email.AddAsSubview( masterView );
            Email.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            Email.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            Email.Placeholder = ConnectStrings.JoinGroup_EmailPlaceholder;
            Email.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            Email.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            Email.AutoCapitalizationType = AutoCapitalizationType.None;
            Email.AutoCorrectionType = AutoCorrectionType.No;

            CellPhoneLayer = PlatformView.Create( );
            CellPhoneLayer.AddAsSubview( masterView );
            CellPhoneLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            CellPhoneLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            CellPhoneLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            CellPhone = PlatformTextField.Create( );
            CellPhone.AddAsSubview( masterView );
            CellPhone.SetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            CellPhone.PlaceholderTextColor = ControlStylingConfig.TextField_PlaceholderTextColor;
            CellPhone.Placeholder = ConnectStrings.JoinGroup_CellPhonePlaceholder;
            CellPhone.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            CellPhone.KeyboardAppearance = KeyboardAppearanceStyle.Dark;
            CellPhone.AutoCapitalizationType = AutoCapitalizationType.None;
            CellPhone.AutoCorrectionType = AutoCorrectionType.No;


            // Join Button
            JoinButton = PlatformButton.Create( );
            JoinButton.AddAsSubview( masterView );
            JoinButton.ClickEvent = JoinClicked;
            JoinButton.BackgroundColor = ControlStylingConfig.Button_BGColor;
            JoinButton.TextColor = ControlStylingConfig.Button_TextColor;
            JoinButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            JoinButton.Text = ConnectStrings.JoinGroup_JoinButtonLabel;
            JoinButton.SizeToFit( );
            JoinButton.UserInteractionEnabled = true;

            // Create our results view overlay
            ResultView = new UIResultView( masterView, View.Frame, OnResultViewDone );

            ResultView.SetStyle( ControlStylingConfig.Medium_Font_Light, 
                ControlStylingConfig.Icon_Font_Secondary, 
                ControlStylingConfig.BackgroundColor,
                ControlStylingConfig.BG_Layer_Color, 
                ControlStylingConfig.BG_Layer_BorderColor, 
                ControlStylingConfig.TextField_PlaceholderTextColor,
                ControlStylingConfig.Button_BGColor, 
                ControlStylingConfig.Button_TextColor );

            // Create our blocker view
            BlockerView = new UIBlockerView( masterView, View.Frame );
        }

        public float GetControlBottom( )
        {
            return JoinButton.Frame.Bottom;
        }

        void OnResultViewDone( )
        {
            //hack - Can't figure out WHY the join button isn't in the proper Z order on the Nexus 7.
            // but I just don't care right now. So hide it and unhide it.
            JoinButton.Hidden = false;

            ResultView.Hide( );
        }

        public void DisplayView( string groupTitle, string distance, string meetingTime, int groupId )
        {
            // store the group ID as we'll need it if they hit submit
            GroupID = groupId;

            // set the group title
            GroupTitle.Text = groupTitle;


            // set the details for the group (distance, meeting time, etc)
            GroupDetails.Text = meetingTime + "\n" + distance;
            GroupDetails.TextAlignment = TextAlignment.Center;


            FirstName.Text = CCVApp.Shared.Network.RockMobileUser.Instance.Person.NickName;
            LastName.Text = CCVApp.Shared.Network.RockMobileUser.Instance.Person.LastName;


            Email.Text = CCVApp.Shared.Network.RockMobileUser.Instance.Person.Email;
            CellPhone.Text = CCVApp.Shared.Network.RockMobileUser.Instance.CellPhoneNumberDigits( );

            ResultView.Hide( );
        }

        public void LayoutChanged( RectangleF containerBounds )
        {
            View.Frame = new RectangleF( containerBounds.Left, containerBounds.Top, containerBounds.Width, containerBounds.Height );

            BlockerView.SetBounds( containerBounds );
            ResultView.SetBounds( containerBounds );

            float sectionSpacing = Rock.Mobile.Graphics.Util.UnitToPx( 25 );
            float layerHeight = Rock.Mobile.Graphics.Util.UnitToPx( 44 );
            float textFieldHeight = Rock.Mobile.Graphics.Util.UnitToPx( 40 );
            float textLeftInset = Rock.Mobile.Graphics.Util.UnitToPx( 5 );
            float textTopInset = Rock.Mobile.Graphics.Util.UnitToPx( 2 );

            float buttonWidth = Rock.Mobile.Graphics.Util.UnitToPx( 122 );

            GroupTitle.Frame = new RectangleF( 0, 0, View.Frame.Width, Rock.Mobile.Graphics.Util.UnitToPx( 50 ) );

            GroupDetailsLayer.Frame = new RectangleF( 0, GroupTitle.Frame.Bottom, View.Frame.Width, Rock.Mobile.Graphics.Util.UnitToPx( 62 ) );
            GroupDetails.Frame = new RectangleF( 0, GroupDetailsLayer.Frame.Top + 2, View.Frame.Width, Rock.Mobile.Graphics.Util.UnitToPx( 60 ) );

            // Name Info
            FirstNameLayer.Frame = new RectangleF( 0, GroupDetailsLayer.Frame.Bottom + sectionSpacing, View.Frame.Width, layerHeight );
            FirstName.Frame = new RectangleF( textLeftInset, FirstNameLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            LastNameLayer.Frame = new RectangleF( 0, FirstNameLayer.Frame.Bottom, View.Frame.Width, layerHeight );
            LastName.Frame = new RectangleF( textLeftInset, LastNameLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            SpouseNameLayer.Frame = new RectangleF( 0, LastNameLayer.Frame.Bottom, View.Frame.Width, layerHeight );
            SpouseName.Frame = new RectangleF( textLeftInset, SpouseNameLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            // Contact Info
            EmailLayer.Frame = new RectangleF( 0, SpouseNameLayer.Frame.Bottom + sectionSpacing, View.Frame.Width, layerHeight );
            Email.Frame = new RectangleF( textLeftInset, EmailLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            CellPhoneLayer.Frame = new RectangleF( 0, EmailLayer.Frame.Bottom, View.Frame.Width, layerHeight );
            CellPhone.Frame = new RectangleF( textLeftInset, CellPhoneLayer.Frame.Top + textTopInset, View.Frame.Width, textFieldHeight );

            // Join Button
            JoinButton.Frame = new RectangleF( (View.Frame.Width - buttonWidth) / 2, CellPhoneLayer.Frame.Bottom + sectionSpacing, buttonWidth, layerHeight );
        }

        bool ValidateInput( )
        {
            bool result = true;

            // validate there's text in all required fields

            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( FirstName.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( FirstNameLayer, targetColor );


            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( LastName.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( LastNameLayer, targetColor );


            // cell phone OR email is fine
            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( Email.Text ) == true && string.IsNullOrEmpty( CellPhone.Text ) == true )
            {
                // if failure, only color email
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Util.AnimateBackgroundColor( EmailLayer, targetColor );

            return result;
        }

        public void TouchesEnded( )
        {
            FirstName.ResignFirstResponder( );
            LastName.ResignFirstResponder( );
            SpouseName.ResignFirstResponder( );
            Email.ResignFirstResponder( );
            CellPhone.ResignFirstResponder( );
        }

        void JoinClicked( PlatformButton button )
        {
            if ( ValidateInput( ) )
            {
                //hack - Can't figure out WHY the join button isn't in the proper Z order on the Nexus 7.
                // but I just don't care right now. So hide it and unhide it.
                JoinButton.Hidden = true;

                BlockerView.Show( );

                // default to no alias ID, but if they're logged in and have one, set it
                int personAliasId = 0;
                if ( CCVApp.Shared.Network.RockMobileUser.Instance.LoggedIn == true && CCVApp.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId.HasValue )
                {
                    personAliasId = CCVApp.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId.Value;
                }

                RockApi.Instance.JoinGroup( personAliasId, FirstName.Text, LastName.Text, SpouseName.Text, Email.Text, CellPhone.Text.AsNumeric( ), GroupID, GroupTitle.Text,
                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                    {
                        BlockerView.Hide( );

                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                        {
                            ResultView.Show( RegisterStrings.RegisterStatus_Success, 
                                ControlStylingConfig.Result_Symbol_Success, 
                                string.Format( ConnectStrings.JoinGroup_RegisterSuccess, GroupTitle.Text ),
                                GeneralStrings.Done );
                        }
                        else
                        {
                            ResultView.Show( RegisterStrings.RegisterStatus_Failed, 
                                ControlStylingConfig.Result_Symbol_Failed, 
                                string.Format( ConnectStrings.JoinGroup_RegisterFailed, GroupTitle.Text ),
                                GeneralStrings.Done );
                        }
                    } );
            }
        }
    }
}

