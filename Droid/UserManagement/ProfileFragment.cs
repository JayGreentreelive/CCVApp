
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

namespace Droid
{
    public class ProfileFragment : Fragment
    {
        public Springboard SpringboardParent { get; set; }

        EditText NickNameField { get; set; }
        EditText LastNameField { get; set; }

        EditText CellPhoneField { get; set; }
        EditText EmailField { get; set; }

        EditText StreetField { get; set; }
        EditText CityField { get; set; }
        EditText StateField { get; set; }
        EditText ZipField { get; set; }

        EditText BirthdateField { get; set; }
        EditText GenderField { get; set; }

        Button DoneButton { get; set; }
        Button LogoutButton { get; set; }

        bool Dirty { get; set; }


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

            View view = inflater.Inflate(Resource.Layout.Profile, container, false);
            view.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            RelativeLayout navBar = view.FindViewById<RelativeLayout>( Resource.Id.navbar_relative_layout );
            navBar.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            // setup the name section
            RelativeLayout backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.name_background );
            ControlStyling.StyleBGLayer( backgroundView );

            NickNameField = backgroundView.FindViewById<EditText>( Resource.Id.nickNameText );
            ControlStyling.StyleTextField( NickNameField, ProfileStrings.NickNamePlaceholder );
            NickNameField.AfterTextChanged += (sender, e) => { Dirty = true; };

            View borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            LastNameField = backgroundView.FindViewById<EditText>( Resource.Id.lastNameText );
            ControlStyling.StyleTextField( LastNameField, ProfileStrings.LastNamePlaceholder );
            LastNameField.AfterTextChanged += (sender, e) => { Dirty = true; };


            // setup the contact section
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.contact_background );
            ControlStyling.StyleBGLayer( backgroundView );

            CellPhoneField = backgroundView.FindViewById<EditText>( Resource.Id.cellPhoneText );
            ControlStyling.StyleTextField( CellPhoneField, ProfileStrings.CellPhonePlaceholder );
            CellPhoneField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            EmailField = backgroundView.FindViewById<EditText>( Resource.Id.emailAddressText );
            ControlStyling.StyleTextField( EmailField, ProfileStrings.EmailPlaceholder );
            EmailField.AfterTextChanged += (sender, e) => { Dirty = true; };


            // setup the address section
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.address_background );
            ControlStyling.StyleBGLayer( backgroundView );

            StreetField = backgroundView.FindViewById<EditText>( Resource.Id.streetAddressText );
            ControlStyling.StyleTextField( StreetField, ProfileStrings.StreetPlaceholder );
            StreetField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.street_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            CityField = backgroundView.FindViewById<EditText>( Resource.Id.cityAddressText );
            ControlStyling.StyleTextField( CityField, ProfileStrings.CityPlaceholder );
            CityField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.city_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            StateField = backgroundView.FindViewById<EditText>( Resource.Id.stateAddressText );
            ControlStyling.StyleTextField( StateField, ProfileStrings.StatePlaceholder );
            StateField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.state_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            ZipField = backgroundView.FindViewById<EditText>( Resource.Id.zipAddressText );
            ControlStyling.StyleTextField( ZipField, ProfileStrings.ZipPlaceholder );
            ZipField.AfterTextChanged += (sender, e) => { Dirty = true; };


            // personal
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.personal_background );
            ControlStyling.StyleBGLayer( backgroundView );

            BirthdateField = backgroundView.FindViewById<EditText>( Resource.Id.birthdateText );
            ControlStyling.StyleTextField( BirthdateField, ProfileStrings.BirthdatePlaceholder );
            BirthdateField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            GenderField = view.FindViewById<EditText>( Resource.Id.genderText );
            ControlStyling.StyleTextField( GenderField, ProfileStrings.GenderPlaceholder );
            GenderField.AfterTextChanged += (sender, e) => { Dirty = true; };


            // Done buttons
            DoneButton = view.FindViewById<Button>( Resource.Id.doneButton );
            ControlStyling.StyleButton( DoneButton, ProfileStrings.DoneButtonTitle );

            LogoutButton = view.FindViewById<Button>( Resource.Id.logoutButton );
            ControlStyling.StyleButton( LogoutButton, ProfileStrings.LogoutButtonTitle );

            DoneButton.Click += (object sender, EventArgs e) => 
                {
                    if( Dirty == true )
                    {
                        // Since they made changes, confirm they want to save them.
                        AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                        builder.SetTitle( ProfileStrings.SubmitChangesTitle );

                        Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                            {
                                new Java.Lang.String( GeneralStrings.Yes ),
                                new Java.Lang.String( GeneralStrings.No ),
                                new Java.Lang.String( GeneralStrings.Cancel )
                            };

                        builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                            {
                                Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                                    {
                                        switch( clickArgs.Which )
                                        {
                                            case 0: SubmitChanges( ); SpringboardParent.ModalFragmentFinished( this, null ); break;
                                            case 1: SpringboardParent.ModalFragmentFinished( this, null ); break;
                                            case 2: break;
                                        }
                                    });
                            });

                        builder.Show( );
                    }
                    else
                    {
                        SpringboardParent.ModalFragmentFinished( this, null );
                    }
                };

            LogoutButton.Click += (object sender, EventArgs e) => 
                {
                    // Since they made changes, confirm they want to save them.
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    builder.SetTitle( ProfileStrings.LogoutTitle );

                    Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                        {
                            new Java.Lang.String( GeneralStrings.Yes ),
                            new Java.Lang.String( GeneralStrings.No ),
                            new Java.Lang.String( GeneralStrings.Cancel )
                        };

                    builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                                {
                                    switch( clickArgs.Which )
                                    {
                                        case 0: RockMobileUser.Instance.Logout( ); SpringboardParent.ModalFragmentFinished( this, null ); break;
                                        case 1: SpringboardParent.ModalFragmentFinished( this, null ); break;
                                        case 2: break;
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
            if( RockMobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );

            NickNameField.Text = RockMobileUser.Instance.Person.NickName;
            LastNameField.Text = RockMobileUser.Instance.Person.LastName;

            EmailField.Text = RockMobileUser.Instance.Person.Email;

            // clear the dirty flag AFTER setting all values so the initial setup
            // doesn't get flagged as dirty
            Dirty = false;
        }

        void SubmitChanges()
        {
            // copy all the edited fields into the person object
            RockMobileUser.Instance.Person.Email = EmailField.Text;

            RockMobileUser.Instance.Person.NickName = NickNameField.Text;
            RockMobileUser.Instance.Person.LastName = LastNameField.Text;

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            RockMobileUser.Instance.UpdateProfile( null );
        }

    }
}

