using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;

namespace iOS
{
	partial class MainUINavigationController : UINavigationController
	{
        const float SLIDE_AMOUNT = 250.0f;
        protected bool SpringboardRevealed { get; set; }
        protected bool Animating { get; set; }

        public ContainerViewController Container { get; set; }

		public MainUINavigationController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //TODO: Allow these to be set in data
            NavigationBar.TintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0xFFFFFFFF );
            NavigationBar.BarTintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x1C1C1CFF );

            string imagePath = NSBundle.MainBundle.BundlePath + "/ccvLogo.png";

            UIImage logo = new UIImage( imagePath );
            UIImageView imageView = new UIImageView( logo );

            NavigationBar.TopItem.TitleView = imageView;


            Container = ChildViewControllers[0] as ContainerViewController;

            // setup a shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( View.Bounds );
            View.Layer.MasksToBounds = false;
            View.Layer.ShadowColor = UIColor.Black.CGColor;
            View.Layer.ShadowOffset = new SizeF( 0.0f, 5.0f );
            View.Layer.ShadowOpacity = .5f;
            View.Layer.ShadowPath = shadowPath.CGPath;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public void CheeseburgerTouchUp( )
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
                    UIView.Animate( .50f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        new NSAction( 
                            delegate 
                            { 
                                float deltaPosition = revealed ? SLIDE_AMOUNT : -SLIDE_AMOUNT;

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
