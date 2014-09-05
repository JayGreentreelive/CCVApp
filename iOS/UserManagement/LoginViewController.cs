using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;

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

		public LoginViewController (IntPtr handle) : base (handle)
		{
            // setup our timer
            LoginSuccessfulTimer = new System.Timers.Timer();
            LoginSuccessfulTimer.AutoReset = false;
            LoginSuccessfulTimer.Interval = 1000;
		}

        protected enum LoginState
        {
            Out,
            Trying,
            In
        };
        LoginState State { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LoginActivityIndicator.Hidden = true;

            // Allow the return on username and password to start
            // the login process
            UsernameField.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryLogin();
                    return true;
                };

            PasswordField.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryLogin();
                    return true;
                };

            // obviously attempt a login if login is pressed
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

            // If cancel is pressed, notify the springboard we're done.
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    Springboard.ResignModelViewController( this );
                };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // restore the buttons
            CancelButton.Hidden = false;
            LoginButton.Hidden = false;
            CreateAccountButton.Hidden = false;

            // if we're logged in, the UI should be slightly different
            if( RockMobileUser.Instance.LoggedIn )
            {
                // populate them with the user's info
                UsernameField.Text = RockMobileUser.Instance.Username;
                PasswordField.Text = RockMobileUser.Instance.Password;

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
            UsernameField.ResignFirstResponder( );
            PasswordField.ResignFirstResponder( );
        }

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public void TryLogin()
        {
            // if both fields are valid, attempt a login!
            if( string.IsNullOrEmpty( UsernameField.Text ) == false &&
                string.IsNullOrEmpty( PasswordField.Text ) == false )
            {
                SetUIState( LoginState.Trying );

                RockMobileUser.Instance.Login( UsernameField.Text, PasswordField.Text, LoginComplete );
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

                    LoginActivityIndicator.Hidden = true;
                    UsernameField.Enabled = true;
                    PasswordField.Enabled = true;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    CreateAccountButton.Hidden = false;
                    CreateAccountButton.Enabled = true;

                    LoginButton.SetTitle( "Login", UIControlState.Normal );

                    break;
                }

                case LoginState.Trying:
                {
                    LoginActivityIndicator.Hidden = false;
                    UsernameField.Enabled = false;
                    PasswordField.Enabled = false;
                    LoginButton.Enabled = false;
                    CancelButton.Enabled = false;
                    CreateAccountButton.Enabled = false;

                    LoginButton.SetTitle( "Login", UIControlState.Normal );

                    break;
                }

                case LoginState.In:
                {
                    LoginActivityIndicator.Hidden = true;
                    UsernameField.Enabled = false;
                    PasswordField.Enabled = false;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    CreateAccountButton.Hidden = true;
                    CreateAccountButton.Enabled = false;

                    LoginButton.SetTitle( "Logout", UIControlState.Normal );

                    break;
                }
            }

            State = state;
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
                    LoginActivityIndicator.Hidden = true;
                    CancelButton.Hidden = true;
                    LoginButton.Hidden = true;
                    CreateAccountButton.Hidden = true;

                    // update the UI
                    LoginResultLabel.Text = string.Format( "Welcome back, {0}!", model.FirstName );

                    // start the timer, which will notify the springboard we're logged in when it ticks.
                    LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                        {
                            // when the timer fires, notify the springboard we're done.
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { Springboard.ResignModelViewController( this ); } );
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
	}
}
