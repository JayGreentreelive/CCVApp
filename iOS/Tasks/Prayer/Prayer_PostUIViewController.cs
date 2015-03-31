using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using CoreGraphics;
using CCVApp.Shared.Config;
using CCVApp.Shared.Analytics;
using CCVApp.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;

namespace iOS
{
    partial class Prayer_PostUIViewController : TaskUIViewController
	{
        public Rock.Client.PrayerRequest PrayerRequest { get; set; }
        public bool Posting { get; set; }
        bool Success { get; set; }
        bool IsActive { get; set; }

        UIBlockerView BlockerView { get; set; }

		public Prayer_PostUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            StatusLabel.Text = PrayerStrings.PostPrayer_Status_Submitting;

            //setup our appearance
            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            ControlStyling.StyleUILabel( StatusLabel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( StatusBackground );

            ControlStyling.StyleUILabel( ResultLabel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( ResultBackground );

            ResultSymbolLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Icon_Font_Secondary, PrayerConfig.PostPrayer_ResultSymbolSize );
            ResultSymbolLabel.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            ResultSymbolLabel.BackgroundColor = UIColor.Clear;

            ControlStyling.StyleButton( DoneButton, GeneralStrings.Done, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            Success = false;
            Posting = false;

            IsActive = true;

            DoneButton.Hidden = true;
            ResultLabel.Hidden = true;
            ResultSymbolLabel.Hidden = true;

            if ( PrayerRequest == null )
            {
                throw new Exception( "Set a PrayerRequest before loading this view controller!" );
            }

            DoneButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    if( Success == true )
                    {
                        NavigationController.PopToRootViewController( true );
                    }
                    else
                    {
                        SubmitPrayerRequest( );
                    }
                };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // immediately attempt to submit the request
            SubmitPrayerRequest( );

            // Note: Ideally i'd like to disable the springboard button until the post is finished.
            // However, that would give them NO WAY to get out of this page should they decide
            // they don't want the prayer to finish posting. Maybe it's fine to let them leave if they
            // want to.
            //EnableSpringboardRevealButton
            Task.NavToolbar.Reveal( false );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsActive = false;
        }

        void SubmitPrayerRequest( )
        {
            StatusLabel.Text = PrayerStrings.PostPrayer_Status_Submitting;
            Success = false;
            Posting = true;

            DoneButton.Hidden = true;
            ResultLabel.Hidden = true;
            ResultSymbolLabel.Hidden = true;

            // fade in our blocker, and when it's complete, send our request off
            BlockerView.Show( delegate
                {
                    // sleep this thread for a second to give an appearance of submission
                    System.Threading.Thread.Sleep( 1000 );

                    // submit the request
                    CCVApp.Shared.Network.RockApi.Instance.PutPrayer( PrayerRequest, delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            Posting = false;

                            // if they left while posting, screw em.
                            if ( IsActive == true )
                            {
                                BlockerView.Hide( null );

                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                {
                                    Success = true;

                                    StatusLabel.Text = PrayerStrings.PostPrayer_Status_SuccessText;

                                    ResultLabel.Hidden = false;
                                    ResultLabel.Text = PrayerStrings.PostPrayer_Result_SuccessText;

                                    ResultSymbolLabel.Hidden = false;
                                    ResultSymbolLabel.Text = ControlStylingConfig.Result_Symbol_Success;

                                    DoneButton.Hidden = false;
                                    DoneButton.SetTitle( GeneralStrings.Done, UIControlState.Normal );

                                    PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Create );
                                }
                                else
                                {
                                    Success = false;

                                    StatusLabel.Text = PrayerStrings.PostPrayer_Status_FailedText;

                                    ResultLabel.Hidden = false;

                                    ResultLabel.Text = PrayerStrings.PostPrayer_Result_FailedText;

                                    ResultSymbolLabel.Hidden = false;
                                    ResultSymbolLabel.Text = ControlStylingConfig.Result_Symbol_Failed;

                                    DoneButton.Hidden = false;
                                    DoneButton.SetTitle( GeneralStrings.Retry, UIControlState.Normal );
                                }
                            }
                        } );
                } );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // don't allow touch input while we're posting
            if ( Posting == false )
            {
                base.TouchesEnded( touches, evt );
            }
        }
	}
}
