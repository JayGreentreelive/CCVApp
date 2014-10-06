
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

namespace Droid
{
    public class ProfileFragment : Fragment
    {
        public Springboard SpringboardParent { get; set; }

        EditText FirstNameField { get; set; }
        EditText MiddleNameField { get; set; }
        EditText LastNameField { get; set; }
        EditText NickNameField { get; set; }
        EditText EmailField { get; set; }

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

            FirstNameField = view.FindViewById<EditText>( Resource.Id.profile_first_name );
            FirstNameField.AfterTextChanged += (sender, e) => { Dirty = true; };

            MiddleNameField = view.FindViewById<EditText>( Resource.Id.profile_middle_name );
            MiddleNameField.AfterTextChanged += (sender, e) => { Dirty = true; };

            LastNameField = view.FindViewById<EditText>( Resource.Id.profile_last_name );
            LastNameField.AfterTextChanged += (sender, e) => { Dirty = true; };

            NickNameField = view.FindViewById<EditText>( Resource.Id.profile_nickname );
            NickNameField.AfterTextChanged += (sender, e) => { Dirty = true; };

            EmailField = view.FindViewById<EditText>( Resource.Id.profile_email );
            EmailField.AfterTextChanged += (sender, e) => { Dirty = true; };

            DoneButton = view.FindViewById<Button>( Resource.Id.profile_doneButton );
            LogoutButton = view.FindViewById<Button>( Resource.Id.profile_logoutButton );

            DoneButton.Click += (object sender, EventArgs e) => 
                {
                    if( Dirty == true )
                    {
                        // Since they made changes, confirm they want to save them.
                        AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                        builder.SetTitle( "Submit Changes?" );

                        Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                            {
                                new Java.Lang.String( "Yes" ),
                                new Java.Lang.String( "No, Thanks" ),
                                new Java.Lang.String( "Cancel" )
                            };

                        builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                            {
                                Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                                    {
                                        switch( clickArgs.Which )
                                        {
                                            case 0: SubmitChanges( ); SpringboardParent.ModalFragmentFinished( this ); break;
                                            case 1: SpringboardParent.ModalFragmentFinished( this ); break;
                                            case 2: break;
                                        }
                                    });
                            });

                        builder.Show( );
                    }
                    else
                    {
                        SpringboardParent.ModalFragmentFinished( this );
                    }
                };

            LogoutButton.Click += (object sender, EventArgs e) => 
                {
                    // Since they made changes, confirm they want to save them.
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    builder.SetTitle( "Log Out?" );

                    Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                        {
                            new Java.Lang.String( "Yes" ),
                            new Java.Lang.String( "No, Thanks" ),
                            new Java.Lang.String( "Cancel" )
                        };

                    builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                                {
                                    switch( clickArgs.Which )
                                    {
                                        case 0: RockMobileUser.Instance.Logout( ); SpringboardParent.ModalFragmentFinished( this ); break;
                                        case 1: SpringboardParent.ModalFragmentFinished( this ); break;
                                        case 2: break;
                                    }
                                });
                        });
                };

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            // logged in sanity check.
            if( RockMobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );

            FirstNameField.Text = RockMobileUser.Instance.Person.FirstName;
            MiddleNameField.Text = RockMobileUser.Instance.Person.MiddleName;
            LastNameField.Text = RockMobileUser.Instance.Person.LastName;

            NickNameField.Text = RockMobileUser.Instance.Person.NickName;

            EmailField.Text = RockMobileUser.Instance.Person.Email;

            // clear the dirty flag AFTER setting all values so the initial setup
            // doesn't get flagged as dirty
            Dirty = false;
        }

        void SubmitChanges()
        {
            // copy all the edited fields into the person object
            RockMobileUser.Instance.Person.Email = EmailField.Text;

            RockMobileUser.Instance.Person.MiddleName = MiddleNameField.Text;

            RockMobileUser.Instance.Person.NickName = NickNameField.Text;

            RockMobileUser.Instance.Person.FirstName = FirstNameField.Text;
            RockMobileUser.Instance.Person.LastName = LastNameField.Text;

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            RockMobileUser.Instance.UpdateProfile( null );
        }

    }
}

