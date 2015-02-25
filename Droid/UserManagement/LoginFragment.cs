
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
using Rock.Mobile.PlatformSpecific.Android.Animation;
using Android.Views.InputMethods;
using Rock.Mobile.PlatformSpecific.Android.UI;

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

        View LoginResultLayer { get; set; }
        TextView LoginResultLabel { get; set; }

        WebLayout WebLayout { get; set; }

        Facebook.FacebookClient Session { get; set; }

        bool BindingFacebook { get; set; }

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
            view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            RelativeLayout navBar = view.FindViewById<RelativeLayout>( Resource.Id.navbar_relative_layout );
            navBar.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            View backgroundView = view.FindViewById<View>( Resource.Id.login_background );
            ControlStyling.StyleBGLayer( backgroundView );

            LoginActivityIndicator = view.FindViewById<ProgressBar>( Resource.Id.login_progressBar );
            LoginActivityIndicator.Visibility = ViewStates.Gone;

            LoginResultLayer = view.FindViewById<View>( Resource.Id.result_background );
            ControlStyling.StyleBGLayer( LoginResultLayer );
            LoginResultLayer.Alpha = 0.0f;

            LoginButton = view.FindViewById<Button>( Resource.Id.loginButton );
            ControlStyling.StyleButton( LoginButton, LoginStrings.LoginButton, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            LoginButton.Click += (object sender, EventArgs e) => 
                {
                    TryRockBind( );
                };

            CancelButton = view.FindViewById<Button>( Resource.Id.cancelButton );
            ControlStyling.StyleButton( CancelButton, GeneralStrings.Cancel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            CancelButton.SetBackgroundDrawable( null );
            CancelButton.Click += (object sender, EventArgs e) => 
                {
                    SpringboardParent.ModalFragmentDone( this, null );
                };

            RegisterButton = view.FindViewById<Button>( Resource.Id.registerButton );
            ControlStyling.StyleButton( RegisterButton, LoginStrings.RegisterButton, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            UsernameField = view.FindViewById<EditText>( Resource.Id.usernameText );
            ControlStyling.StyleTextField( UsernameField, LoginStrings.UsernamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

            View borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            PasswordField = view.FindViewById<EditText>( Resource.Id.passwordText );
            ControlStyling.StyleTextField( PasswordField, LoginStrings.PasswordPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

            LoginResultLabel = view.FindViewById<TextView>( Resource.Id.loginResult );
            ControlStyling.StyleUILabel( LoginResultLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

            // Setup the facebook button
            FacebookButton = view.FindViewById<ImageButton>( Resource.Id.facebookButton );
            FacebookButton.SetBackgroundDrawable( null );
            FacebookButton.Click += (object sender, EventArgs e ) =>
            {
                TryFacebookBind( );
            };

            // invoke a webview
            WebLayout = new WebLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
            WebLayout.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
            WebLayout.SetBackgroundColor( Android.Graphics.Color.Black );

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            SetUIState( LoginState.Out );

            SpringboardParent.ModalFragmentOpened( this );

            // clear the input fields only on resuming. That way if they fail to
            // login because of something like a wrong password, they won't
            // have to retype everything in.
            UsernameField.Text = "";
            PasswordField.Text = "";
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

        public void TryFacebookBind( )
        {
            // if we aren't already trying to bind facebook
            if ( BindingFacebook == false )
            {
                // go for it.
                BindingFacebook = true;

                RockMobileUser.Instance.BindFacebookAccount( delegate(string fromUri, Facebook.FacebookClient session )
                    {
                        Session = session;

                        ( View as RelativeLayout ).AddView( WebLayout );

                        WebLayout.LoadUrl( fromUri, 
                            delegate( string url )
                            {
                                // either way, wait for a facebook response
                                if ( RockMobileUser.Instance.HasFacebookResponse( url, Session ) )
                                {
                                    BindingFacebook = false;

                                    SetUIState( LoginState.Trying );
                                    ( View as RelativeLayout ).RemoveView( WebLayout );
                                    RockMobileUser.Instance.FacebookCredentialResult( url, Session, BindComplete );
                                }
                            } );
                        //
                    } );
            }
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

        public void LoginComplete( System.Net.HttpStatusCode statusCode, string statusDescription )
        {
            switch( statusCode )
            {
                // if we received No Content, we're logged in
                case System.Net.HttpStatusCode.NoContent:
                {
                    RockMobileUser.Instance.GetProfileAndCellPhone( ProfileComplete );
                    break;
                }

                case System.Net.HttpStatusCode.Unauthorized:
                {
                    // allow them to attempt logging in again
                    SetUIState( LoginState.Out );

                    // wrong user name / password
                    FadeLoginResult( true );
                    LoginResultLabel.Text = LoginStrings.Error_Credentials;
                    break;
                }

                case System.Net.HttpStatusCode.ResetContent:
                {
                    // consider this a cancellation

                    // allow them to attempt logging in again
                    SetUIState( LoginState.Out );


                    LoginResultLabel.Text = "";
                    break;
                }

                default:
                {
                    // allow them to attempt logging in again
                    SetUIState( LoginState.Out );

                    // failed to login for some reason
                    FadeLoginResult( true );
                    LoginResultLabel.Text = LoginStrings.Error_Unknown;
                    break;
                }
            }
        }

        public override void OnStop()
        {
            base.OnStop();

            SpringboardParent.EnableBack = true;
            SpringboardParent.ModalFragmentClosed( this );

            // remove the webview if it was left open
            if ( WebLayout.Parent != null )
            {
                ( View as RelativeLayout ).RemoveView( WebLayout );
            }

            // we can safely flag facebook binding as false, because the callback will be ignored.
            BindingFacebook = false;
        }

        public void ProfileComplete(System.Net.HttpStatusCode code, string desc, Rock.Client.Person model)
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
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
                    RockMobileUser.Instance.GetFamilyAndAddress( AddressComplete );

                    break;
                }

                default:
                {
                    SetUIState( LoginState.Out );

                    // if we couldn't get their profile, that should still count as a failed login.
                    FadeLoginResult( true );
                    LoginResultLabel.Text = LoginStrings.Error_Unknown;

                    RockMobileUser.Instance.LogoutAndUnbind( );
                    break;
                }
            }
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
            switch ( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // if they have a profile picture, grab it.
                    RockMobileUser.Instance.TryDownloadProfilePicture( GeneralConfig.ProfileImageSize, ProfileImageComplete );

                    // hide the activity indicator, because we are now logged in,
                    // but leave the buttons all disabled.
                    LoginActivityIndicator.Visibility = ViewStates.Gone;

                    // update the UI
                    FadeLoginResult( true );
                    LoginResultLabel.Text = string.Format( LoginStrings.Success, RockMobileUser.Instance.PreferredName( ) );

                    // start the timer, which will notify the springboard we're logged in when it ticks.
                    LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                    {
                        // when the timer fires, notify the springboard we're done.
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                // now ok to go back again. (if this failed at any point, moving to the LogOut state
                                // will also re-enable the back button.
                                SpringboardParent.EnableBack = true;

                                SpringboardParent.ModalFragmentDone( this, null );
                            } );
                    };

                    LoginSuccessfulTimer.Start( );
                    break;
                }

                default:
                {
                    // if we couldn't get their profile, that should still count as a failed login.
                    SetUIState( LoginState.Out );

                    FadeLoginResult( true );
                    LoginResultLabel.Text = LoginStrings.Error_Unknown;

                    RockMobileUser.Instance.LogoutAndUnbind( );
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
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { SpringboardParent.SetProfileImage( ); } );
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
                    // allow back when logged out
                    SpringboardParent.EnableBack = true;

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
                    // while trying a login, don't allow back
                    SpringboardParent.EnableBack = false;

                    FadeLoginResult( false );

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

        void FadeLoginResult( bool fadeIn )
        {
            SimpleAnimator_Float fader = new SimpleAnimator_Float( LoginResultLayer.Alpha, fadeIn == true ? 1.00f : 0.00f, .33f, 
                delegate(float percent, object value )
                {
                    LoginResultLayer.Alpha = (float)value;
                },
                delegate
                {
                } );

            fader.Start( );
        }
    }
}
