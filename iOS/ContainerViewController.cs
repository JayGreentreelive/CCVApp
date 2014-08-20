using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;

namespace iOS
{
	partial class ContainerViewController : UIViewController
	{
        Activity _CurrentActivity;
        public Activity CurrentActivity { get { return _CurrentActivity; } }

        public UINavigationController SubNavigationController { get; set; }

		public ContainerViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.Black;

            UIButton button = ( (MainUINavigationController)NavigationController).CreateCheeseburgerButton( );
            button.TouchUpInside += (object sender, EventArgs e) => 
                {
                    (ParentViewController as MainUINavigationController).CheeseburgerTouchUp( );
                };
            this.NavigationItem.SetLeftBarButtonItem( new UIBarButtonItem( button ), true );

            CreateSubNavBar( );
        }

        protected void CreateSubNavBar( )
        {
            SubNavigationController = Storyboard.InstantiateViewController( "SubNavController" ) as UINavigationController;

            SubNavigationController.NavigationBar.TintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0xFFFFFFFF );
            SubNavigationController.NavigationBar.BarTintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x1C1C1CFF );
            SubNavigationController.NavigationBar.SetBackgroundImage( null, UIBarMetrics.Default );

            SubNavigationController.View.Frame = new RectangleF( 0, 
                NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height, 
                SubNavigationController.View.Frame.Width, 
                SubNavigationController.View.Frame.Height);
        }

        public void ActivateActivity( Activity activity )
        {
            // reset our stack before changing activities
            SubNavigationController.PopToRootViewController( false );

            if( CurrentActivity != null )
            {
                CurrentActivity.MakeInActive( );
            }

            _CurrentActivity = activity;

            if( activity as AboutActivity != null )
            {
                AddChildViewController( SubNavigationController );
                View.AddSubview( SubNavigationController.View );

                CurrentActivity.MakeActive( SubNavigationController );
            }
            else
            {
                SubNavigationController.RemoveFromParentViewController( );
                SubNavigationController.View.RemoveFromSuperview( );

                CurrentActivity.MakeActive( this );
            }
        }

        public void OnResignActive()
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.AppOnResignActive( );
            }
        }

        public void DidEnterBackground( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.AppDidEnterBackground( );
            }
        }

        public void WillTerminate( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.AppWillTerminate( );
            }
        }
	}
}
