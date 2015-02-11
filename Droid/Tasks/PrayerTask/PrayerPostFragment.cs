
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
using System.Drawing;
using CCVApp.Shared.Strings;
using CCVApp.Shared.Config;
using CCVApp.Shared.Analytics;
using Rock.Mobile.PlatformSpecific.Android.Graphics;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerPostFragment : TaskFragment
            {
                public Rock.Client.PrayerRequest PrayerRequest { get; set; }
                public bool Posting { get; set; }
                bool Success { get; set; }
                bool IsActive { get; set; }


                public ProgressBar ProgressBar { get; set; }
                public View StatusLayer { get; set; }
                public TextView StatusText { get; set; }

                public View ResultLayer { get; set; }
                public TextView ResultSymbol { get; set; }
                public TextView ResultText { get; set; }

                public Button DoneButton { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.Prayer_Post, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );


                    StatusLayer = view.FindViewById<View>( Resource.Id.status_background );
                    ControlStyling.StyleBGLayer( StatusLayer );

                    StatusText = StatusLayer.FindViewById<TextView>( Resource.Id.text );
                    ControlStyling.StyleUILabel( StatusText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );



                    ResultLayer = view.FindViewById<View>( Resource.Id.result_background );
                    ControlStyling.StyleBGLayer( ResultLayer );

                    ResultSymbol = ResultLayer.FindViewById<TextView>( Resource.Id.resultSymbol );
                    ResultSymbol.SetTypeface( FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary ), TypefaceStyle.Normal );
                    ResultSymbol.SetTextSize( ComplexUnitType.Dip, PrayerConfig.PostPrayer_ResultSymbolSize );
                    ResultSymbol.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );

                    ResultText = ResultLayer.FindViewById<TextView>( Resource.Id.text );
                    ControlStyling.StyleUILabel( ResultText, ControlStylingConfig.Large_Font_Regular, ControlStylingConfig.Large_FontSize );

                    ProgressBar = ResultLayer.FindViewById<ProgressBar>( Resource.Id.activityIndicator );


                    DoneButton = view.FindViewById<Button>( Resource.Id.doneButton );
                    ControlStyling.StyleButton( DoneButton, GeneralStrings.Done, ControlStylingConfig.Large_Font_Regular, ControlStylingConfig.Large_FontSize );


                    DoneButton.Click += (object sender, EventArgs e ) =>
                    {
                            if( Success == true )
                            {
                                // leave
                                ParentTask.OnClick( this, 0 );
                            }
                            else
                            {
                                // retry
                                SubmitPrayerRequest( );
                            }
                    };

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    IsActive = true;

                    SubmitPrayerRequest( );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    IsActive = false;
                }

                void SubmitPrayerRequest( )
                {
                    // update the status to say "submitting..."
                    StatusText.Text = PrayerStrings.PostPrayer_Status_Submitting;

                    // clear the results section
                    ResultSymbol.Text = "";
                    ResultText.Text = "";

                    // hide the done button
                    DoneButton.Enabled = false;
                    DoneButton.Visibility = ViewStates.Invisible;

                    ProgressBar.Visibility = ViewStates.Visible;

                    Success = false;
                    Posting = true;

                    // submit the request
                    CCVApp.Shared.Network.RockApi.Instance.PutPrayer( PrayerRequest, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            Posting = false;
                            ProgressBar.Visibility = ViewStates.Invisible;

                            // if they left while posting, screw em.
                            if( IsActive == true )
                            {
                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                {
                                    Success = true;

                                    // success! Update text to say so, and reveal the done button.
                                    PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Create );

                                    StatusText.Text = PrayerStrings.PostPrayer_Status_SuccessText;

                                    ResultSymbol.Text = PrayerConfig.PostPrayer_ResultSymbol_SuccessText;
                                    ResultText.Text = PrayerStrings.PostPrayer_Result_SuccessText;

                                    DoneButton.Visibility = ViewStates.Visible;
                                    DoneButton.Text = GeneralStrings.Done;
                                    DoneButton.Enabled = true;
                                }
                                else
                                {
                                    Success = false;

                                    // failed. Update text to say so, and use the done button as a "retry"
                                    StatusText.Text = PrayerStrings.PostPrayer_Status_FailedText;

                                    ResultSymbol.Text = PrayerConfig.PostPrayer_ResultSymbol_FailedText;
                                    ResultText.Text = PrayerStrings.PostPrayer_Result_FailedText;

                                    DoneButton.Visibility = ViewStates.Visible;
                                    DoneButton.Text = GeneralStrings.Retry;
                                    DoneButton.Enabled = true;
                                }
                            }
                        } );
                }
            }
        }
    }
}
