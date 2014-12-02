using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using MonoTouch.CoreGraphics;

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

        UIPanGestureRecognizer PanGesture { get; set; }

        /// <summary>
        /// Tracks the last position of panning so delta can be applied
        /// </summary>
        PointF PanLastPos { get; set; }

        /// <summary>
        /// Direction we're currently panning. Important for syncing the card positions
        /// </summary>
        int PanDir { get; set; }

        /// <summary>
        /// A wrapper for Container.CurrentTask, since Container is protected.
        /// </summary>
        /// <value>The current task.</value>
        public Task CurrentTask { get { return Container != null ? Container.CurrentTask : null; } }

        UIView DarkPanel { get; set; }

		public MainUINavigationController (IntPtr handle) : base (handle)
		{
        }

        public void EnableSpringboardRevealButton( bool enabled )
        {
            Container.EnableSpringboardRevealButton( enabled );

            if ( enabled == false )
            {
                RevealSpringboard( false );
            }
        }

        /// <summary>
        /// Determines whether the springboard is fully closed or not.
        /// If its state is open OR animation is going on, consider it open.
        /// </summary>
        /// <returns><c>true</c> if this instance is springboard closed; otherwise, <c>false</c>.</returns>
        public bool IsSpringboardClosed( )
        {
            if( SpringboardRevealed == false && Animating == false )
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
            View.BackgroundColor = UIColor.Black;

            DarkPanel = new UIView(View.Frame);
            DarkPanel.Layer.Opacity = 0.0f;
            DarkPanel.BackgroundColor = UIColor.Black;
            View.AddSubview( DarkPanel );

            // setup the style of the nav bar
            NavigationBar.TintColor = PlatformBaseUI.GetUIColor( PrimaryNavBarConfig.RevealButton_DepressedColor );


            UIImage solidColor = new UIImage();
            UIGraphics.BeginImageContext( new SizeF( 1, 1 ) );
            CGContext context = UIGraphics.GetCurrentContext( );

            context.SetFillColorWithColor( PlatformBaseUI.GetUIColor( PrimaryNavBarConfig.BackgroundColor ).CGColor );
            context.FillRect( new RectangleF( 0, 0, 1, 1 ) );

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
            View.Layer.ShadowColor = PlatformBaseUI.GetUIColor( PrimaryContainerConfig.ShadowColor ).CGColor;
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

                    TryPanSpringboard( delta );
                    break;
                }

                case UIGestureRecognizerState.Ended:
                {
                    PointF currVelocity = obj.VelocityInView( View );

                    float restingPoint = (View.Layer.Bounds.Width / 2);
                    float currX = View.Layer.Position.X - restingPoint;

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
                UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait )
            {
                PanGesture.Enabled = true;
            }
            else
            {
                PanGesture.Enabled = false;
            }
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            UpdateSpringboardAllowedState( toInterfaceOrientation );
        }

        void UpdateSpringboardAllowedState( UIInterfaceOrientation toInterfaceOrientation )
        {
            // this catch-all will ensure we don't allow the springboard button or panning
            // when not in portrait mode.
            if ( toInterfaceOrientation == UIInterfaceOrientation.Portrait )
            {
                PanGesture.Enabled = true;
                EnableSpringboardRevealButton( true );
            }
            else
            {
                PanGesture.Enabled = false;
                EnableSpringboardRevealButton( false );
            }
        }

        public void TryPanSpringboard( PointF delta )
        {
            // make sure the springboard is clamped
            float xPos = View.Layer.Position.X + delta.X;

            float viewHalfWidth = ( View.Layer.Bounds.Width / 2 );

            xPos = Math.Max( viewHalfWidth, Math.Min( xPos, PrimaryContainerConfig.SlideAmount + viewHalfWidth ) );

            View.Layer.Position = new PointF( xPos, View.Layer.Position.Y );

            float percentDark = Math.Max( 0, Math.Min( (xPos - viewHalfWidth) / PrimaryContainerConfig.SlideAmount, PrimaryContainerConfig.SlideDarkenAmount ) );
            DarkPanel.Layer.Opacity = percentDark;
        }

        public void SpringboardRevealButtonTouchUp( )
        {
            // best practice states that we should let the view controller who presented us also dismiss us.
            // however, we have a unique situation where we are the parent to ALL OTHER view controllers,
            // so managing ourselves becomes a lot simpler.
            RevealSpringboard( !SpringboardRevealed );
        }

        public bool ActivateTask( Task task )
        {
            // don't allow switching activites while we're animating.
            if( Animating == false )
            {
                Container.ActivateTask( task );

                PopToRootViewController( false );

                RevealSpringboard( false );

                return true;
            }

            return false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // certain iOS controllers (here's looking at you MovieController) cause the device
            // orientation to change without notifying us when they finish being fullscreen.
            // We WILL receive a WillAppear callback tho, so let's make sure the springboard state is sync'd
            // with the given orientation.
            UpdateSpringboardAllowedState( UIApplication.SharedApplication.StatusBarOrientation );
        }

        public void OnActivated( )
        {
        }

        public void WillEnterForeground( )
        {

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
                        new NSAction( 
                            delegate 
                            { 
                                float endPos = 0.0f;
                                if( wantReveal == true )
                                {
                                    endPos = PrimaryContainerConfig.SlideAmount + (View.Layer.Bounds.Width / 2);
                                    DarkPanel.Layer.Opacity = PrimaryContainerConfig.SlideDarkenAmount;
                                }
                                else
                                {
                                    endPos = (View.Layer.Bounds.Width / 2);
                                    DarkPanel.Layer.Opacity = 0.0f;
                                }

                                float moveAmount = endPos - View.Layer.Position.X;
                                View.Layer.Position = new PointF( View.Layer.Position.X + moveAmount, View.Layer.Position.Y );
                            })

                        , new NSAction(
                            delegate
                            {
                                Animating = false;

                                SpringboardRevealed = wantReveal;

                                // if the springboard is open, disable input on app stuff
                                Container.View.UserInteractionEnabled = !SpringboardRevealed;
                            })
                    );
                }
            }
        }
	}
}
