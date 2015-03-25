using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using Rock.Mobile.PlatformSpecific.iOS.Graphics;

namespace iOS
{
    /// <summary>
    /// The entire app lives underneath a main navigation bar. This is the control
    /// that drives that navigation bar and manages sliding in and out to reveal the springboard.
    /// </summary>
	partial class MainUINavigationController : UINavigationController
	{
        /// <summary>
        /// Flag determining whether the springboard is revealed. Revealed means
        /// this view controller has been slid over to show the springboard.
        /// </summary>
        /// <value><c>true</c> if springboard revealed; otherwise, <c>false</c>.</value>
        protected bool SpringboardRevealed { get; set; }

        /// <summary>
        /// True when this view controller is in the process of moving.
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        protected bool Animating { get; set; }

        /// <summary>
        /// The view controller that actually contains the active content.
        /// </summary>
        /// <value>The container.</value>
        protected ContainerViewController Container { get; set; }

        protected UIPanGestureRecognizer PanGesture { get; set; }

        public SpringboardViewController ParentSpringboard { get; set; }

        /// <summary>
        /// Tracks the last position of panning so delta can be applied
        /// </summary>
        protected CGPoint PanLastPos { get; set; }

        /// <summary>
        /// Direction we're currently panning. Important for syncing the card positions
        /// </summary>
        protected int PanDir { get; set; }

        /// <summary>
        /// A wrapper for Container.CurrentTask, since Container is protected.
        /// </summary>
        /// <value>The current task.</value>
        public Task CurrentTask { get { return Container != null ? Container.CurrentTask : null; } }

        protected UIView DarkPanel { get; set; }

		public MainUINavigationController (IntPtr handle) : base (handle)
		{
        }

        /// <summary>
        /// Determines whether the springboard is fully closed.
        /// If its state is open OR animation is going on, consider it is not closed.
        /// DO NOT USE THE INVERSE TO KNOW ITS OPEN. USE IsSpringboardOpen()
        /// </summary>
        public bool IsSpringboardClosed( )
        {
            if( SpringboardRevealed == false && Animating == false )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the springboard is fully open.
        /// If its state is closed OR animation is going on, consider it not open.
        /// DO NOT USE THE INVERSE TO KNOW ITS CLOSED. USE IsSpringboardClosed()
        /// </summary>
        public bool IsSpringboardOpen( )
        {
            if ( SpringboardRevealed == true && Animating == false )
            {
                return true;
            }

            return false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // MainNavigationController must have a black background so that the ticks
            // before the task displays don't cause a flash
            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            DarkPanel = new UIView(View.Frame);
            DarkPanel.Layer.Opacity = 0.0f;
            DarkPanel.BackgroundColor = UIColor.Black;
            View.AddSubview( DarkPanel );

            // setup the style of the nav bar
            NavigationBar.TintColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );


            UIImage solidColor = new UIImage();
            UIGraphics.BeginImageContext( new CGSize( 1, 1 ) );
            CGContext context = UIGraphics.GetCurrentContext( );

            context.SetFillColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.PrimaryNavBarBackgroundColor ).CGColor );
            context.FillRect( new CGRect( 0, 0, 1, 1 ) );

            solidColor = UIGraphics.GetImageFromCurrentImageContext( );

            UIGraphics.EndImageContext( );

            NavigationBar.BarTintColor = UIColor.Clear;
            NavigationBar.SetBackgroundImage( solidColor, UIBarMetrics.Default );
            NavigationBar.Translucent = false;

            // our first (and only) child IS a ContainerViewController.
            Container = ChildViewControllers[0] as ContainerViewController;
            if( Container == null ) throw new InvalidCastException( String.Format( "MainUINavigationController's first child must be a ContainerViewController.") );

            // setup a shadow that provides depth when this panel is slid "out" from the springboard.
            UIBezierPath shadowPath = UIBezierPath.FromRect( View.Bounds );
            View.Layer.MasksToBounds = false;
            View.Layer.ShadowColor = UIColor.Black.CGColor;
            View.Layer.ShadowOffset = PrimaryContainerConfig.ShadowOffset;
            View.Layer.ShadowOpacity = PrimaryContainerConfig.ShadowOpacity;
            View.Layer.ShadowPath = shadowPath.CGPath;

            // setup our pan gesture
            PanGesture = new UIPanGestureRecognizer( OnPanGesture );
            PanGesture.MinimumNumberOfTouches = 1;
            PanGesture.MaximumNumberOfTouches = 1;
            View.AddGestureRecognizer( PanGesture );
        }

        void OnPanGesture(UIPanGestureRecognizer obj) 
        {
            switch( obj.State )
            {
                case UIGestureRecognizerState.Began:
                {
                    // when panning begins, clear our pan values
                    PanLastPos = new CGPoint( 0, 0 );
                    PanDir = 0;
                    break;
                }

                case UIGestureRecognizerState.Changed:
                {
                    // use the velocity to determine the direction of the pan
                    CGPoint currVelocity = obj.VelocityInView( View );
                    if( currVelocity.X < 0 )
                    {
                        PanDir = -1;
                    }
                    else
                    {
                        PanDir = 1;
                    }

                    // Update the positions of the cards
                    CGPoint absPan = obj.TranslationInView( View );
                    CGPoint delta = new CGPoint( absPan.X - PanLastPos.X, 0 );
                    PanLastPos = absPan;

                    TryPanSpringboard( delta );
                    break;
                }

                case UIGestureRecognizerState.Ended:
                {
                    CGPoint currVelocity = obj.VelocityInView( View );

                    float restingPoint = (float) (View.Layer.Bounds.Width / 2);
                    float currX = (float) (View.Layer.Position.X - restingPoint);

                    // if they slide at least a third of the way, allow a switch
                    float toggleThreshold = (PrimaryContainerConfig.SlideAmount / 3);

                    // check whether the springboard is open, because that changes the
                    // context of hte user's intention
                    if( SpringboardRevealed == true )
                    {
                        // since it's open, close it if it crosses the closeThreshold
                        // OR velocty is high
                        float closeThreshold = PrimaryContainerConfig.SlideAmount - toggleThreshold;
                        if( currX < closeThreshold || currVelocity.X < -1000 )
                        {
                            RevealSpringboard( false );
                        }
                        else
                        {
                            RevealSpringboard( true );
                        }
                    }
                    else
                    {
                        // since it's closed, allow it to open as long as it's beyond toggleThreshold
                        // OR velocity is high
                        if( currX > toggleThreshold || currVelocity.X > 1000 )
                        {
                            RevealSpringboard( true );
                        }
                        else
                        {
                            RevealSpringboard( false );
                        }
                    }
                    break;
                }
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            // only allow panning if the task is ok with it AND we're in portrait mode.
            if (CurrentTask.CanContainerPan( touches, evt ) == true && 
                SpringboardViewController.IsDeviceLandscape( ) == false )
            {
                PanGesture.Enabled = true;
            }
            else
            {
                PanGesture.Enabled = false;
            }
        }

        public void LayoutChanging( )
        {
            Container.LayoutChanging( );

            // should we setup the rules as if it's a split view?
            if ( SpringboardViewController.IsLandscapeRegular( ) == true )
            {
                DarkPanel.Hidden = true;

                PanGesture.Enabled = false;
            
                RevealSpringboard( true );
            }
            else
            {
                DarkPanel.Hidden = false;

                PanGesture.Enabled = true;

                RevealSpringboard( false );
            }
        }

        public void TryPanSpringboard( CGPoint delta )
        {
            // make sure the springboard is clamped
            float xPos = (float) (View.Layer.Position.X + delta.X);

            float viewHalfWidth = (float) ( View.Layer.Bounds.Width / 2 );

            xPos = Math.Max( viewHalfWidth, Math.Min( xPos, PrimaryContainerConfig.SlideAmount + viewHalfWidth ) );

            View.Layer.Position = new CGPoint( xPos, View.Layer.Position.Y );

            float percentDark = Math.Max( 0, Math.Min( (xPos - viewHalfWidth) / PrimaryContainerConfig.SlideAmount, PrimaryContainerConfig.SlideDarkenAmount ) );
            DarkPanel.Layer.Opacity = percentDark;
        }

        public void SpringboardRevealButtonTouchUp( )
        {
            // best practice states that we should let the view controller who presented us also dismiss us.
            // however, we have a unique situation where we are the parent to ALL OTHER view controllers,
            // so managing ourselves becomes a lot simpler.
            RevealSpringboard( !SpringboardRevealed );

            ParentSpringboard.RevealButtonClicked( );
        }

        public bool ActivateTask( Task task )
        {
            // don't allow switching activites while we're animating.
            if( Animating == false )
            {
                Container.ActivateTask( task );

                // I don't think this call does anything, but getting this close to
                // shipping, i don't want to remove it.
                PopToRootViewController( false );

                // task activation should only close the springboard if our device isn't wide landscape
                if( SpringboardViewController.IsLandscapeRegular( ) == false )
                {
                    RevealSpringboard( false );
                }

                return true;
            }

            return false;
        }

        public void OnActivated( )
        {
            Container.OnActivated( );
        }

        public void WillEnterForeground( )
        {
            Container.WillEnterForeground( );
        }

        public void OnResignActive( )
        {
            Container.OnResignActive( );
        }

        public void DidEnterBackground( )
        {
            Container.DidEnterBackground( );
        }

        public void WillTerminate( )
        {
            Container.WillTerminate( );
        }

        public void RevealSpringboard( bool wantReveal )
        {
            // only do something if there's a change
            //if( wantReveal != SpringboardRevealed )
            {
                // of course don't allow a change while we're animating it.
                if( Animating == false )
                {
                    Animating = true;

                    // Animate the front panel out
                    UIView.Animate( PrimaryContainerConfig.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        new Action( 
                            delegate 
                            { 
                                float endPos = 0.0f;
                                if( wantReveal == true )
                                {
                                    endPos = (float) (PrimaryContainerConfig.SlideAmount + (View.Layer.Bounds.Width / 2));
                                    DarkPanel.Layer.Opacity = PrimaryContainerConfig.SlideDarkenAmount;
                                }
                                else
                                {
                                    endPos = (float) (View.Layer.Bounds.Width / 2);
                                    DarkPanel.Layer.Opacity = 0.0f;
                                }

                                float moveAmount = (float) (endPos - View.Layer.Position.X);
                                View.Layer.Position = new CGPoint( View.Layer.Position.X + moveAmount, View.Layer.Position.Y );
                            })

                        , new Action(
                            delegate
                            {
                                Animating = false;

                                SpringboardRevealed = wantReveal;

                                // if the springboard is open, disable input on app stuff if the device doesn't support
                                // regular landscape
                                if ( SpringboardViewController.IsLandscapeRegular( ) == false )
                                {
                                    Container.View.UserInteractionEnabled = !SpringboardRevealed;
                                }
                            })
                    );
                }
            }
        }
	}
}
