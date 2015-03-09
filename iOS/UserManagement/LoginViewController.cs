using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;
using System.IO;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Strings;
using CCVApp.Shared;
using Rock.Mobile.Threading;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using System.Collections.Generic;
using Rock.Mobile.Animation;

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

        Rock.Mobile.PlatformSpecific.iOS.UI.BlockerView BlockerView { get; set; }

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

        UIImageView FBImageView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new Rock.Mobile.PlatformSpecific.iOS.UI.BlockerView( View.Frame );
            View.AddSubview( BlockerView );

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // Allow the return on username and password to start
            // the login process
            ControlStyling.StyleTextField( UsernameText, LoginStrings.UsernamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( UsernameLayer );
            UsernameText.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryRockBind();
                    return true;
                };

            ControlStyling.StyleTextField( PasswordText, LoginStrings.PasswordPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( PasswordLayer );
            PasswordText.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryRockBind();
                    return true;
                };

            // obviously attempt a login if login is pressed
            ControlStyling.StyleButton( LoginButton, LoginStrings.LoginButton, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            LoginButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        RockMobileUser.Instance.LogoutAndUnbind( );

                        SetUIState( LoginState.Out );
                    }
                    else
                    {
                        TryRockBind();
                    }
                };

            ControlStyling.StyleButton( RegisterButton, LoginStrings.RegisterButton, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            // setup the facebook button
            string imagePath = NSBundle.MainBundle.BundlePath + "/" + "facebook_login.png";
            FBImageView = new UIImageView( new UIImage( imagePath ) );

            FacebookLogin.SetTitle( "", UIControlState.Normal );
            FacebookLogin.AddSubview( FBImageView );

            FacebookLogin.TouchUpInside += (object sender, EventArgs e) => 
                {
                    TryFacebookBind();
                };

            // If cancel is pressed, notify the springboard we're done.
            CancelButton.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow canceling while we wait for a web request.
                    if( LoginState.Trying != State )
                    {
                        Springboard.ResignModelViewController( this, null );
                    }
                };

            // setup the result
            ControlStyling.StyleUILabel( LoginResultLabel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( LoginResultLayer );
            LoginResultLayer.Layer.Opacity = 0.00f;

            // setup the fake header
            HeaderView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            imagePath = NSBundle.MainBundle.BundlePath + "/" + PrimaryNavBarConfig.LogoFile;
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
            HeaderView.Layer.ShadowOffset = new CoreGraphics.CGSize( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new CoreGraphics.CGPoint( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );
            FBImageView.Layer.Position = new CoreGraphics.CGPoint( FacebookLogin.Bounds.Width / 2, FacebookLogin.Bounds.Height / 2 );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            LoginResultLayer.Layer.Opacity = 0.00f;

            // clear these only on the appearance of the view. (As opposed to also 
            // when the state becomes LogOut.) This way, if they do something like mess
            // up their password, it won't force them to retype it all in.
            UsernameText.Text = "";
            PasswordText.Text = "";
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
                UsernameText.Text = RockMobileUser.Instance.UserID;
                PasswordText.Text = RockMobileUser.Instance.RockPassword;

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

        public void TryRockBind()
        {
            // if both fields are valid, attempt a login!
            if( string.IsNullOrEmpty( UsernameText.Text ) == false &&
                string.IsNullOrEmpty( PasswordText.Text ) == false )
            {
                SetUIState( LoginState.Trying );

                RockMobileUser.Instance.BindRockAccount( UsernameText.Text, PasswordText.Text, BindComplete );
            }
        }

        public void TryFacebookBind( )
        {
            SetUIState( LoginState.Trying );

            // have our rock mobile user begin the facebook bind process
            RockMobileUser.Instance.BindFacebookAccount( delegate(string fromUri, Facebook.FacebookClient session) 
            {
                    // it's ready, so create a webView that will take them to the FBLogin page
                    WebLayout webLayout = new WebLayout( View.Frame );
                    webLayout.DeleteCacheandCookies( );

                    View.AddSubview( webLayout.ContainerView );

                    // set it totally transparent so we can fade it in
                    webLayout.ContainerView.BackgroundColor = UIColor.Black;
                    webLayout.ContainerView.Layer.Opacity = 0.00f;
                    webLayout.SetCancelButtonColor( ControlStylingConfig.TextField_PlaceholderTextColor );

                    // do a nice fade-in
                    SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( 0.00f, 1.00f, .25f, 
                        delegate(float percent, object value) 
                        {
                            webLayout.ContainerView.Layer.Opacity = (float)value;
                        },
                        delegate 
                        {
                            // once faded in, begin loading the page
                            webLayout.ContainerView.Layer.Opacity = 1.00f;

                            webLayout.LoadUrl( fromUri, delegate(WebLayout.Result result, string url) 
                                {
                                    // if fail/success comes in
                                    if( result != WebLayout.Result.Cancel )
                                    {
                                        // see if it's a valid facebook response
                                        if ( RockMobileUser.Instance.HasFacebookResponse( url, session ) )
                                        {
                                            // it is, so remove the webview and continue the bind process
                                            webLayout.ContainerView.RemoveFromSuperview( );
                                            RockMobileUser.Instance.FacebookCredentialResult( url, session, BindComplete );
                                        }
                                    }
                                    else
                                    {
                                        // they pressed cancel, so simply cancel the attempt
                                        webLayout.ContainerView.RemoveFromSuperview( );
                                        LoginComplete( System.Net.HttpStatusCode.ResetContent, "" );
                                    }
                                } );
                        });

                    floatAnimator.Start( );
            });
        }

        public void BindComplete( bool success )
        {
            if ( success )
            {
                // However we chose to bind, we can now login with the bound account
                RockMobileUser.Instance.Login( LoginComplete );
            }
            else
            {
                LoginComplete( System.Net.HttpStatusCode.BadRequest, "" );
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
                    RockMobileUser.Instance.GetProfileAndCellPhone( ProfileComplete );
                    break;
                }

                case System.Net.HttpStatusCode.Unauthorized:
                {
                    BlockerView.FadeOut( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            // wrong user name / password
                            FadeLoginResult( true );
                            LoginResultLabel.Text = LoginStrings.Error_Credentials;
                        } );
                    break;
                }

                case System.Net.HttpStatusCode.ResetContent:
                {
                    // consider this a cancellation
                    BlockerView.FadeOut( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            LoginResultLabel.Text = "";
                        } );

                    break;
                }

                default:
                {
                    BlockerView.FadeOut( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResultLabel.Text = LoginStrings.Error_Unknown;
                        } );
                    break;
                }
            }
        }

        public void ProfileComplete(System.Net.HttpStatusCode code, string desc, Rock.Client.Person model) 
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIThread_ProfileComplete( code, desc, model );
                } );
        }

        void UIThread_ProfileComplete( System.Net.HttpStatusCode code, string desc, Rock.Client.Person model ) 
        {
            BlockerView.FadeOut( delegate
                {
                    switch ( code )
                    {
                        case System.Net.HttpStatusCode.OK:
                        {
                            // get their address
                            RockMobileUser.Instance.GetFamilyAndAddress( AddressComplete );

                            break;
                        }

                        default:
                        {
                            // if we couldn't get their profile, that should still count as a failed login.
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResultLabel.Text = LoginStrings.Error_Unknown;

                            RockMobileUser.Instance.LogoutAndUnbind( );
                            break;
                        }
                    }
                } );
        }

        public void AddressComplete( System.Net.HttpStatusCode code, string desc, List<Rock.Client.Group> model )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIThread_AddressComplete( code, desc, model );
                } );
        }

        void UIThread_AddressComplete( System.Net.HttpStatusCode code, string desc, List<Rock.Client.Group> model ) 
        {
            BlockerView.FadeOut( delegate
                {
                    switch ( code )
                    {
                        case System.Net.HttpStatusCode.OK:
                        {
                            // if they have a profile picture, grab it.
                            RockMobileUser.Instance.TryDownloadProfilePicture( GeneralConfig.ProfileImageSize, ProfileImageComplete );

                            // update the UI
                            FadeLoginResult( true );
                            LoginResultLabel.Text = string.Format( LoginStrings.Success, RockMobileUser.Instance.PreferredName( ) );

                            // start the timer, which will notify the springboard we're logged in when it ticks.
                            LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                                {
                                    // when the timer fires, notify the springboard we're done.
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                        {
                                            Springboard.ResignModelViewController( this, null );
                                        } );
                                };

                            LoginSuccessfulTimer.Start( );

                            break;
                        }

                        default:
                        {
                            // if we couldn't get their profile, that should still count as a failed login.
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResultLabel.Text = LoginStrings.Error_Unknown;

                            RockMobileUser.Instance.LogoutAndUnbind( );
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
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { Springboard.UpdateProfilePic( ); } );
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
                new Action( 
                    delegate 
                    { 
                        LoginResultLayer.Layer.Opacity = fadeIn == true ? 1.00f : 0.00f;
                    })

                , new Action(
                    delegate
                    {
                    })
            );
        }
	}
}
