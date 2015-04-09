
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
using Android.Text;
using Android.Widget;
using CCVApp.Shared.Network;
using Android.Views.InputMethods;
using CCVApp.Shared.Strings;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using Android.Telephony;
using Rock.Mobile.Util.Strings;
using Java.Lang.Reflect;
using CCVApp.Shared.UI;

namespace Droid
{
    public class RegisterFragment : Fragment, View.IOnTouchListener
    {
        public Springboard SpringboardParent { get; set; }

        EditText UserNameText { get; set; }
        uint UserNameBGColor { get; set; }

        EditText PasswordText { get; set; }
        uint PasswordBGColor { get; set; }

        EditText ConfirmPasswordText { get; set; }
        uint ConfirmPasswordBGColor { get; set; }

        EditText NickNameText { get; set; }
        uint NickNameBGColor { get; set; }

        EditText LastNameText { get; set; }
        uint LastNameBGColor { get; set; }

        EditText EmailText { get; set; }
        uint EmailBGColor { get; set; }

        EditText CellPhoneText { get; set; }

        Button RegisterButton { get; set; }
        Button CancelButton { get; set; }

        enum RegisterState
        {
            None,
            Trying,
            Success,
            Fail
        }

        RegisterState State { get; set; }

        UIResultView ResultView { get; set; }

        RelativeLayout ProgressBarBlocker { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            View view = inflater.Inflate(Resource.Layout.Register, container, false);
            view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
            view.SetOnTouchListener( this );

            RelativeLayout layoutView = view.FindViewById<RelativeLayout>( Resource.Id.scroll_linear_background );

            ProgressBarBlocker = view.FindViewById<RelativeLayout>( Resource.Id.progressBarBlocker );
            ProgressBarBlocker.Visibility = ViewStates.Gone;
            ProgressBarBlocker.LayoutParameters = new RelativeLayout.LayoutParams( 0, 0 );
            ProgressBarBlocker.LayoutParameters.Width = NavbarFragment.GetFullDisplayWidth( );
            ProgressBarBlocker.LayoutParameters.Height = this.Resources.DisplayMetrics.HeightPixels;

            ResultView = new UIResultView( layoutView, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), OnResultViewDone );

            ResultView.SetStyle( ControlStylingConfig.Medium_Font_Light, 
                ControlStylingConfig.Icon_Font_Secondary, 
                ControlStylingConfig.BackgroundColor,
                ControlStylingConfig.BG_Layer_Color, 
                ControlStylingConfig.BG_Layer_BorderColor, 
                ControlStylingConfig.TextField_PlaceholderTextColor,
                ControlStylingConfig.Button_BGColor, 
                ControlStylingConfig.Button_TextColor );


            RelativeLayout navBar = view.FindViewById<RelativeLayout>( Resource.Id.navbar_relative_layout );
            navBar.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );


            // setup the username / password section
            RelativeLayout backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.username_background );
            ControlStyling.StyleBGLayer( backgroundView );

            UserNameText = backgroundView.FindViewById<EditText>( Resource.Id.userNameText );
            ControlStyling.StyleTextField( UserNameText, RegisterStrings.UsernamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            UserNameBGColor = ControlStylingConfig.BG_Layer_Color;
            UserNameText.InputType |= InputTypes.TextFlagCapWords;

            View borderView = backgroundView.FindViewById<View>( Resource.Id.username_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            PasswordText = backgroundView.FindViewById<EditText>( Resource.Id.passwordText );
            ControlStyling.StyleTextField( PasswordText, RegisterStrings.PasswordPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            PasswordBGColor = ControlStylingConfig.BG_Layer_Color;
            PasswordText.InputType |= InputTypes.TextVariationPassword;

            borderView = backgroundView.FindViewById<View>( Resource.Id.password_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            ConfirmPasswordText = backgroundView.FindViewById<EditText>( Resource.Id.confirmPasswordText );
            ControlStyling.StyleTextField( ConfirmPasswordText, RegisterStrings.ConfirmPasswordPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            ConfirmPasswordBGColor = ControlStylingConfig.BG_Layer_Color;
            ConfirmPasswordText.InputType |= InputTypes.TextVariationPassword;


            // setup the name section
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.name_background );
            ControlStyling.StyleBGLayer( backgroundView );

            NickNameText = backgroundView.FindViewById<EditText>( Resource.Id.nickNameText );
            ControlStyling.StyleTextField( NickNameText, RegisterStrings.NickNamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            NickNameBGColor = ControlStylingConfig.BG_Layer_Color;
            NickNameText.InputType |= InputTypes.TextFlagCapWords;

            borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            LastNameText = backgroundView.FindViewById<EditText>( Resource.Id.lastNameText );
            ControlStyling.StyleTextField( LastNameText, RegisterStrings.LastNamePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            LastNameBGColor = ControlStylingConfig.BG_Layer_Color;
            LastNameText.InputType |= InputTypes.TextFlagCapWords;


            // setup the contact section
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.contact_background );
            ControlStyling.StyleBGLayer( backgroundView );

            CellPhoneText = backgroundView.FindViewById<EditText>( Resource.Id.cellPhoneText );
            ControlStyling.StyleTextField( CellPhoneText, RegisterStrings.CellPhonePlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            CellPhoneText.AddTextChangedListener(new PhoneNumberFormattingTextWatcher());

            borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            EmailText = backgroundView.FindViewById<EditText>( Resource.Id.emailAddressText );
            ControlStyling.StyleTextField( EmailText, RegisterStrings.EmailPlaceholder, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            EmailBGColor = ControlStylingConfig.BG_Layer_Color;


            // Register button
            RegisterButton = view.FindViewById<Button>( Resource.Id.registerButton );
            ControlStyling.StyleButton( RegisterButton, RegisterStrings.RegisterButton, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            CancelButton = view.FindViewById<Button>( Resource.Id.cancelButton );
            ControlStyling.StyleButton( CancelButton, GeneralStrings.Cancel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            RegisterButton.Click += (object sender, EventArgs e) => 
                {
                    RegisterUser( );
                };

            CancelButton.Click += (object sender, EventArgs e) => 
                {
                    // Since they made changes, confirm they want to save them.
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    builder.SetTitle( RegisterStrings.ConfirmCancelReg );

                    Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                        {
                            new Java.Lang.String( GeneralStrings.Yes ),
                            new Java.Lang.String( GeneralStrings.No )
                        };

                    builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    switch( clickArgs.Which )
                                    {
                                        case 0: SpringboardParent.ModalFragmentDone( null ); break;
                                        case 1: break;
                                    }
                                });
                        });

                    builder.Show( );
                };

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            // logged in sanity check.
            //if( RockMobileUser.Instance.LoggedIn == true ) throw new Exception("A user cannot be logged in when registering. How did you do this?" );

            UserNameText.Text = string.Empty;
            PasswordText.Text = string.Empty;
            ConfirmPasswordText.Text = string.Empty;

            NickNameText.Text = string.Empty;
            LastNameText.Text = string.Empty;

            EmailText.Text = string.Empty;
            CellPhoneText.Text = string.Empty;

            SpringboardParent.ModalFragmentOpened( this );
        }

        public override void OnStop()
        {
            base.OnStop();

            SpringboardParent.ModalFragmentDone( null );

            State = RegisterState.None;
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            // consume all input so things tasks underneath don't respond
            return true;
        }

        void ToggleControls( bool enabled )
        {
            UserNameText.Enabled = enabled;
            PasswordText.Enabled = enabled;
            ConfirmPasswordText.Enabled = enabled;

            NickNameText.Enabled = enabled;
            LastNameText.Enabled = enabled;

            EmailText.Enabled = enabled;
            CellPhoneText.Enabled = enabled;
            RegisterButton.Enabled = enabled;
            CancelButton.Enabled = enabled;
        }

        void RegisterUser()
        {
            if ( State == RegisterState.None )
            {
                // make sure they entered all required fields
                if ( ValidateInput( ) )
                {
                    ToggleControls( false );

                    ProgressBarBlocker.Visibility = ViewStates.Visible;
                    State = RegisterState.Trying;

                    // create a new user and submit them
                    Rock.Client.Person newPerson = new Rock.Client.Person();
                    Rock.Client.PhoneNumber newPhoneNumber = new Rock.Client.PhoneNumber();

                    // copy all the edited fields into the person object
                    newPerson.Email = EmailText.Text;

                    newPerson.NickName = NickNameText.Text;
                    newPerson.LastName = LastNameText.Text;

                    // Update their cell phone. 
                    if ( string.IsNullOrEmpty( CellPhoneText.Text ) == false )
                    {
                        // update the phone number
                        string digits = CellPhoneText.Text.AsNumeric( );
                        newPhoneNumber.Number = digits;
                        newPhoneNumber.NumberFormatted = digits.AsPhoneNumber( );
                        newPhoneNumber.NumberTypeValueId = GeneralConfig.CellPhoneValueId;
                    }

                    RockApi.Instance.RegisterNewUser( newPerson, newPhoneNumber, UserNameText.Text, PasswordText.Text,
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            ProgressBarBlocker.Visibility = ViewStates.Gone;
                             ToggleControls( true );

                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                State = RegisterState.Success;
                                ResultView.Show( RegisterStrings.RegisterStatus_Success, 
                                    ControlStylingConfig.Result_Symbol_Success, 
                                    RegisterStrings.RegisterResult_Success,
                                    GeneralStrings.Done );
                            }
                            else
                            {
                                State = RegisterState.Fail;
                                ResultView.Show( RegisterStrings.RegisterStatus_Failed, 
                                    ControlStylingConfig.Result_Symbol_Failed, 
                                    RegisterStrings.RegisterResult_Failed,
                                    GeneralStrings.Done );
                            }

                            ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                        } );
                }
            }
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            ProgressBarBlocker.LayoutParameters.Width = NavbarFragment.GetFullDisplayWidth( );
            ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
        }

        /// <summary>
        /// Ensure all required fields have data
        /// </summary>
        public bool ValidateInput( )
        {
            bool result = true;

            // validate there's text in all required fields
            uint userNameTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( UserNameText.Text ) == true )
            {
                userNameTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( UserNameBGColor, userNameTargetColor, UserNameText, delegate { UserNameBGColor = userNameTargetColor; } );

            // for the password, if EITHER field is blank, that's not ok, OR if the passwords don't match, also not ok.
            uint passwordTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( (string.IsNullOrEmpty( PasswordText.Text ) == true || string.IsNullOrEmpty( ConfirmPasswordText.Text ) == true) ||
                 (PasswordText.Text != ConfirmPasswordText.Text) )
            {
                passwordTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( PasswordBGColor, passwordTargetColor, PasswordText, delegate { PasswordBGColor = passwordTargetColor; } );
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( ConfirmPasswordBGColor, passwordTargetColor, ConfirmPasswordText, delegate { ConfirmPasswordBGColor = passwordTargetColor; } );


            uint nickNameTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( NickNameText.Text ) == true )
            {
                nickNameTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( NickNameBGColor, nickNameTargetColor, NickNameText, delegate { NickNameBGColor = nickNameTargetColor; } );


            uint lastNameTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( LastNameText.Text ) == true )
            {
                lastNameTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( LastNameBGColor, lastNameTargetColor, LastNameText, delegate { LastNameBGColor = lastNameTargetColor; } );

            // cell phone OR email is fine
            uint emailTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( EmailText.Text ) == true && string.IsNullOrEmpty( CellPhoneText.Text ) == true )
            {
                emailTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( EmailBGColor, emailTargetColor, EmailText, delegate { EmailBGColor = emailTargetColor; } );


            return result;
        }

        void OnResultViewDone( )
        {
            switch ( State )
            {
                case RegisterState.Success:
                {
                    SpringboardParent.ModalFragmentDone( null );
                    State = RegisterState.None;
                    break;
                }

                case RegisterState.Fail:
                {
                    ResultView.Hide( );
                    State = RegisterState.None;
                    break;
                }
            }
        }

    }
}

