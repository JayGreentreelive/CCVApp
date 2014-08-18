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
        bool SpringboardRevealed { get; set; }

        ContainerViewController Container { get; set; }

		public MainUINavigationController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Container = ChildViewControllers[0] as ContainerViewController;
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

        public void PresentActivity( Activity activity )
        {
            Container.PresentActivity( activity );

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

        protected void RevealSpringboard( bool revealed )
        {
            // only do something if there's a change
            if( revealed != SpringboardRevealed )
            {
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
                            SpringboardRevealed = revealed;
                        })
                );
            }
        }
	}
}
