using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreAnimation;
using Rock.Mobile.PlatformUI;

namespace iOS
{
	partial class PrayerMainUIViewController : TaskUIViewController
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

            PlatformView View { get; set; }
            UILabel Name { get; set; }
            UILabel Date { get; set; }
            PrayerTextView Prayer { get; set; }
            UIButton Pray { get; set; }

            public PrayerCard( ref PlatformView cardView, RectangleF bounds )
            {
                View = PlatformView.Create( );
                View.Bounds = bounds;

                Name = new UILabel( );
                Date = new UILabel( );
                Prayer = new PrayerTextView( );
                Pray = new UIButton( UIButtonType.System );

                // set anchor points to the left corner for simple positioning
                Name.Layer.AnchorPoint = new PointF( 0, 0 );
                Prayer.Layer.AnchorPoint = new PointF( 0, 0 );
                Pray.Layer.AnchorPoint = new PointF( 0, 0 );
                Date.Layer.AnchorPoint = new PointF( 0, 0 );

                Prayer.Editable = false;
                Prayer.BackgroundColor = UIColor.Clear;
                Prayer.Layer.BorderColor = UIColor.Gray.CGColor;
                Prayer.Layer.BorderWidth = 2;
                Prayer.DelaysContentTouches = false; // don't allow delaying touch, we need to forward it


                Pray.SetTitle( "Pray", UIControlState.Normal );
                Pray.SizeToFit( );

                Pray.Layer.Position = new PointF( (View.Bounds.Width - Pray.Layer.Bounds.Width) / 2, View.Bounds.Height - Pray.Layer.Bounds.Height );

                // set the text color
                Name.TextColor = UIColor.White;
                Date.TextColor = UIColor.White;
                Prayer.TextColor = UIColor.White;

                // set the outline for the card
                View.BorderColor = 0x777777FF;
                View.CornerRadius = 4;
                View.BorderWidth = 1;

                // add the controls
                UIView nativeView = View.PlatformNativeObject as UIView;

                Prayer.Parent = nativeView;

                nativeView.AddSubview( Name );
                nativeView.AddSubview( Date );
                nativeView.AddSubview( Prayer );
                nativeView.AddSubview( Pray );

                cardView = View;
            }

            const int ViewPadding = 10;
            public void SetPrayer( Rock.Client.PrayerRequest prayer )
            {
                // set the text for the name, size it so we get the height, then
                // restrict its bounds to the card itself
                Name.Text = prayer.FirstName;
                Name.SizeToFit( );
                Name.Frame = new RectangleF( ViewPadding, ViewPadding, View.Bounds.Width - (ViewPadding * 2), Name.Bounds.Height );

                // set the date text, set the width to be the card itself,
                // and let SizeToFit measure the height.
                Date.Text = string.Format( "{0:dddd MMMM d}, {2:yyyy}", prayer.CreatedDateTime, prayer.CreatedDateTime, prayer.CreatedDateTime );
                Date.Frame = new RectangleF( ViewPadding, Name.Frame.Bottom, View.Bounds.Width - (ViewPadding * 2), 0 );
                Date.SizeToFit( );

                // set the prayer text, allow multiple lines, set the width to be the card itself,
                // and let SizeToFit measure the height.
                Prayer.Text = prayer.Text;

                Prayer.SizeToFit( );
                float prayerHeight = Math.Min( Prayer.Frame.Height, View.Bounds.Height - Prayer.Frame.Top - Pray.Frame.Height - ViewPadding );
                Prayer.Frame = new RectangleF( ViewPadding, Date.Frame.Bottom, View.Bounds.Width - (ViewPadding * 2), prayerHeight );
            }
        }

        /// <summary>
        /// Actual list of prayer requests
        /// </summary>
        /// <value>The prayer requests.</value>
        List<Rock.Client.PrayerRequest> PrayerRequests { get; set; }

        PlatformCardCarousel Carousel { get; set; }

        PrayerCard SubLeftPrayer { get; set; }
        PrayerCard LeftPrayer { get; set; }
        PrayerCard CenterPrayer { get; set; }
        PrayerCard RightPrayer { get; set; }
        PrayerCard PostRightPrayer { get; set; }

		public PrayerMainUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            float viewRealHeight = ( View.Bounds.Height - NavigationController.NavigationBar.Bounds.Height );

            float cardSizePerc = .80f;
            float cardWidth = View.Bounds.Width * cardSizePerc;
            float cardHeight = viewRealHeight * cardSizePerc;

            // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
            float cardYOffset = ( viewRealHeight * .03f ) + NavigationController.NavigationBar.Bounds.Height;

            Carousel = PlatformCardCarousel.Create( cardWidth, cardHeight, new RectangleF( 0, cardYOffset, View.Bounds.Width, viewRealHeight ), UpdatePrayerCards );

            // create our cards
            SubLeftPrayer = new PrayerCard( ref Carousel.SubLeftCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
            LeftPrayer = new PrayerCard( ref Carousel.LeftCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
            CenterPrayer = new PrayerCard( ref Carousel.CenterCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
            RightPrayer = new PrayerCard( ref Carousel.RightCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
            PostRightPrayer = new PrayerCard( ref Carousel.PostRightCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );

            Carousel.Init( View );

            // hide the actiivty indicator and make sure it is front and center
            ActivityIndicator.Hidden = false;
            View.BringSubviewToFront( ActivityIndicator );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Carousel.ViewWillAppear( animated );

            ActivityIndicator.Hidden = false;

            Task.NavToolbar.SetCreateButtonEnabled( false );

            // request the prayers each time this appears
            CCVApp.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests) 
                {
                    ActivityIndicator.Hidden = true;
                    Task.NavToolbar.SetCreateButtonEnabled( true, delegate
                        {
                            Prayer_CreateUIViewController viewController = Storyboard.InstantiateViewController( "Prayer_CreateUIViewController" ) as Prayer_CreateUIViewController;
                            Task.PerformSegue( this, viewController );
                        }
                    );

                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                    {
                        if( prayerRequests.Count > 0 )
                        {
                            PrayerRequests = prayerRequests;

                            Carousel.NumItems = PrayerRequests.Count;

                            UpdatePrayerCards( 0 );
                        }
                    }
                    else
                    {
                        SpringboardViewController.DisplayError( "Prayer", "There was a problem getting prayer requests. Check your network settings and try again." );
                    }
                });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            //CreatePrayerButton.Enabled = false;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan( touches, evt );

            Console.WriteLine( "Touches Began" );

            Carousel.TouchesBegan( );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded( touches, evt );

            Console.WriteLine( "Touches Ended" );

            Carousel.TouchesEnded( );
        }

        void UpdatePrayerCards( int prayerIndex )
        {
            if( prayerIndex < PrayerRequests.Count )
            {
                CenterPrayer.SetPrayer( PrayerRequests[ prayerIndex ] );

                // set the left and right in case they want to swipe thru the prayers
                if( prayerIndex - 2 >= 0 )
                {
                    SubLeftPrayer.SetPrayer( PrayerRequests[ prayerIndex - 2 ] );
                }

                if( prayerIndex - 1 >= 0 )
                {
                    LeftPrayer.SetPrayer( PrayerRequests[ prayerIndex - 1 ] );
                }

                if( prayerIndex + 1 < PrayerRequests.Count )
                {
                    RightPrayer.SetPrayer( PrayerRequests[ prayerIndex + 1 ] );
                }

                if( prayerIndex + 2 < PrayerRequests.Count )
                {
                    PostRightPrayer.SetPrayer( PrayerRequests[ prayerIndex + 2 ] );
                }
            }
        }
	}
}
