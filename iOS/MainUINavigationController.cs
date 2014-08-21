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
        /// A wrapper for Container.CurrentActivity, since Container is protected.
        /// </summary>
        /// <value>The current activity.</value>
        public Activity CurrentActivity { get { return Container != null ? Container.CurrentActivity : null; } }

		public MainUINavigationController (IntPtr handle) : base (handle)
		{
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
            // before the activity displays don't cause a flash
            View.BackgroundColor = UIColor.Black;

            // setup the style of the nav bar
            NavigationBar.TintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.PrimaryNavBar.TextColor );
            NavigationBar.BarTintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.PrimaryNavBar.BackgroundColor );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + CCVApp.Config.PrimaryNavBar.LogoFile;
            NavigationBar.SetBackgroundImage( new UIImage( imagePath ), UIBarMetrics.Default );

            // our first (and only) child IS a ContainerViewController.
            Container = ChildViewControllers[0] as ContainerViewController;
            if( Container == null ) throw new InvalidCastException( String.Format( "MainUINavigationController's first child must be a ContainerViewController.") );

            // setup a shadow that provides depth when this panel is slid "out" from the springboard.
            UIBezierPath shadowPath = UIBezierPath.FromRect( View.Bounds );
            View.Layer.MasksToBounds = false;
            View.Layer.ShadowColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.PrimaryContainer.ShadowColor ).CGColor;
            View.Layer.ShadowOffset = CCVApp.Config.PrimaryContainer.ShadowOffset;
            View.Layer.ShadowOpacity = CCVApp.Config.PrimaryContainer.ShadowOpacity;
            View.Layer.ShadowPath = shadowPath.CGPath;
        }

        public void SpringboardRevealButtonTouchUp( )
        {
            // best practice states that we should let the view controller who presented us also dismiss us.
            // however, we have a unique situation where we are the parent to ALL OTHER view controllers,
            // so managing ourselves becomes a lot simpler.
            RevealSpringboard( !SpringboardRevealed );
        }

        public void ActivateActivity( Activity activity )
        {
            Container.ActivateActivity( activity );

            PopToRootViewController( false );

            RevealSpringboard( false );
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

        public void RevealSpringboard( bool revealed )
        {
            // only do something if there's a change
            if( revealed != SpringboardRevealed )
            {
                // of course don't allow a change while we're animating it.
                if( Animating == false )
                {
                    Animating = true;

                    // Animate the front panel out
                    UIView.Animate( CCVApp.Config.PrimaryContainer.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        new NSAction( 
                            delegate 
                            { 
                                float deltaPosition = revealed ? CCVApp.Config.PrimaryContainer.SlideAmount : -CCVApp.Config.PrimaryContainer.SlideAmount;

                                View.Layer.Position = new PointF( View.Layer.Position.X + deltaPosition, View.Layer.Position.Y ); 
                            })

                        , new NSAction(
                            delegate
                            {
                                Animating = false;

                                SpringboardRevealed = revealed;

                                // if the springboard is open, disable input on app stuff
                                View.UserInteractionEnabled = !SpringboardRevealed;
                            })
                    );
                }
            }
        }
	}
}
