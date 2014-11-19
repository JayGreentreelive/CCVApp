using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreAnimation;
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;

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

        PlatformView View { get; set; }
        UILabel Name { get; set; }
        UILabel Date { get; set; }
        UILabel Category { get; set; }
        PrayerTextView PrayerText { get; set; }
        UIButton Pray { get; set; }
        UIView PrayFillIn { get; set; }

        Rock.Client.PrayerRequest PrayerRequest { get; set; }

        public PrayerCard( ref PlatformView cardView, RectangleF bounds )
        {
            //setup the actual "card" outline
            View = PlatformView.Create( );
            View.Bounds = bounds;
            View.BackgroundColor = PrayerConfig.Card_BackgroundColor;
            View.BorderColor = PrayerConfig.Card_BorderColor;
            View.CornerRadius = PrayerConfig.Card_CornerRadius;
            View.BorderWidth = PrayerConfig.Card_BorderWidth;


            // setup the prayer request text field
            PrayerText = new PrayerTextView( );
            PrayerText.Editable = false;
            PrayerText.BackgroundColor = UIColor.Clear;
            PrayerText.Layer.AnchorPoint = new PointF( 0, 0 );
            PrayerText.DelaysContentTouches = false; // don't allow delaying touch, we need to forward it
            PrayerText.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.Card_PrayerColor );
            PrayerText.Font = iOSCommon.LoadFontDynamic( PrayerConfig.Card_PrayerFont, PrayerConfig.Card_PrayerSize );
            PrayerText.TextContainerInset = UIEdgeInsets.Zero;
            PrayerText.TextContainer.LineFragmentPadding = 0;

            // setup the bottom prayer button, and its fill-in circle
            Pray = UIButton.FromType( UIButtonType.Custom );
            Pray.Layer.AnchorPoint = new PointF( 0, 0 );
            Pray.SetTitle( PrayerStrings.Prayer_Confirm, UIControlState.Normal );
            Pray.SetTitleColor( PlatformBaseUI.GetUIColor( PrayerConfig.Card_ButtonColor_Normal ), UIControlState.Normal );
            Pray.SetTitleColor( PlatformBaseUI.GetUIColor( PrayerConfig.Card_ButtonColor_Highlighted ), UIControlState.Highlighted );
            Pray.Font = iOSCommon.LoadFontDynamic( PrayerConfig.Card_ButtonFont, PrayerConfig.Card_ButtonSize );
            Pray.SizeToFit( );

            PrayFillIn = new UIView( );
            PrayFillIn.Bounds = new RectangleF( 0, 0, Pray.Frame.Height / 2, Pray.Frame.Height / 2 );
            PrayFillIn.Layer.Position = new PointF( View.Bounds.Width - PrayFillIn.Layer.Bounds.Width - ViewPadding, View.Bounds.Height - PrayFillIn.Layer.Bounds.Height - (PrayFillIn.Layer.Bounds.Height / 2) );
            PrayFillIn.Layer.CornerRadius = PrayFillIn.Bounds.Width / 2;
            PrayFillIn.Layer.BorderWidth = 1;
            PrayFillIn.Layer.BorderColor = PlatformBaseUI.GetUIColor( PrayerConfig.Card_PrayerColor ).CGColor;
            PrayFillIn.Layer.AnchorPoint = new PointF( 0, 0 );

            Pray.Layer.Position = new PointF( PrayFillIn.Frame.Left - Pray.Layer.Bounds.Width - ViewPadding, View.Bounds.Height - Pray.Layer.Bounds.Height );
            Pray.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // Hack - Use FlagCount to track whether we've prayed for this or not
                    if ( PrayerRequest != null )
                    {
                        PrayerRequest.FlagCount = PrayerRequest.FlagCount == 1 ? 0 : 1;

                        TogglePrayerFillIn( );
                    }
                };


            // setup the name field
            Name = new UILabel( );
            Name.Layer.AnchorPoint = new PointF( 0, 0 );
            Name.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.Card_NameColor );
            Name.Font = iOSCommon.LoadFontDynamic( PrayerConfig.Card_NameFont, PrayerConfig.Card_NameSize );

            // setup the date field
            Date = new UILabel( );
            Date.Layer.AnchorPoint = new PointF( 0, 0 );
            Date.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.Card_DateColor );
            Date.Font = iOSCommon.LoadFontDynamic( PrayerConfig.Card_DateFont, PrayerConfig.Card_DateSize );


            // setup the category field
            Category = new UILabel( );
            Category.Layer.AnchorPoint = new PointF( 0, 0 );
            Category.TextColor = PlatformBaseUI.GetUIColor( PrayerConfig.Card_CategoryColor );
            Category.Font = iOSCommon.LoadFontDynamic( PrayerConfig.Card_CategoryFont, PrayerConfig.Card_CategorySize );


            // add the controls
            UIView nativeView = View.PlatformNativeObject as UIView;

            nativeView.AddSubview( Name );
            nativeView.AddSubview( Category );
            nativeView.AddSubview( Date );
            nativeView.AddSubview( PrayerText );
            nativeView.AddSubview( Pray );
            nativeView.AddSubview( PrayFillIn );
            PrayerText.Parent = nativeView;

            cardView = View;
        }

        const int ViewPadding = 5;
        public void SetPrayer( Rock.Client.PrayerRequest prayer )
        {
            PrayerRequest = prayer;

            // Hack - Use FlagCount to track whether we've prayed for this or not
            if ( PrayerRequest.FlagCount == null )
            {
                PrayerRequest.FlagCount = 0;
            }

            // set the text for the name, size it so we get the height, then
            // restrict its bounds to the card itself
            Name.Text = prayer.FirstName;
            Name.SizeToFit( );
            Name.Frame = new RectangleF( ViewPadding, ViewPadding, View.Bounds.Width - (ViewPadding * 2), Name.Bounds.Height );

            Category.Text = "Category";
            Category.SizeToFit( );
            Category.Layer.Position = new PointF( ViewPadding, Name.Frame.Bottom );

            // set the date text, then calculate the dimensions so we can right-adjust it
            Date.Text = string.Format( "{0:MM/dd/yy}", prayer.CreatedDateTime );
            Date.Frame = new RectangleF( ViewPadding, Name.Frame.Bottom, View.Bounds.Width - ViewPadding, 0 );
            Date.SizeToFit( );
            Date.Layer.Position = new PointF( View.Bounds.Width - Date.Frame.Width - ViewPadding, Name.Frame.Bottom );

            // set the prayer text, allow multiple lines, set the width to be the card itself,
            // and let SizeToFit measure the height.
            PrayerText.Text = prayer.Text;

            PrayerText.Frame = new RectangleF( ViewPadding, Date.Frame.Bottom + ViewPadding, View.Bounds.Width - (ViewPadding * 2), 0 );
            PrayerText.SizeToFit( );
            float prayerHeight = Math.Min( PrayerText.Frame.Height, View.Bounds.Height - PrayerText.Frame.Top - Pray.Frame.Height - ViewPadding );
            PrayerText.Frame = new RectangleF( PrayerText.Frame.Left, PrayerText.Frame.Top, PrayerText.Frame.Width, prayerHeight );

            // set the prayer fill in appropriately
            TogglePrayerFillIn( );
        }

        void TogglePrayerFillIn( )
        {
            // Hack - Use FlagCount to track whether we've prayed for this or not
            if( PrayerRequest.FlagCount != 0 )
            {
                PrayFillIn.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.Card_PrayerColor );
            }
            else
            {
                PrayFillIn.BackgroundColor = UIColor.Clear;
            }
        }
    }

	partial class PrayerMainUIViewController : TaskUIViewController
	{
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

        bool RequestingPrayers { get; set; }
        bool ViewActive { get; set; }

        UIButton AddPrayer { get; set; }

		public PrayerMainUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Setup the "AddPrayer" button for creating a new prayer request
            AddPrayer = UIButton.FromType( UIButtonType.Custom );
            AddPrayer.Layer.AnchorPoint = new PointF( 0, 0 );
            AddPrayer.SetTitle( PrayerConfig.AddPrayer_Text, UIControlState.Normal );
            AddPrayer.SetTitleColor( PlatformBaseUI.GetUIColor( PrayerConfig.AddPrayer_ButtonColor_Normal ), UIControlState.Normal );
            AddPrayer.SetTitleColor( PlatformBaseUI.GetUIColor( PrayerConfig.AddPrayer_ButtonColor_Highlighted ), UIControlState.Highlighted );
            AddPrayer.Font = iOSCommon.LoadFontDynamic( PrayerConfig.AddPrayer_Font, PrayerConfig.AddPrayer_Size );
            AddPrayer.Layer.BorderColor = PlatformBaseUI.GetUIColor( PrayerConfig.AddPrayer_BorderColor ).CGColor;
            AddPrayer.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.AddPrayer_BackgroundColor );
            AddPrayer.Layer.CornerRadius = PrayerConfig.AddPrayer_CornerRadius;
            AddPrayer.Layer.BorderWidth = PrayerConfig.AddPrayer_BorderWidth;
            AddPrayer.SizeToFit( );
            AddPrayer.Bounds = new RectangleF( 0, 0, AddPrayer.Bounds.Width * 2, AddPrayer.Bounds.Height );
            AddPrayer.Layer.Position = new PointF( ( View.Frame.Width - AddPrayer.Frame.Width ) / 2, View.Frame.Height - AddPrayer.Frame.Height + 4 );
            AddPrayer.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    Prayer_CreateUIViewController viewController = Storyboard.InstantiateViewController( "Prayer_CreateUIViewController" ) as Prayer_CreateUIViewController;
                    Task.PerformSegue( this, viewController );
                };
            View.AddSubview( AddPrayer );


            View.BackgroundColor = PlatformBaseUI.GetUIColor( PrayerConfig.BackgroundColor );

            float viewRealHeight = ( View.Bounds.Height - NavigationController.NavigationBar.Bounds.Height );

            float cardSizePerc = .83f;
            float cardWidth = View.Bounds.Width * cardSizePerc;
            float cardHeight = viewRealHeight * cardSizePerc;

            // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
            float cardYOffset = ( viewRealHeight * .05f ) + NavigationController.NavigationBar.Bounds.Height;

            Carousel = PlatformCardCarousel.Create( cardWidth, cardHeight, new RectangleF( 0, cardYOffset, View.Bounds.Width, viewRealHeight ), PrayerConfig.Card_AnimationDuration, UpdatePrayerCards );

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

            ViewActive = true;

            Carousel.ViewWillAppear( animated );

            ActivityIndicator.Hidden = false;

            Task.NavToolbar.SetCreateButtonEnabled( false );
            AddPrayer.Enabled = false;

            // this will prevent double requests in the case that we leave and return to the prayer
            // page before the initial request completes
            if ( RequestingPrayers == false )
            {
                RequestingPrayers = true;

                // request the prayers each time this appears
                CCVApp.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests )
                    {
                        // force this onto the main thread so that if there's a race condition in requesting prayers we won't hit it.
                        InvokeOnMainThread( delegate
                            {
                                // only process this if the view is still active. It's possible this request came in after we left the view.
                                if( ViewActive == true )
                                {
                                    RequestingPrayers = false;

                                    ActivityIndicator.Hidden = true;

                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                    {
                                        if ( prayerRequests.Count > 0 )
                                        {
                                            /*Task.NavToolbar.SetCreateButtonEnabled( true, delegate
                                                {
                                                    Prayer_CreateUIViewController viewController = Storyboard.InstantiateViewController( "Prayer_CreateUIViewController" ) as Prayer_CreateUIViewController;
                                                    Task.PerformSegue( this, viewController );
                                                }
                                            );*/

                                            AddPrayer.Enabled = true;

                                            PrayerRequests = prayerRequests;

                                            Carousel.NumItems = PrayerRequests.Count;

                                            UpdatePrayerCards( 0 );
                                        }
                                    }
                                    else
                                    {
                                        SpringboardViewController.DisplayError( PrayerStrings.Error_Title, PrayerStrings.Error_Retrieve_Message );
                                    }
                                }
                            });
                    } );
            }
        }

        public void MakeInActive()
        {
            ViewActive = false;
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
