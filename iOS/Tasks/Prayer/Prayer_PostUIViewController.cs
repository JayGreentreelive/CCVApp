using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using System.Drawing;
using Rock.Mobile.PlatformCommon;
using CCVApp.Shared.Config;

namespace iOS
{
    partial class Prayer_PostUIViewController : TaskUIViewController
	{
        public Rock.Client.PrayerRequest PrayerRequest { get; set; }
        public bool Posting { get; set; }
        bool Success { get; set; }
        bool IsActive { get; set; }

        BlockerView BlockerView { get; set; }

		public Prayer_PostUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new BlockerView( View.Frame );
            View.AddSubview( BlockerView );

            StatusLabel.Text = PrayerStrings.PostPrayer_Status_Submitting;

            //setup our appearance
            View.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );

            ControlStyling.StyleUILabel( StatusLabel );
            ControlStyling.StyleBGLayer( StatusBackground );

            ControlStyling.StyleUILabel( ResultLabel );
            ControlStyling.StyleBGLayer( ResultBackground );

            ResultSymbolLabel.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( PrayerConfig.PostPrayer_ResultSymbolFont, PrayerConfig.PostPrayer_ResultSymbolSize );
            ResultSymbolLabel.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            ResultSymbolLabel.BackgroundColor = UIColor.Clear;

            ControlStyling.StyleButton( DoneButton, GeneralStrings.Done );
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

            BlockerView.Hidden = false;
            BlockerView.Layer.Opacity = 0.00f;

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
            BlockerView.FadeIn( delegate
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
                                BlockerView.FadeOut( null );

                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                {
                                    Success = true;

                                    StatusLabel.Text = PrayerStrings.PostPrayer_Status_SuccessText;

                                    ResultLabel.Hidden = false;
                                    ResultLabel.Text = PrayerStrings.PostPrayer_Result_SuccessText;

                                    ResultSymbolLabel.Hidden = false;
                                    ResultSymbolLabel.Text = PrayerConfig.PostPrayer_ResultSymbol_SuccessText;

                                    DoneButton.Hidden = false;
                                    DoneButton.SetTitle( "Done", UIControlState.Normal );
                                }
                                else
                                {
                                    Success = false;

                                    StatusLabel.Text = PrayerStrings.PostPrayer_Status_FailedText;

                                    ResultLabel.Hidden = false;

                                    ResultLabel.Text = PrayerStrings.PostPrayer_Result_FailedText;

                                    ResultSymbolLabel.Hidden = false;
                                    ResultSymbolLabel.Text = PrayerConfig.PostPrayer_ResultSymbol_FailedText;

                                    DoneButton.Hidden = false;
                                    DoneButton.SetTitle( "Retry", UIControlState.Normal );
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
