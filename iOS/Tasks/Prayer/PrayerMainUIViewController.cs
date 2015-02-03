using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CoreGraphics;
using CoreAnimation;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using CCVApp.Shared.Analytics;

namespace iOS
{
    class PrayerCard
    {
        /// <summary>
        /// Overrides the prayer content text so we can detect when the
        /// user is panning while their finger is within it, and then
        /// move the cards.
        /// </summary>
        class PrayerTextView : UITextView
        {
            public UIView Parent { get; set; } 

            public override void TouchesMoved(NSSet touches, UIEvent evt)
            {
                base.TouchesMoved(touches, evt);

                Parent.TouchesMoved( touches, evt );
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);

                Parent.TouchesBegan( touches, evt );
            }

            public override void TouchesCancelled(NSSet touches, UIEvent evt)
            {
                base.TouchesCancelled(touches, evt);

                Parent.TouchesCancelled( touches, evt );
            }
        }

        public PlatformView View { get; set; }
        UILabel Name { get; set; }
        UILabel Date { get; set; }
        UILabel Category { get; set; }
        PrayerTextView PrayerText { get; set; }
        UIButton Pray { get; set; }
        UIView PrayFillIn { get; set; }
        Rock.Client.PrayerRequest PrayerRequest { get; set; }
        bool Prayed { get; set; }

        public PrayerCard( Rock.Client.PrayerRequest prayer, CGRect bounds )
        {
            //setup the actual "card" outline
            View = PlatformView.Create( );
            View.Bounds = new System.Drawing.RectangleF( (float)bounds.X, (float)bounds.Y, (float)bounds.Width, (float)bounds.Height );
            View.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            View.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            View.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            View.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;


            // setup the prayer request text field
            PrayerText = new PrayerTextView( );
            PrayerText.Editable = false;
            PrayerText.BackgroundColor = UIColor.Clear;
            PrayerText.Layer.AnchorPoint = new CGPoint( 0, 0 );
            PrayerText.DelaysContentTouches = false; // don't allow delaying touch, we need to forward it
            PrayerText.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            PrayerText.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
            PrayerText.TextContainerInset = UIEdgeInsets.Zero;
            PrayerText.TextContainer.LineFragmentPadding = 0;

            // setup the bottom prayer button, and its fill-in circle
            Pray = UIButton.FromType( UIButtonType.Custom );
            Pray.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Pray.SetTitle( PrayerStrings.Prayer_Confirm, UIControlState.Normal );

            Pray.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            Pray.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false ) ), UIControlState.Highlighted );

            Pray.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            Pray.SizeToFit( );

            PrayFillIn = new UIView( );
            PrayFillIn.Bounds = new CGRect( 0, 0, Pray.Frame.Height / 2, Pray.Frame.Height / 2 );
            PrayFillIn.Layer.Position = new CGPoint( View.Bounds.Width - PrayFillIn.Layer.Bounds.Width - ViewPadding, View.Bounds.Height - PrayFillIn.Layer.Bounds.Height - (PrayFillIn.Layer.Bounds.Height / 2) );
            PrayFillIn.Layer.CornerRadius = PrayFillIn.Bounds.Width / 2;
            PrayFillIn.Layer.BorderWidth = 1;
            PrayFillIn.Layer.BorderColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
            PrayFillIn.Layer.AnchorPoint = new CGPoint( 0, 0 );

            Pray.Layer.Position = new CGPoint( PrayFillIn.Frame.Left - Pray.Layer.Bounds.Width - ViewPadding, View.Bounds.Height - Pray.Layer.Bounds.Height );
            Pray.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( Prayed == false )
                    {
                        Prayed = true;

                        CCVApp.Shared.Network.RockApi.Instance.IncrementPrayerCount( PrayerRequest.Id, null );

                        PrayFillIn.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor );
                    }
                };


            // setup the name field
            Name = new UILabel( );
            Name.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Name.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            Name.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            // setup the date field
            Date = new UILabel( );
            Date.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Date.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            Date.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );


            // setup the category field
            Category = new UILabel( );
            Category.Layer.AnchorPoint = new CGPoint( 0, 0 );
            Category.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            Category.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );


            // add the controls
            UIView nativeView = View.PlatformNativeObject as UIView;

            nativeView.AddSubview( Name );
            nativeView.AddSubview( Category );
            nativeView.AddSubview( Date );
            nativeView.AddSubview( PrayerText );
            nativeView.AddSubview( Pray );
            nativeView.AddSubview( PrayFillIn );
            PrayerText.Parent = nativeView;

            SetPrayer( prayer );
        }

        const int ViewPadding = 5;
        void SetPrayer( Rock.Client.PrayerRequest prayer )
        {
            PrayerRequest = prayer;

            // set the text for the name, size it so we get the height, then
            // restrict its bounds to the card itself
            Name.Text = prayer.FirstName;
            Name.SizeToFit( );
            Name.Frame = new CGRect( ViewPadding, ViewPadding, View.Bounds.Width - (ViewPadding * 2), Name.Bounds.Height );

            Category.Text = prayer.Category != null ? prayer.Category.Name : "";
            Category.SizeToFit( );
            Category.Layer.Position = new CGPoint( ViewPadding, Name.Frame.Bottom );

            // set the date text, then calculate the dimensions so we can right-adjust it
            Date.Text = string.Format( "{0:MM/dd/yy}", prayer.CreatedDateTime );
            Date.Frame = new CGRect( ViewPadding, Name.Frame.Bottom, View.Bounds.Width - ViewPadding, 0 );
            Date.SizeToFit( );
            Date.Layer.Position = new CGPoint( View.Bounds.Width - Date.Frame.Width - ViewPadding, Name.Frame.Bottom );

            // set the prayer text, allow multiple lines, set the width to be the card itself,
            // and let SizeToFit measure the height.
            PrayerText.Text = prayer.Text;

            PrayerText.Frame = new CGRect( ViewPadding, Date.Frame.Bottom + ViewPadding, View.Bounds.Width - (ViewPadding * 2), 0 );
            PrayerText.SizeToFit( );
            float prayerHeight = (float) Math.Min( PrayerText.Frame.Height, View.Bounds.Height - PrayerText.Frame.Top - Pray.Frame.Height - ViewPadding );
            PrayerText.Frame = new CGRect( PrayerText.Frame.Left, PrayerText.Frame.Top, PrayerText.Frame.Width, prayerHeight );
        }
    }

	partial class PrayerMainUIViewController : TaskUIViewController
	{
        /// <summary>
        /// Actual list of prayer requests
        /// </summary>
        /// <value>The prayer requests.</value>
        List<PrayerCard> PrayerRequests { get; set; }

        PlatformCardCarousel Carousel { get; set; }

        bool RequestingPrayers { get; set; }
        bool ViewActive { get; set; }

        CGRect CardSize { get; set; }

        Rock.Mobile.PlatformSpecific.iOS.UI.BlockerView BlockerView { get; set; }

		public PrayerMainUIViewController (IntPtr handle) : base (handle)
		{
            PrayerRequests = new List<PrayerCard>();
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new Rock.Mobile.PlatformSpecific.iOS.UI.BlockerView( View.Frame );

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            float viewRealHeight = (float)( View.Bounds.Height - Task.NavToolbar.Frame.Height);

            float cardSizePerc = .83f;
            float cardWidth = (float)(View.Bounds.Width * cardSizePerc);
            float cardHeight = (float) (viewRealHeight * cardSizePerc);

            // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
            float cardYOffset = ( viewRealHeight * .03f );

            Carousel = PlatformCardCarousel.Create( View, cardWidth, cardHeight, new System.Drawing.RectangleF( 0, cardYOffset, (float)View.Bounds.Width, viewRealHeight ), PrayerConfig.Card_AnimationDuration );

            CardSize = new CGRect( 0, 0, cardWidth, cardHeight );

            // Setup the request prayers layer
            //setup our appearance
            RetrievingPrayersView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_Retrieving;
            ControlStyling.StyleUILabel( StatusLabel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( StatusBackground );

            ControlStyling.StyleUILabel( ResultLabel, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( ResultBackground );

            View.AddSubview( BlockerView );

            ControlStyling.StyleButton( RetryButton, GeneralStrings.Retry, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            RetryButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                RetrievePrayerRequests( );
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ViewActive = true;
            Carousel.Hidden = false;

            Task.NavToolbar.SetCreateButtonEnabled( false );

            View.BringSubviewToFront( RetrievingPrayersView );
            View.BringSubviewToFront( BlockerView );

            // this will prevent double requests in the case that we leave and return to the prayer
            // page before the initial request completes
            if ( RequestingPrayers == false )
            {
                RetrievePrayerRequests( );
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        void RetrievePrayerRequests( )
        {
            // show the retrieve layer
            RetrievingPrayersView.Layer.Opacity = 1.00f;
            StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_Retrieving;
            ResultLabel.Hidden = true;
            RetryButton.Hidden = true;

            BlockerView.FadeIn( delegate
                {
                    RequestingPrayers = true;

                    Task.NavToolbar.SetCreateButtonEnabled( true, delegate
                        {
                            // now disable the button so they can't spam it
                            Task.NavToolbar.SetCreateButtonEnabled( false );

                            Prayer_CreateUIViewController viewController = Storyboard.InstantiateViewController( "Prayer_CreateUIViewController" ) as Prayer_CreateUIViewController;
                            Task.PerformSegue( this, viewController );

                            Console.WriteLine( "pushing" );
                        }
                    );

                    // request the prayers each time this appears
                    CCVApp.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests )
                        {
                            // force this onto the main thread so that if there's a race condition in requesting prayers we won't hit it.
                            InvokeOnMainThread( delegate
                                {
                                    // only process this if the view is still active. It's possible this request came in after we left the view.
                                    if ( ViewActive == true )
                                    {
                                        PrayerRequests.Clear( );
                                        Carousel.Clear( );

                                        RequestingPrayers = false;

                                        BlockerView.FadeOut( null );

                                        // somestimes our prayers can be received with errors in the xml, so ensure we have a valid model.
                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) && prayerRequests != null )
                                        {
                                            if ( prayerRequests.Count > 0 )
                                            {
                                                RetrievingPrayersView.Layer.Opacity = 0.00f;

                                                // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                                                foreach( Rock.Client.PrayerRequest prayer in prayerRequests )
                                                {
                                                    PrayerCard card = new PrayerCard( prayer, CardSize );
                                                    PrayerRequests.Add( card );
                                                    Carousel.AddCard( card.View );
                                                }

                                                // add a read analytic
                                                PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Read );
                                            }
                                        }
                                        else
                                        {
                                            StatusLabel.Text = PrayerStrings.ViewPrayer_StatusText_Failed;
                                            RetryButton.Hidden = false;
                                            ResultLabel.Hidden = false;
                                            ResultLabel.Text = PrayerStrings.Error_Retrieve_Message;

                                            Task.NavToolbar.SetCreateButtonEnabled( false );
                                        }
                                    }
                                } );
                        } );
                } );
        }

        public void MakeInActive()
        {
            ViewActive = false;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan( touches, evt );

            Carousel.TouchesBegan( );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // don't call the base because we don't want to support the nav toolbar on this page
            Carousel.TouchesEnded( );
        }
	}
}
