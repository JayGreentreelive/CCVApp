
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
using Android.Graphics;
using Rock.Mobile.PlatformUI;
using Rock.Mobile.PlatformCommon;
using System.Drawing;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerCreateFragment : TaskFragment
            {
                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Prayer_Create, container, false);
                    view.SetOnTouchListener( this );

                    EditText FirstNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_firstNameText );
                    EditText LastNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_lastNameText );
                    EditText RequestText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_requestText );
                    ProgressBar ActivityIndicator = (ProgressBar)view.FindViewById<ProgressBar>( Resource.Id.prayer_create_activityIndicator );
                    ActivityIndicator.Visibility = ViewStates.Gone;


                    Button submitButton = (Button)view.FindViewById<Button>( Resource.Id.prayer_create_submitButton );
                    submitButton.Click += (object sender, EventArgs e ) =>
                    {
                            if( string.IsNullOrEmpty( FirstNameText.Text ) == false &&
                                string.IsNullOrEmpty( RequestText.Text ) == false )
                            {
                                Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                                FirstNameText.Enabled = false;
                                LastNameText.Enabled = false;
                                RequestText.Enabled = false;

                                prayerRequest.FirstName = FirstNameText.Text;
                                prayerRequest.LastName = LastNameText.Text;
                                prayerRequest.Text = RequestText.Text;
                                prayerRequest.EnteredDateTime = DateTime.Now;

                                ActivityIndicator.Visibility = ViewStates.Visible;

                                // submit the request
                                CCVApp.Shared.Network.RockApi.Instance.PutPrayer( prayerRequest, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                                    {
                                        ActivityIndicator.Visibility = ViewStates.Gone;

                                        if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                        {
                                            Activity.OnBackPressed();
                                        }
                                        else
                                        {
                                            // if it failed to post, let them try again.
                                            Springboard.DisplayError( CCVApp.Shared.Strings.Prayer.Error_Title, CCVApp.Shared.Strings.Prayer.Error_Submit_Message );
                                            FirstNameText.Enabled = true;
                                            LastNameText.Enabled = true;
                                            RequestText.Enabled = true;
                                        }
                                    });
                            }
                    };

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.0f );
                }
            }
        }
    }
}
