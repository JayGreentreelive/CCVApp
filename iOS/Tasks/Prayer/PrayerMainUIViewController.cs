using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreAnimation;

namespace iOS
{
	partial class PrayerMainUIViewController : TaskUIViewController
	{
        class PrayerCard
        {
            public UIView View { get; set; }
            UILabel Name { get; set; }
            UILabel Prayer { get; set; }

            public PrayerCard( RectangleF bounds )
            {
                View = new UIView( bounds );
                Name = new UILabel( );
                Prayer = new UILabel( );

                // set anchor points to the left corner for simple positioning
                Name.Layer.AnchorPoint = new PointF( 0, 0 );
                Prayer.Layer.AnchorPoint = new PointF( 0, 0 );
                View.Layer.AnchorPoint = new PointF( 0, 0 );

                // don't consume the interactions
                View.UserInteractionEnabled = false;

                // set the text color
                Name.TextColor = UIColor.White;
                Prayer.TextColor = UIColor.White;

                // set the outline for the card
                View.Layer.BorderColor = UIColor.Gray.CGColor;
                View.Layer.CornerRadius = 4;
                View.Layer.BorderWidth = 1;

                // add the controls
                View.AddSubview( Name );
                View.AddSubview( Prayer );
            }

            const int ViewPadding = 10;

            public void SetPrayer( string name, string prayer )
            {
                // set the text for the name, size it so we get the height, then
                // restrict its bounds to the card itself
                Name.Text = name;
                Name.SizeToFit( );
                Name.Frame = new RectangleF( ViewPadding, ViewPadding, View.Bounds.Width - (ViewPadding * 2), Name.Bounds.Height );

                // set the prayer text, allow multiple lines, set the width to be the card itself,
                // and let SizeToFit measure the height.
                Prayer.Text = prayer;
                Prayer.Lines = 99;
                Prayer.Frame = new RectangleF( ViewPadding, Name.Frame.Bottom, View.Bounds.Width - (ViewPadding * 2), 0 );
                Prayer.SizeToFit( );
            }
        }

        // Create 5 cards so that we're guaranteed to always have cards visible on screen
        PrayerCard SubLeftCard { get; set; }
        PrayerCard LeftCard { get; set; }
        PrayerCard CenterCard { get; set; }
        PrayerCard RightCard { get; set; }
        PrayerCard PostRightCard { get; set; }

        // the default positions for each card
        PointF SubLeftPos { get; set; }
        PointF LeftPos { get; set; }
        PointF CenterPos { get; set; }
        PointF RightPos { get; set; }
        PointF PostRightPos { get; set; }

        /// <summary>
        /// Tracks the last position of panning so delta can be applied
        /// </summary>
        PointF PanLastPos { get; set; }

        /// <summary>
        /// Direction we're currently panning. Important for syncing the card positions
        /// </summary>
        int PanDir { get; set; }

        /// <summary>
        /// Actual list of prayer requests
        /// </summary>
        /// <value>The prayer requests.</value>
        List<Rock.Client.PrayerRequest> PrayerRequests { get; set; }

        /// <summary>
        /// The prayer currently being viewed (always the center card)
        /// </summary>
        int ViewingIndex { get; set; }

        /// <summary>
        /// True when an animation to restore card positions is playing.
        /// Needed so we know when to allow "fast" panning.
        /// </summary>
        bool Animating = false;

        class PrayerAnimDelegate : CAAnimationDelegate
        {
            public PrayerMainUIViewController Parent { get; set; }

            public override void AnimationStarted(CAAnimation anim)
            {

            }

            public override void AnimationStopped(CAAnimation anim, bool finished)
            {
                Parent.AnimationStopped( anim, finished );
            }
        }

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
            float cardYOffset = ((viewRealHeight - cardHeight) / 2) + NavigationController.NavigationBar.Bounds.Height;

            // the center position should be center on screen
            CenterPos = new PointF( ((View.Bounds.Width - cardWidth) / 2), cardYOffset );

            // left should be exactly one screen width to the left, and right one screen width to the right
            LeftPos = new PointF( CenterPos.X - View.Bounds.Width, cardYOffset );
            RightPos = new PointF( CenterPos.X + View.Bounds.Width, cardYOffset );

            // sub left and post right should be two screens to the left / right of center
            SubLeftPos = new PointF( LeftPos.X - View.Bounds.Width, cardYOffset );
            PostRightPos = new PointF( RightPos.X + View.Bounds.Width, cardYOffset );

            // create our cards
            SubLeftCard = new PrayerCard( new RectangleF( 0, 0, cardWidth, cardHeight ) );
            LeftCard = new PrayerCard( new RectangleF( 0, 0, cardWidth, cardHeight ) );
            CenterCard = new PrayerCard( new RectangleF( 0, 0, cardWidth, cardHeight ) );
            RightCard = new PrayerCard( new RectangleF( 0, 0, cardWidth, cardHeight ) );
            PostRightCard = new PrayerCard( new RectangleF( 0, 0, cardWidth, cardHeight ) );

            // default the initial position of the cards
            SubLeftCard.View.Layer.Position = SubLeftPos;
            LeftCard.View.Layer.Position = LeftPos;
            CenterCard.View.Layer.Position = CenterPos;
            RightCard.View.Layer.Position = RightPos;
            PostRightCard.View.Layer.Position = PostRightPos;

            // setup our pan gesture
            UIPanGestureRecognizer panGesture = new UIPanGestureRecognizer( OnPanGesture );
            panGesture.MinimumNumberOfTouches = 1;
            panGesture.MaximumNumberOfTouches = 1;

            // add the gesture and all cards to our view
            View.AddGestureRecognizer( panGesture );
            View.AddSubview( SubLeftCard.View );
            View.AddSubview( LeftCard.View );
            View.AddSubview( CenterCard.View );
            View.AddSubview( RightCard.View );
            View.AddSubview( PostRightCard.View );

            // hide the actiivty indicator and make sure it is front and center
            ActivityIndicator.Hidden = false;
            View.BringSubviewToFront( ActivityIndicator );

            CreatePrayerButton.TouchUpInside += delegate(object sender, EventArgs e) 
                {
                    Prayer_CreateUIViewController viewController = Storyboard.InstantiateViewController( "Prayer_CreateUIViewController" ) as Prayer_CreateUIViewController;
                    Task.PerformSegue( this, viewController );
                };

            PrayButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    //todo: update prayer count
                };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ViewingIndex = 0;

            ActivityIndicator.Hidden = false;

            CreatePrayerButton.Enabled = false;
            PrayButton.Enabled = false;

            // request the prayers each time this appears
            CCVApp.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests) 
                {
                    ActivityIndicator.Hidden = true;

                    if( prayerRequests.Count > 0 )
                    {
                        CreatePrayerButton.Enabled = true;
                        PrayButton.Enabled = true;

                        PrayerRequests = prayerRequests;

                        UpdatePrayerCards( ViewingIndex );
                    }
                });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            CreatePrayerButton.Enabled = false;
            PrayButton.Enabled = false;
        }

        void OnPanGesture(UIPanGestureRecognizer obj) 
        {
            switch( obj.State )
            {
                case UIGestureRecognizerState.Began:
                {
                    // when panning begins, clear our pan values
                    PanLastPos = new PointF( 0, 0 );
                    PanDir = 0;
                    break;
                }

                case UIGestureRecognizerState.Changed:
                {
                    // use the velocity to determine the direction of the pan
                    PointF currVelocity = obj.VelocityInView( View );
                    if( currVelocity.X < 0 )
                    {
                        PanDir = -1;
                    }
                    else
                    {
                        PanDir = 1;
                    }

                    // Update the positions of the cards
                    PointF absPan = obj.TranslationInView( View );
                    PointF delta = new PointF( absPan.X - PanLastPos.X, 0 );
                    PanLastPos = absPan;

                    TryPanCards( delta );

                    // sync the positions, which will adjust cards as they scroll so the center
                    // remains in the center (it's a fake infinite list)
                    SyncCardPositionsForPan( );

                    // ensure the cards don't move beyond their boundaries
                    ClampCards( );
                    break;
                }

                case UIGestureRecognizerState.Ended:
                {
                    // when panning is complete, restore the cards to their natural positions
                    AnimateCardsToNeutral( );
                    break;
                }
            }
        }

        void ClampCards( )
        {
            // don't allow the left or right cards to move if 
            // we're at the edge of the list.
            if( ViewingIndex - 2 < 0 )
            {
                SubLeftCard.View.Layer.Position = SubLeftPos;
            }

            if( ViewingIndex - 1 < 0 )
            {
                LeftCard.View.Layer.Position = LeftPos;
            }

            if( ViewingIndex + 1 >= PrayerRequests.Count )
            {
                RightCard.View.Layer.Position = RightPos;
            }

            if( ViewingIndex + 2 >= PrayerRequests.Count )
            {
                PostRightCard.View.Layer.Position = PostRightPos;
            }
        }

        void TryPanCards( PointF panPos )
        {
            // adjust all the cards by the amount panned (this should be a delta value)
            if( ViewingIndex - 2 >= 0 )
            {
                SubLeftCard.View.Layer.Position = new PointF( SubLeftCard.View.Layer.Position.X + panPos.X, LeftPos.Y );
            }

            if( ViewingIndex - 1 >= 0 )
            {
                LeftCard.View.Layer.Position = new PointF( LeftCard.View.Layer.Position.X + panPos.X, LeftPos.Y );
            }

            CenterCard.View.Layer.Position = new PointF( CenterCard.View.Layer.Position.X + panPos.X, CenterPos.Y );

            if( ViewingIndex + 1 < PrayerRequests.Count )
            {
                RightCard.View.Layer.Position = new PointF( RightCard.View.Layer.Position.X + panPos.X, RightPos.Y );
            }

            if( ViewingIndex + 2 < PrayerRequests.Count )
            {
                PostRightCard.View.Layer.Position = new PointF( PostRightCard.View.Layer.Position.X + panPos.X, RightPos.Y );
            }
        }

        /// <summary>
        /// Only called if the user didn't pan. Used primarly to detect
        /// the user tapping DURING an animation so we can pause the card movement.
        /// </summary>
        /// <param name="touches">Touches.</param>
        /// <param name="evt">Evt.</param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            // when touch begins, remove all animations
            SubLeftCard.View.Layer.RemoveAllAnimations();
            LeftCard.View.Layer.RemoveAllAnimations();
            CenterCard.View.Layer.RemoveAllAnimations();
            RightCard.View.Layer.RemoveAllAnimations();
            PostRightCard.View.Layer.RemoveAllAnimations();

            // and commit the animated positions as the actual card positions.
            SubLeftCard.View.Layer.Position = SubLeftCard.View.Layer.PresentationLayer.Position;
            LeftCard.View.Layer.Position = LeftCard.View.Layer.PresentationLayer.Position;
            CenterCard.View.Layer.Position = CenterCard.View.Layer.PresentationLayer.Position;
            RightCard.View.Layer.Position = RightCard.View.Layer.PresentationLayer.Position;
            PostRightCard.View.Layer.Position = PostRightCard.View.Layer.PresentationLayer.Position;

            // this has the effect of freezing & stopping the animation in motion.
            // OnAnimationEnded will be called, but finished will be false, so
            // we'll know it was stopped manually
            Console.WriteLine( "Touches Began" );
        }

        /// <summary>
        /// Only called if the user didn't pan. Used primarly to detect
        /// which direction to resume the cards if the user touched and
        /// released without panning.
        /// </summary>
        /// <param name="touches">Touches.</param>
        /// <param name="evt">Evt.</param>
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // Attempt to restore the cards to their natural position. This
            // will NOT be called if the user invoked the pan gesture. (which is a good thing)
            AnimateCardsToNeutral( );

            Console.WriteLine( "Touches Ended" );
        }

        /// <summary>
        /// Animates a card from startPos to endPos over time
        /// </summary>
        const float AnimDuration = .25f;
        void AnimateCard( UIView cardView, string animName, PointF startPos, PointF endPos, float duration, UIViewController parentDelegate )
        {
            CABasicAnimation cardAnim = CABasicAnimation.FromKeyPath( "position" );

            cardAnim.From = NSValue.FromPointF( startPos );
            cardAnim.To = NSValue.FromPointF( endPos );

            cardAnim.Duration = duration;
            cardAnim.TimingFunction = CAMediaTimingFunction.FromName( CAMediaTimingFunction.EaseInEaseOut );

            // these ensure we maintain the card position when finished
            cardAnim.FillMode = CAFillMode.Forwards;
            cardAnim.RemovedOnCompletion = false;

            // if a delegate was provided, give it to the card
            if( parentDelegate != null )
            {
                cardAnim.Delegate = new PrayerAnimDelegate() { Parent = this };
            }

            // 
            cardView.Layer.AddAnimation( cardAnim, animName );
        }

        /// <summary>
        /// Called when card movement is complete.
        /// </summary>
        /// <param name="anim">Animation.</param>
        /// <param name="finished">If set to <c>true</c> finished.</param>
        void AnimationStopped( CAAnimation anim, bool finished )
        {
            // the only thing we realy want to do is update the carousel so
            // that the cards are once again L C R and the prayer indices are -1, 0, 1
            if( finished == true )
            {
                Animating = false;
                Console.WriteLine( "Animation Stopped" );
            }
        }

        /// <summary>
        /// This turns the cards into a "carousel" that will push cards forward and pull
        /// prayers back, or pull cards back and push cards forward, giving the illusion
        /// of endlessly panning thru cards.
        /// </summary>
        void SyncCardPositionsForPan( )
        {
            // see if the center of either the left or right card crosses the threshold for switching
            float deltaLeftX = CardDistFromCenter( LeftCard );
            float deltaRightX = CardDistFromCenter( RightCard );

            Console.WriteLine( "Right Delta: {0} Left Delta {1}", deltaRightX, deltaLeftX );

            // if animating is true, we want the tolerance to be MUCH higher,
            // allowing easier flicking to the next card.
            // The real world effect is that if the user flicks cards,
            // they will quickly and easily move. If the user pans on the cards,
            // it will be harder to get them to switch.
            float tolerance = (Animating == true) ? 400 : 260;

            // if we're panning LEFT, that means the right hand card might be in range to sync
            if( Math.Abs(deltaRightX) < tolerance && PanDir == -1)
            {
                if( ViewingIndex + 1 < PrayerRequests.Count )
                {
                    Console.WriteLine( "Syncing Card Positions Right" );

                    ViewingIndex = ViewingIndex + 1;
                    UpdatePrayerCards( ViewingIndex );

                    // reset the card positions, creating the illusion that the cards really moved
                    SubLeftCard.View.Layer.Position = new PointF( deltaRightX + SubLeftPos.X, LeftPos.Y );
                    LeftCard.View.Layer.Position = new PointF( deltaRightX + LeftPos.X, LeftPos.Y );
                    CenterCard.View.Layer.Position = new PointF( deltaRightX + CenterPos.X, CenterPos.Y );
                    RightCard.View.Layer.Position = new PointF( deltaRightX + RightPos.X, RightPos.Y );
                    PostRightCard.View.Layer.Position = new PointF( deltaRightX + PostRightPos.X, RightPos.Y );
                }
            }
            // if we're panning RIGHT, that means the left hand card might be in range to sync
            else if( Math.Abs( deltaLeftX ) < tolerance && PanDir == 1)
            {
                if( ViewingIndex - 1 >= 0 )
                {
                    Console.WriteLine( "Syncing Card Positions Left" );

                    ViewingIndex = ViewingIndex - 1;
                    UpdatePrayerCards( ViewingIndex );

                    // reset the card positions, creating the illusion that the cards really moved
                    SubLeftCard.View.Layer.Position = new PointF( deltaLeftX + SubLeftPos.X, LeftPos.Y );
                    LeftCard.View.Layer.Position = new PointF( deltaLeftX + LeftPos.X, LeftPos.Y );
                    CenterCard.View.Layer.Position = new PointF( deltaLeftX + CenterPos.X, CenterPos.Y );
                    RightCard.View.Layer.Position = new PointF( deltaLeftX + RightPos.X, RightPos.Y );
                    PostRightCard.View.Layer.Position = new PointF( deltaLeftX + PostRightPos.X, RightPos.Y );
                }
            }
        }

        /// <summary>
        /// Helper to get the distance from the center
        /// </summary>
        /// <returns>The dist from center.</returns>
        float CardDistFromCenter( PrayerCard card )
        {
            float cardHalfWidth = CenterCard.View.Bounds.Width / 2;
            return ( card.View.Layer.Position.X + cardHalfWidth ) - (CenterPos.X + cardHalfWidth);
        }

        void AnimateCardsToNeutral( )
        {
            // this will animate each card to its neutral resting point
            Animating = true;
            AnimateCard( SubLeftCard.View, "SubLeftCard", SubLeftCard.View.Layer.Position, SubLeftPos, AnimDuration, null );
            AnimateCard( LeftCard.View, "LeftCard", LeftCard.View.Layer.Position, LeftPos, AnimDuration, null );
            AnimateCard( CenterCard.View, "CenterCard", CenterCard.View.Layer.Position, CenterPos, AnimDuration, this );
            AnimateCard( RightCard.View, "RightCard", RightCard.View.Layer.Position, RightPos, AnimDuration, null );
            AnimateCard( PostRightCard.View, "PostRightCard", PostRightCard.View.Layer.Position, PostRightPos, AnimDuration, null );
        }

        void UpdatePrayerCards( int prayerIndex )
        {
            // grab the prayer request
            Rock.Client.PrayerRequest request = PrayerRequests[ ViewingIndex ];

            CenterCard.SetPrayer( request.FirstName, request.Text );

            // set the left and right in case they want to swipe thru the prayers
            if( ViewingIndex - 2 >= 0 )
            {
                request = PrayerRequests[ ViewingIndex - 2 ];
                SubLeftCard.SetPrayer( request.FirstName, request.Text );
            }

            if( ViewingIndex - 1 >= 0 )
            {
                request = PrayerRequests[ ViewingIndex - 1 ];
                LeftCard.SetPrayer( request.FirstName, request.Text );
            }

            if( ViewingIndex + 1 < PrayerRequests.Count )
            {
                request = PrayerRequests[ ViewingIndex + 1 ];
                RightCard.SetPrayer( request.FirstName, request.Text );
            }

            if( ViewingIndex + 2 < PrayerRequests.Count )
            {
                request = PrayerRequests[ ViewingIndex + 2 ];
                PostRightCard.SetPrayer( request.FirstName, request.Text );
            }
        }
	}
}
