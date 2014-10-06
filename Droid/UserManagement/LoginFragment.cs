
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
        Button CreateAccountButton { get; set; }
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

            LoginActivityIndicator = view.FindViewById<ProgressBar>( Resource.Id.login_progressBar );
            LoginActivityIndicator.Visibility = ViewStates.Gone;

            LoginButton = view.FindViewById<Button>( Resource.Id.login_loginButton );
            CancelButton = view.FindViewById<Button>( Resource.Id.login_cancelButton );
            CreateAccountButton = view.FindViewById<Button>( Resource.Id.login_needAccountButton );

            UsernameField = view.FindViewById<EditText>( Resource.Id.login_usernameText );
            PasswordField = view.FindViewById<EditText>( Resource.Id.login_passwordText );
            LoginResultLabel = view.FindViewById<TextView>( Resource.Id.login_loginResultLabel );

            LoginButton.Click += (object sender, EventArgs e) => 
                {
                    TryLogin( );
                };

            CancelButton.Click += (object sender, EventArgs e) => 
                {
                    SpringboardParent.ModalFragmentFinished( this );
                };

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            SetUIState( LoginState.Out );
        }

        protected void TryLogin()
        {
            // if both fields are valid, attempt a login!
            if( string.IsNullOrEmpty( UsernameField.Text ) == false &&
                string.IsNullOrEmpty( PasswordField.Text ) == false )
            {
                SetUIState( LoginState.Trying );

                RockMobileUser.Instance.Login( UsernameField.Text, PasswordField.Text, LoginComplete );
            }
        }

        public void LoginComplete( System.Net.HttpStatusCode statusCode, string statusDescription )
        {
            switch( statusCode )
            {
                // if we received No Content, we're logged in
                case System.Net.HttpStatusCode.NoContent:
                {
                    RockMobileUser.Instance.GetProfile( ProfileComplete );
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
            switch( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // hide the activity indicator, because we are now logged in,
                    // but leave the buttons all disabled.
                    LoginActivityIndicator.Visibility = ViewStates.Gone;
                    CancelButton.Visibility = ViewStates.Invisible;
                    LoginButton.Visibility = ViewStates.Invisible;
                    CreateAccountButton.Visibility = ViewStates.Invisible;

                    // update the UI
                    LoginResultLabel.Text = string.Format( "Welcome back, {0}!", model.FirstName );

                    // start the timer, which will notify the springboard we're logged in when it ticks.
                    LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                        {
                            // when the timer fires, notify the springboard we're done.
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { SpringboardParent.ModalFragmentFinished( this ); } );
                        };

                    LoginSuccessfulTimer.Start();

                    break;
                }

                default:
                {
                    // if we couldn't get their profile, that should still count as a failed login.
                    SetUIState( LoginState.Out );

                    RockMobileUser.Instance.Logout( );

                    LoginResultLabel.Text = "Unable to Login. Try Again";
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
                    CreateAccountButton.Visibility = ViewStates.Visible;
                    CreateAccountButton.Enabled = true;

                    break;
                }

                case LoginState.Trying:
                {
                    LoginActivityIndicator.Visibility = ViewStates.Visible;
                    UsernameField.Enabled = false;
                    PasswordField.Enabled = false;
                    LoginButton.Enabled = false;
                    CancelButton.Enabled = false;
                    CreateAccountButton.Enabled = false;

                    break;
                }
            }

            State = state;
        }

    }
}

