using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;

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

        /// <summary>
        /// A wrapper for Container.CurrentTask, since Container is protected.
        /// </summary>
        /// <value>The current task.</value>
        public Task CurrentTask { get { return Container != null ? Container.CurrentTask : null; } }

		public MainUINavigationController (IntPtr handle) : base (handle)
		{
        }

        public void EnableSpringboardRevealButton( bool enabled )
        {
            Container.EnableSpringboardRevealButton( enabled );

            if( enabled == false )
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

            // setup the style of the nav bar
            NavigationBar.TintColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryNavBar.RevealButton_DepressedColor );
            NavigationBar.BarTintColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryNavBar.BackgroundColor );


            // our first (and only) child IS a ContainerViewController.
            Container = ChildViewControllers[0] as ContainerViewController;
            if( Container == null ) throw new InvalidCastException( String.Format( "MainUINavigationController's first child must be a ContainerViewController.") );

            // setup a shadow that provides depth when this panel is slid "out" from the springboard.
            UIBezierPath shadowPath = UIBezierPath.FromRect( View.Bounds );
            View.Layer.MasksToBounds = false;
            View.Layer.ShadowColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryContainer.ShadowColor ).CGColor;
            View.Layer.ShadowOffset = CCVApp.Shared.Config.PrimaryContainer.ShadowOffset;
            View.Layer.ShadowOpacity = CCVApp.Shared.Config.PrimaryContainer.ShadowOpacity;
            View.Layer.ShadowPath = shadowPath.CGPath;

            // setup our pan gesture
            UIPanGestureRecognizer panGesture = new UIPanGestureRecognizer( OnPanGesture );
            panGesture.MinimumNumberOfTouches = 1;
            panGesture.MaximumNumberOfTouches = 1;
            View.AddGestureRecognizer( panGesture );
        }

        /// <summary>
        /// Tracks the last position of panning so delta can be applied
        /// </summary>
        PointF PanLastPos { get; set; }

        /// <summary>
        /// Direction we're currently panning. Important for syncing the card positions
        /// </summary>
        int PanDir { get; set; }

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
                    float toggleThreshold = (CCVApp.Shared.Config.PrimaryContainer.SlideAmount / 3);

                    // check whether the springboard is open, because that changes the
                    // context of hte user's intention
                    if( SpringboardRevealed == true )
                    {
                        // since it's open, close it if it crosses the closeThreshold
                        // OR velocty is high
                        float closeThreshold = CCVApp.Shared.Config.PrimaryContainer.SlideAmount - toggleThreshold;
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

        public void TryPanSpringboard( PointF delta )
        {
            // make sure the springboard is clamped
            float xPos = View.Layer.Position.X + delta.X;

            xPos = Math.Max( (View.Layer.Bounds.Width / 2), Math.Min( xPos, CCVApp.Shared.Config.PrimaryContainer.SlideAmount + (View.Layer.Bounds.Width / 2) ) );

            View.Layer.Position = new PointF( xPos, View.Layer.Position.Y );
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
                    UIView.Animate( CCVApp.Shared.Config.PrimaryContainer.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        new NSAction( 
                            delegate 
                            { 
                                float endPos = 0.0f;
                                if( wantReveal == true )
                                {
                                    endPos = CCVApp.Shared.Config.PrimaryContainer.SlideAmount + (View.Layer.Bounds.Width / 2);
                                }
                                else
                                {
                                    endPos = (View.Layer.Bounds.Width / 2);
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
