
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
using CCVApp.Shared.Strings;
using CCVApp.Shared.Config;

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

                    view.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // setup the name background
                    RelativeLayout backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.name_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    View borderView = backgroundLayout.FindViewById<View>( Resource.Id.middle_border );
                    borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    //

                    // setup the prayer request background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.prayerRequest_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );
                    //

                    // setup the switch background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.switch_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    // setup the category background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.spinner_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    // setup the text views
                    EditText firstNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_firstNameText );
                    ControlStyling.StyleTextField( firstNameText, PrayerStrings.CreatePrayer_FirstNamePlaceholderText );

                    EditText lastNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_lastNameText );
                    ControlStyling.StyleTextField( lastNameText, PrayerStrings.CreatePrayer_LastNamePlaceholderText );

                    EditText requestText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_requestText );
                    ControlStyling.StyleTextField( requestText, PrayerStrings.CreatePrayer_PrayerRequest );


                    ProgressBar ActivityIndicator = (ProgressBar)view.FindViewById<ProgressBar>( Resource.Id.prayer_create_activityIndicator );
                    ActivityIndicator.Visibility = ViewStates.Gone;

                    Switch anonymousSwitch = (Switch)view.FindViewById<Switch>( Resource.Id.postAnonymousSwitch );
                    anonymousSwitch.Checked = false;
                    anonymousSwitch.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e ) =>
                    {
                            if( anonymousSwitch.Checked == false )
                            {
                                firstNameText.Enabled = true;
                                lastNameText.Enabled = true;

                                firstNameText.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                                lastNameText.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                            }
                            else
                            {
                                firstNameText.Enabled = false;
                                lastNameText.Enabled = false;

                                firstNameText.SetTextColor( Android.Graphics.Color.DimGray );
                                lastNameText.SetTextColor( Android.Graphics.Color.DimGray );
                            }
                    };

                    Switch publicSwitch = (Switch)view.FindViewById<Switch>( Resource.Id.makePublicSwitch );
                    publicSwitch.Checked = true;

                    // setup our category spinner
                    Spinner spinner = (Spinner)view.FindViewById<Spinner>( Resource.Id.categorySpinner );
                    ArrayAdapter<String> adapter = new ArrayAdapter<String>( Rock.Mobile.PlatformCommon.Droid.Context, Android.Resource.Layout.SimpleListItem1 );
                    adapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );
                    spinner.Adapter = adapter;

                    // populate the category
                    foreach ( string category in CCVApp.Shared.Network.RockGeneralData.Instance.Data.PrayerCategories )
                    {
                        adapter.Add( category );
                    }

                    Button submitButton = (Button)view.FindViewById<Button>( Resource.Id.prayer_create_submitButton );
                    submitButton.Click += (object sender, EventArgs e ) =>
                    {
                            if( (string.IsNullOrEmpty( firstNameText.Text ) == false || anonymousSwitch.Checked == true) &&
                                string.IsNullOrEmpty( requestText.Text ) == false )
                            {
                                Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                                firstNameText.Enabled = false;
                                lastNameText.Enabled = false;
                                requestText.Enabled = false;

                                // respect their privacy settings
                                if( anonymousSwitch.Checked == true )
                                {
                                    prayerRequest.FirstName = "Anonymous";
                                    prayerRequest.LastName = "Anonymous";
                                }
                                else
                                {
                                    prayerRequest.FirstName = firstNameText.Text;
                                    prayerRequest.LastName = lastNameText.Text;
                                }

                                prayerRequest.Text = requestText.Text;
                                prayerRequest.EnteredDateTime = DateTime.Now;
                                prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                                prayerRequest.CategoryId = 110; //todo: Let the end user set this.
                                prayerRequest.IsActive = true;
                                prayerRequest.IsPublic = publicSwitch.Checked;
                                prayerRequest.IsApproved = false;

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
                                            Springboard.DisplayError( PrayerStrings.Error_Title, PrayerStrings.Error_Submit_Message );
                                            firstNameText.Enabled = true;
                                            lastNameText.Enabled = true;
                                            requestText.Enabled = true;
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
