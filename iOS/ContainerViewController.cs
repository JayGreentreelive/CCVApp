using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;

namespace iOS
{
	partial class ContainerViewController : UIViewController
	{
        Activity CurrentActivity { get; set; }

        public UINavigationController SubNavigationController { get; set; }

		public ContainerViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.Black;

            this.NavigationItem.SetLeftBarButtonItem
            (
                new UIBarButtonItem(UIBarButtonSystemItem.Action, (sender,args) => 
                {
                    (ParentViewController as MainUINavigationController).CheeseburgerTouchUp( );
                }), true
            );

            CreateSubNavBar( );
        }

        protected void CreateSubNavBar( )
        {
            SubNavigationController = Storyboard.InstantiateViewController( "SubNavController" ) as UINavigationController;

            SubNavigationController.NavigationBar.TintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0xFFFFFFFF );
            SubNavigationController.NavigationBar.BarTintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x1C1C1CFF );

            SubNavigationController.View.Frame = new RectangleF( 0, 
                NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height, 
                SubNavigationController.View.Frame.Width, 
                SubNavigationController.View.Frame.Height);
        }

        public void ActivateActivity( Activity activity )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.OnResignActive( );
            }

            CurrentActivity = activity;

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
                CurrentActivity.OnResignActive( );
            }
        }

        public void DidEnterBackground( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.DidEnterBackground( );
            }
        }

        public void WillTerminate( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.WillTerminate( );
            }
        }
	}
}
