﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CCVApp.Shared.Network;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using Android.Webkit;
using Rock.Mobile.Threading;

namespace Droid
{
    public class LoginFragment : Fragment
    {
        /// <summary>
        /// Timer to allow a small delay before returning to the springboard after a successful login.
        /// </summary>
        /// <value>The login successful timer.</value>
        System.Timers.Timer LoginSuccessfulTimer { get; set; }

        protected enum LoginState
        {
            Out,
            Trying,
        };
        LoginState State { get; set; }

        public Springboard SpringboardParent { get; set; }

        ProgressBar LoginActivityIndicator { get; set; }
        Button LoginButton { get; set; }
        Button CancelButton { get; set; }
        Button RegisterButton { get; set; }
        ImageButton FacebookButton { get; set; }
        EditText UsernameField { get; set; }
        EditText PasswordField { get; set; }
        TextView LoginResultLabel { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // setup our timer
            LoginSuccessfulTimer = new System.Timers.Timer();
            LoginSuccessfulTimer.AutoReset = false;
            LoginSuccessfulTimer.Interval = 1000;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            View view = inflater.Inflate(Resource.Layout.Login, container, false);
            view.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            RelativeLayout navBar = view.FindViewById<RelativeLayout>( Resource.Id.navbar_relative_layout );
            navBar.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            View backgroundView = view.FindViewById<View>( Resource.Id.login_background );
            ControlStyling.StyleBGLayer( backgroundView );

            LoginActivityIndicator = view.FindViewById<ProgressBar>( Resource.Id.login_progressBar );
            LoginActivityIndicator.Visibility = ViewStates.Gone;

            LoginButton = view.FindViewById<Button>( Resource.Id.loginButton );
            ControlStyling.StyleButton( LoginButton, LoginStrings.LoginButton );
            LoginButton.Click += (object sender, EventArgs e) => 
                {
                    TryRockBind( );
                };

            CancelButton = view.FindViewById<Button>( Resource.Id.cancelButton );
            ControlStyling.StyleButton( CancelButton, GeneralStrings.Cancel );
            CancelButton.SetBackgroundDrawable( null );
            CancelButton.Click += (object sender, EventArgs e) => 
                {
                    SpringboardParent.ModalFragmentFinished( this, null );
                };

            RegisterButton = view.FindViewById<Button>( Resource.Id.registerButton );
            ControlStyling.StyleButton( RegisterButton, LoginStrings.RegisterButton );

            UsernameField = view.FindViewById<EditText>( Resource.Id.usernameText );
            ControlStyling.StyleTextField( UsernameField, LoginStrings.UsernamePlaceholder );

            View borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            PasswordField = view.FindViewById<EditText>( Resource.Id.passwordText );
            ControlStyling.StyleTextField( PasswordField, LoginStrings.PasswordPlaceholder );

            LoginResultLabel = view.FindViewById<TextView>( Resource.Id.loginResult );
            ControlStyling.StyleUILabel( LoginResultLabel );

            // Setup the facebook button
            FacebookButton = view.FindViewById<ImageButton>( Resource.Id.facebookButton );
            FacebookButton.SetBackgroundDrawable( null );
            FacebookButton.Click += (object sender, EventArgs e ) =>
            {
                TryFacebookBind( );
            };

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            SetUIState( LoginState.Out );
        }

        protected void TryRockBind()
        {
            // if both fields are valid, attempt a login!
            if( string.IsNullOrEmpty( UsernameField.Text ) == false &&
                string.IsNullOrEmpty( PasswordField.Text ) == false )
            {
                SetUIState( LoginState.Trying );

                RockMobileUser.Instance.BindRockAccount( UsernameField.Text, PasswordField.Text, BindComplete );
            }
        }

        class FBWebViewClient : WebViewClient
        {
            public LoginFragment Parent { get; set; }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);

                Parent.OnPageFinished( view, url );
            }
        }

        Facebook.FacebookClient Session { get; set; }
        WebView WebView { get; set; }
        public void TryFacebookBind( )
        {
            SetUIState( LoginState.Trying );

            RockMobileUser.Instance.BindFacebookAccount( delegate(string fromUri, Facebook.FacebookClient session) 
                {
                    Session = session;

                    // invoke a webview
                    WebView = new WebView( Rock.Mobile.PlatformCommon.Droid.Context );
                    WebView.SetWebViewClient( new FBWebViewClient( ) { Parent = this } );
                    WebView.Settings.JavaScriptEnabled = true;
                    WebView.Settings.SetSupportZoom(true);
                    WebView.Settings.BuiltInZoomControls = true;
                    WebView.Settings.LoadWithOverviewMode = true; //Load 100% zoomed out
                    WebView.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
                    WebView.ScrollbarFadingEnabled = true;

                    WebView.VerticalScrollBarEnabled = true;
                    WebView.HorizontalScrollBarEnabled = true;

                    (View as LinearLayout).AddView( WebView, 0 );
                    WebView.LoadUrl( fromUri );
                });
        }

        public void OnPageFinished( WebView view, string url )
        {
            if ( RockMobileUser.Instance.HasFacebookResponse( url, Session ) )
            {
                ( View as LinearLayout ).RemoveView( WebView );

                RockMobileUser.Instance.FacebookCredentialResult( url, Session, BindComplete );
            }
        }

        public void BindComplete( bool success )
        {
            if ( success )
            {// However we chose to bind, we can now login with the bound account
                RockMobileUser.Instance.Login( LoginComplete );
            }
            else
            {
                LoginComplete( System.Net.HttpStatusCode.BadRequest, "" );
            }
        }

        public void LoginComplete( System.Net.HttpStatusCode statusCode, string statusDescription )
        {
            switch( statusCode )
            {
                // if we received No Content, we're logged in
                case System.Net.HttpStatusCode.NoContent:
                {
                    // hack: Until facebook is fully implemented in rock, don't attempt syncing the profile cause it won't exist.
                    if ( RockMobileUser.Instance.AccountType == RockMobileUser.BoundAccountType.Facebook )
                    {
                        ProfileComplete( System.Net.HttpStatusCode.OK, "", RockMobileUser.Instance.Person );
                    }
                    else
                    {
                        RockMobileUser.Instance.GetProfile( ProfileComplete );
                    }
                    break;
                }

                case System.Net.HttpStatusCode.Unauthorized:
                {
                    // wrong user name / password

                    // allow them to attempt logging in again
                    SetUIState( LoginState.Out );

                    LoginResultLabel.Text = "Invalid Username or Password";
                    break;
                }

                default:
                {
                    // failed to login for some reason

                    // allow them to attempt logging in again
                    SetUIState( LoginState.Out );

                    LoginResultLabel.Text = "Unable to Login. Try Again";
                    break;
                }
            }
        }

        public void ProfileComplete(System.Net.HttpStatusCode code, string desc, Rock.Client.Person model)
        {
            UIThreading.PerformOnUIThread( delegate
                {
                    UIThread_ProfileComplete( code, desc, model );
                } );
        }

        void UIThread_ProfileComplete(System.Net.HttpStatusCode code, string desc, Rock.Client.Person model)
        {
            switch( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // if they have a profile picture, grab it.
                    RockMobileUser.Instance.TryDownloadProfilePicture( GeneralConfig.ProfileImageSize, ProfileImageComplete );

                    // hide the activity indicator, because we are now logged in,
                    // but leave the buttons all disabled.
                    LoginActivityIndicator.Visibility = ViewStates.Gone;
                    CancelButton.Visibility = ViewStates.Invisible;
                    LoginButton.Visibility = ViewStates.Invisible;
                    RegisterButton.Visibility = ViewStates.Invisible;

                    // update the UI
                    LoginResultLabel.Text = string.Format( "Welcome back, {0}!", model.FirstName );

                    // start the timer, which will notify the springboard we're logged in when it ticks.
                    LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                        {
                            // when the timer fires, notify the springboard we're done.
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { SpringboardParent.ModalFragmentFinished( this, null ); } );
                        };

                    LoginSuccessfulTimer.Start();

                    break;
                }

                default:
                {
                    // if we couldn't get their profile, that should still count as a failed login.
                    SetUIState( LoginState.Out );

                    RockMobileUser.Instance.LogoutAndUnbind( );

                    LoginResultLabel.Text = "Unable to Login. Try Again";
                    break;
                }
            }
        }

        public void ProfileImageComplete( System.Net.HttpStatusCode code, string desc )
        {
            switch( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // sweet! make the UI update.
                    Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { SpringboardParent.SetProfileImage( ); } );
                    break;
                }

                default:
                {
                    // bummer, we couldn't get their profile picture. Doesn't really matter...
                    break;
                }
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
                    UsernameField.Text = "";
                    PasswordField.Text = "";

                    LoginActivityIndicator.Visibility = ViewStates.Gone;
                    UsernameField.Enabled = true;
                    PasswordField.Enabled = true;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Visibility = ViewStates.Visible;
                    RegisterButton.Enabled = true;

                    break;
                }

                case LoginState.Trying:
                {
                    LoginActivityIndicator.Visibility = ViewStates.Visible;
                    UsernameField.Enabled = false;
                    PasswordField.Enabled = false;
                    LoginButton.Enabled = false;
                    CancelButton.Enabled = false;
                    RegisterButton.Enabled = false;

                    break;
                }
            }

            State = state;
        }

    }
}

