using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;
using System.IO;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Strings;

namespace iOS
{
	partial class LoginViewController : UIViewController
	{
        /// <summary>
        /// Reference to the parent springboard for returning apon completion
        /// </summary>
        /// <value>The springboard.</value>
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// Timer to allow a small delay before returning to the springboard after a successful login.
        /// </summary>
        /// <value>The login successful timer.</value>
        System.Timers.Timer LoginSuccessfulTimer { get; set; }

        BlockerView BlockerView { get; set; }

		public LoginViewController (IntPtr handle) : base (handle)
		{
            // setup our timer
            LoginSuccessfulTimer = new System.Timers.Timer();
            LoginSuccessfulTimer.AutoReset = false;
            LoginSuccessfulTimer.Interval = 2000;
		}

        protected enum LoginState
        {
            Out,
            Trying,

            // Deprecated state
            In
        };
        LoginState State { get; set; }

        UIImageView LogoView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new BlockerView( View.Frame );
            View.AddSubview( BlockerView );

            View.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );

            // Allow the return on username and password to start
            // the login process
            ControlStyling.StyleTextField( UsernameText, LoginStrings.UsernamePlaceholder );
            ControlStyling.StyleBGLayer( UsernameLayer );
            UsernameText.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryLogin();
                    return true;
                };

            ControlStyling.StyleTextField( PasswordText, LoginStrings.PasswordPlaceholder );
            ControlStyling.StyleBGLayer( PasswordLayer );
            PasswordText.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryLogin();
                    return true;
                };

            // obviously attempt a login if login is pressed
            ControlStyling.StyleButton( LoginButton, LoginStrings.LoginButton );
            LoginButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        RockMobileUser.Instance.Logout( );

                        SetUIState( LoginState.Out );
                    }
                    else
                    {
                        TryLogin();
                    }
                };

            ControlStyling.StyleButton( RegisterButton, LoginStrings.RegisterButton );

            // If cancel is pressed, notify the springboard we're done.
            CancelButton.SetTitleColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    Springboard.ResignModelViewController( this, null );
                };

            // setup the result
            ControlStyling.StyleUILabel( LoginResultLabel );
            ControlStyling.StyleBGLayer( LoginResultLayer );
            LoginResultLayer.Layer.Opacity = 0.00f;

            // setup the fake header
            HeaderView.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrimaryNavBarConfig.LogoFile;
            LogoView = new UIImageView( new UIImage( imagePath ) );
            HeaderView.AddSubview( LogoView );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderView.Layer.ShadowOffset = new System.Drawing.SizeF( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new System.Drawing.PointF( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // restore the buttons
            CancelButton.Hidden = false;
            LoginButton.Hidden = false;
            RegisterButton.Hidden = false;

            // if we're logged in, the UI should be slightly different
            if( RockMobileUser.Instance.LoggedIn )
            {
                // populate them with the user's info
                UsernameText.Text = RockMobileUser.Instance.Username;
                PasswordText.Text = RockMobileUser.Instance.Password;

                SetUIState( LoginState.In );
            }
            else
            {
                SetUIState( LoginState.Out );
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // if they tap somewhere outside of the text fields, 
            // hide the keyboard
            UsernameText.ResignFirstResponder( );
            PasswordText.ResignFirstResponder( );
        }

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations( )
        {
            return Springboard.GetSupportedInterfaceOrientations( );
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation( )
        {
            return Springboard.PreferredInterfaceOrientationForPresentation( );
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public void TryLogin()
        {
            // if both fields are valid, attempt a login!
            if( string.IsNullOrEmpty( UsernameText.Text ) == false &&
                string.IsNullOrEmpty( PasswordText.Text ) == false )
            {
                SetUIState( LoginState.Trying );

                RockMobileUser.Instance.Login( UsernameText.Text, PasswordText.Text, LoginComplete );
            }
        }

        protected void SetUIState( LoginState state )
        {
            // reset the result label
            LoginResultLabel.Text = "";

            switch( state )
            {
                case LoginState.Out:
                {
                    UsernameText.Text = "";
                    PasswordText.Text = "";

                    UsernameText.Enabled = true;
                    PasswordText.Enabled = true;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Hidden = false;
                    RegisterButton.Enabled = true;

                    LoginButton.SetTitle( LoginStrings.LoginButton, UIControlState.Normal );

                    break;
                }

                case LoginState.Trying:
                {
                    FadeLoginResult( false );
                    BlockerView.FadeIn( null );

                    UsernameText.Enabled = false;
                    PasswordText.Enabled = false;
                    LoginButton.Enabled = false;
                    CancelButton.Enabled = false;
                    RegisterButton.Enabled = false;

                    LoginButton.SetTitle( LoginStrings.LoginButton, UIControlState.Normal );

                    break;
                }

                // Deprecated state
                case LoginState.In:
                {
                    UsernameText.Enabled = false;
                    PasswordText.Enabled = false;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Hidden = true;
                    RegisterButton.Enabled = false;

                    LoginButton.SetTitle( "Logout", UIControlState.Normal );

                    break;
                }
            }

            State = state;
        }

        public void LoginComplete( System.Net.HttpStatusCode statusCode, string statusDescription )
        {
            switch ( statusCode )
            {
                // if we received No Content, we're logged in
                case System.Net.HttpStatusCode.NoContent:
                {
                    RockMobileUser.Instance.GetProfile( ProfileComplete );
                    break;
                }

                case System.Net.HttpStatusCode.Unauthorized:
                {
                    BlockerView.FadeOut( delegate
                        {
                            // wrong user name / password
                            FadeLoginResult( true );

                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            LoginResultLabel.Text = LoginStrings.Error_Credentials;
                        } );
                    break;
                }

                default:
                {
                    BlockerView.FadeOut( delegate
                        {
                            // failed to login for some reason
                            FadeLoginResult( true );

                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            LoginResultLabel.Text = LoginStrings.Error_Unknown;
                        } );
                    break;
                }
            }
        }

        public void ProfileComplete(System.Net.HttpStatusCode code, string desc, Rock.Client.Person model) 
        {
            BlockerView.FadeOut( delegate
                {
                    switch ( code )
                    {
                        case System.Net.HttpStatusCode.OK:
                        {
                            // if they have a profile picture, grab it.
                            if ( model.PhotoId != null )
                            {
                                RockMobileUser.Instance.DownloadProfilePicture( GeneralConfig.ProfileImageSize, ProfileImageComplete );
                            }

                            // update the UI
                            FadeLoginResult( true );
                            LoginResultLabel.Text = string.Format( LoginStrings.Success, model.FirstName );

                            // start the timer, which will notify the springboard we're logged in when it ticks.
                            LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                            {
                                // when the timer fires, notify the springboard we're done.
                                Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                                    {
                                        Springboard.ResignModelViewController( this, null );
                                    } );
                            };

                            LoginSuccessfulTimer.Start( );

                            break;
                        }

                        default:
                        {
                            // failed to login for some reason
                            FadeLoginResult( true );

                            // if we couldn't get their profile, that should still count as a failed login.
                            SetUIState( LoginState.Out );

                            RockMobileUser.Instance.Logout( );

                            LoginResultLabel.Text = LoginStrings.Error_Unknown;
                            break;
                        }
                    }
                } );
        }

        public void ProfileImageComplete( System.Net.HttpStatusCode code, string desc )
        {
            switch( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // sweet! make the UI update.
                    Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { Springboard.UpdateProfilePic( ); } );
                    break;
                }

                default:
                {
                    // bummer, we couldn't get their profile picture. Doesn't really matter...
                    break;
                }
            }
        }

        void FadeLoginResult( bool fadeIn )
        {
            UIView.Animate( .33f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new NSAction( 
                    delegate 
                    { 
                        LoginResultLayer.Layer.Opacity = fadeIn == true ? 1.00f : 0.00f;
                    })

                , new NSAction(
                    delegate
                    {
                    })
            );
        }
	}
}
